using GeneratorKit.Reflection;
using GeneratorKit.Reflection.Context;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace GeneratorKit;

internal abstract class GeneratorContext : IReflectionContext
{
  public abstract Compilation Compilation { get; }
  public abstract GeneratorContext Root { get; }

  public abstract object CreateInstance(Type type, object?[] arguments);

  public abstract Type GetRuntimeType(SymbolType type);
  public abstract object InvokeConstructor(SymbolConstructorInfo constructor, object?[] arguments);
  public abstract object? InvokeMethod(SymbolMethodInfo method, object? instance, object?[] arguments);
  public abstract object? GetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments);
  public abstract void SetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments, object? value);
  public abstract object? GetField(SymbolFieldInfo field, object? instance);
  public abstract void SetField(SymbolFieldInfo field, object? instance, object? value);

  public virtual SymbolAssembly CreateAssemblyDelegator(IAssemblySymbol symbol)
  {
    return new SymbolAssembly(this, symbol, null);
  }

  public virtual SymbolConstructorInfo CreateConstructorInfoDelegator(IMethodSymbol symbol)
  {
    return new SymbolConstructorInfo(this, symbol);
  }

  public virtual SymbolFieldInfo CreateFieldInfoDelegator(IFieldSymbol symbol, SymbolType? reflectedType = null)
  {
    return new SymbolFieldInfo(this, symbol, reflectedType);
  }

  public virtual SymbolMethodInfo CreateMethodInfoDelegator(IMethodSymbol symbol, SymbolType? reflectedType = null)
  {
    return new SymbolMethodInfo(this, symbol, reflectedType);
  }

  public virtual SymbolModule CreateModuleDelegator(IModuleSymbol symbol)
  {
    return new SymbolModule(this, symbol);
  }

  public virtual SymbolPropertyInfo CreatePropertyInfoDelegator(IPropertySymbol symbol, SymbolType? reflectedType = null)
  {
    return new SymbolPropertyInfo(this, symbol, reflectedType);
  }

  public virtual SymbolType CreateTypeDelegator(ITypeSymbol symbol)
  {
    return symbol.Kind switch
    {
      SymbolKind.NamedType     => CreateTypeDelegator((INamedTypeSymbol)symbol),
      SymbolKind.ArrayType     => CreateTypeDelegator((IArrayTypeSymbol)symbol),
      SymbolKind.TypeParameter => CreateTypeDelegator((ITypeParameterSymbol)symbol),
      _                        => throw new NotSupportedException($"Symbol of kind {symbol.Kind} is not supported.")
    };
  }

  public SymbolNamedType CreateTypeDelegator(INamedTypeSymbol symbol)
  {
    return new SymbolNamedType(this, symbol);
  }

  public SymbolArrayType CreateTypeDelegator(IArrayTypeSymbol symbol)
  {
    return new SymbolArrayType(this, symbol);
  }

  public SymbolTypeParameter CreateTypeDelegator(ITypeParameterSymbol symbol)
  {
    return new SymbolTypeParameter(this, symbol);
  }

  public virtual SymbolNamedType GetGenericTypeDefinition(SymbolNamedType type)
  {
    SymbolNamedType result = CreateTypeDelegator(type.Symbol.ConstructedFrom);
    Debug.Assert(result.IsGenericTypeDefinition, "Failed to create a generic type definition for the current context.");
    return result;
  }

  public virtual SymbolNamedType MakeGenericType(SymbolNamedType type, Type[] typeArguments)
  {
    GenericTypeContext context = new GenericTypeContext(this, typeArguments);
    return context.CreateTypeDelegator(type.Symbol);
  }

  public virtual SymbolMethodInfo GetGenericMethodDefinition(SymbolMethodInfo method)
  {
    SymbolMethodInfo result = CreateMethodInfoDelegator(method.Symbol.ConstructedFrom, null);
    Debug.Assert(result.IsGenericMethodDefinition, "Failed to create a generic method definition for the current context.");
    return result;
  }

  public virtual SymbolMethodInfo MakeGenericMethod(SymbolMethodInfo method, Type[] typeArguments, SymbolType? reflectedType)
  {
    GenericMethodContext context = new GenericMethodContext(this, typeArguments);
    return context.CreateMethodInfoDelegator(method.Symbol, reflectedType);
  }

  public virtual SymbolMethodInfo GetBaseDefinition(SymbolMethodInfo method, SymbolType? reflectedType)
  {
    IMethodSymbol symbol = method.Symbol;
    if (symbol.IsOverride)
    {
      IMethodSymbol overriddenMethod = method.IsGenericMethod && !method.IsGenericMethodDefinition
        ? symbol.ConstructedFrom.OverriddenMethod!
        : symbol.OverriddenMethod!;
      INamedTypeSymbol containingType = overriddenMethod.ContainingType;
      GeneratorContext context = containingType.IsGenericType
        ? new GenericTypeContext(this, containingType.TypeArguments.Map(GetContextType))
        : this;

      return context.CreateMethodInfoDelegator(overriddenMethod);
    }
    else
    {
      return symbol.IsVirtual && reflectedType is not null
        ? CreateMethodInfoDelegator(symbol)
        : method;
    }
  }

  public virtual SymbolNamedType GetDeclaringType(SymbolMethodInfo method)
  {
    return CreateTypeDelegator(method.Symbol.ContainingType);
  }

  public virtual bool IsGenericTypeDefinition(INamedTypeSymbol symbol) // TODO: Override in GeneratorRuntime
  {
    if (!symbol.IsGenericType)
      return false;

    ImmutableArray<ITypeParameterSymbol> typeParameters = symbol.TypeParameters;
    ImmutableArray<ITypeSymbol> typeArguments = symbol.TypeArguments;

    for (int i = 0; i < typeParameters.Length; i++)
    {
      ITypeParameterSymbol typeParameter = typeParameters[i];
      if (!typeParameter.Equals(typeArguments[i], SymbolEqualityComparer.Default))
        return false;

      if (!GetContextType(typeParameter).IsGenericParameter)
        return false;
    }

    return true;
  }

  public virtual bool ContainsGenericParameters(INamedTypeSymbol symbol) // TODO: Override in GeneratorRuntime
  {
    if (!symbol.IsGenericType)
      return false;

    foreach (ITypeSymbol typeArgument in symbol.TypeArguments)
    {
      switch (typeArgument.Kind)
      {
        case SymbolKind.TypeParameter:
          if (GetContextType((ITypeParameterSymbol)typeArgument).ContainsGenericParameters)
            return true;
          break;
        case SymbolKind.NamedType:
          if (ContainsGenericParameters((INamedTypeSymbol)typeArgument))
            return true;
          break;
        case SymbolKind.ArrayType:
          if (ContainsGenericParameters((IArrayTypeSymbol)typeArgument))
            return true;
          break;
      }
    }

    return false;
  }

  public virtual bool IsGenericMethodDefinition(IMethodSymbol symbol) // TODO: Override in GeneratorRuntime
  {
    if (!symbol.IsGenericMethod)
      return false;

    ImmutableArray<ITypeParameterSymbol> typeParameters = symbol.TypeParameters;
    ImmutableArray<ITypeSymbol> typeArguments = symbol.TypeArguments;

    for (int i = 0; i < typeParameters.Length; i++)
    {
      ITypeParameterSymbol typeParameter = typeParameters[i];
      if (!typeParameter.Equals(typeArguments[i], SymbolEqualityComparer.Default))
        return false;

      if (!GetContextType(typeParameter).IsGenericParameter)
        return false;
    }

    return true;
  }

  public virtual bool ContainsGenericParameters(IMethodSymbol symbol) // TODO: Override in GeneratorRuntime
  {
    if (symbol.ContainingType is not null && ContainsGenericParameters(symbol.ContainingType))
      return true;
    if (!symbol.IsGenericMethod)
      return false;

    foreach (ITypeSymbol typeArgument in symbol.TypeArguments)
    {
      switch (typeArgument.Kind)
      {
        case SymbolKind.TypeParameter:
          if (GetContextType((ITypeParameterSymbol)typeArgument).ContainsGenericParameters)
            return true;
          break;
        case SymbolKind.NamedType:
          if (ContainsGenericParameters((INamedTypeSymbol)typeArgument))
            return true;
          break;
        case SymbolKind.ArrayType:
          if (ContainsGenericParameters((IArrayTypeSymbol)typeArgument))
            return true;
          break;
      }
    }

    return false;
  }

  public bool ContainsGenericParameters(IArrayTypeSymbol symbol)
  {
    ITypeSymbol elementType = symbol.ElementType;
    return elementType.Kind switch
    {
      SymbolKind.NamedType     => ContainsGenericParameters((INamedTypeSymbol)elementType),
      SymbolKind.ArrayType     => ContainsGenericParameters((IArrayTypeSymbol)elementType),
      SymbolKind.TypeParameter => true,
      _                        => throw new NotSupportedException()
    };
  }

  public virtual Type GetContextType(ITypeSymbol symbol)
  {
    return symbol.Kind is SymbolKind.TypeParameter
      ? GetContextType((ITypeParameterSymbol)symbol)
      : CreateTypeDelegator(symbol);
  }

  public virtual Type GetContextType(ITypeParameterSymbol symbol)
  {
    return CreateTypeDelegator(symbol);
  }
}
