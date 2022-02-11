using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit;

internal abstract class GeneratorContext : IGeneratorContext
{
  public GeneratorContext(IReflectionRuntime runtime)
  {
    Runtime = runtime;
  }

  protected IReflectionRuntime Runtime { get; }

  public abstract Type GetContextType(ITypeSymbol symbol);
  public abstract Type GetContextType(ITypeParameterSymbol symbol);
  public abstract SymbolType GetGenericTypeDefinition(SymbolNamedType type);
  public abstract SymbolType MakeGenericType(SymbolNamedType type, Type[] typeArguments);
  public abstract SymbolMethodInfo GetGenericMethodDefinition(SymbolMethodInfo method);
  public abstract SymbolMethodInfo MakeGenericMethod(SymbolMethodInfo method, Type[] typeArguments, SymbolType? reflectedType);
  public abstract SymbolType GetDeclaringType(SymbolMethodInfo method);

  public virtual bool IsGenericTypeDefinition(INamedTypeSymbol symbol)
  {
    if (!symbol.IsGenericType)
      return false;

    foreach (ITypeSymbol typeArgument in symbol.TypeArguments)
    {
      if (typeArgument.Kind is not SymbolKind.TypeParameter)
        return false;

      if (!GetContextType((ITypeParameterSymbol)typeArgument).IsGenericParameter)
        return false;
    }

    return true;
  }

  public virtual bool ContainsGenericParameters(INamedTypeSymbol symbol)
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

  public virtual bool IsGenericMethodDefinition(IMethodSymbol symbol)
  {
    if (!symbol.IsGenericMethod)
      return false;

    foreach (ITypeSymbol typeArgument in symbol.TypeArguments)
    {
      if (typeArgument.Kind is not SymbolKind.TypeParameter)
        return false;

      if (!GetContextType((ITypeParameterSymbol)typeArgument).IsGenericParameter)
        return false;
    }

    return true;
  }

  public virtual bool ContainsGenericParameters(IMethodSymbol symbol)
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

  public virtual SymbolAssembly CreateAssemblyDelegator(IAssemblySymbol symbol)
  {
    return new SymbolAssembly(this, symbol, null);
  }

  public virtual SymbolConstructorInfo CreateConstructorInfoDelegator(IMethodSymbol symbol)
  {
    return new SymbolConstructorInfo(Runtime, this, symbol);
  }

  public virtual SymbolEventInfo CreateEventInfoDelegator(IEventSymbol symbol, SymbolType? reflectedType = null)
  {
    return new SymbolEventInfo(Runtime, this, symbol, reflectedType);
  }

  public virtual SymbolFieldInfo CreateFieldInfoDelegator(IFieldSymbol symbol, SymbolType? reflectedType = null)
  {
    return new SymbolFieldInfo(Runtime, this, symbol, reflectedType);
  }

  public virtual SymbolMethodInfo CreateMethodInfoDelegator(IMethodSymbol symbol, SymbolType? reflectedType = null)
  {
    return new SymbolMethodInfo(Runtime, this, symbol, reflectedType);
  }

  public virtual SymbolModule CreateModuleDelegator(IModuleSymbol symbol)
  {
    return new SymbolModule(this, symbol);
  }

  public virtual SymbolPropertyInfo CreatePropertyInfoDelegator(IPropertySymbol symbol, SymbolType? reflectedType = null)
  {
    return new SymbolPropertyInfo(Runtime, this, symbol, reflectedType);
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

  protected SymbolNamedType CreateTypeDelegator(INamedTypeSymbol symbol)
  {
    return new SymbolNamedType(Runtime, this, symbol);
  }

  protected SymbolArrayType CreateTypeDelegator(IArrayTypeSymbol symbol)
  {
    return new SymbolArrayType(Runtime, this, symbol);
  }

  protected SymbolTypeParameter CreateTypeDelegator(ITypeParameterSymbol symbol)
  {
    return new SymbolTypeParameter(Runtime, this, symbol);
  }
}
