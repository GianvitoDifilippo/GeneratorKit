using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;

namespace GeneratorKit.Reflection.Context;

internal class GenericTypeContext : GeneratorContext
{
  private readonly GeneratorContext _parent;
  private readonly Type[] _typeArguments;

  public GenericTypeContext(GeneratorContext parent, Type[] typeArguments)
  {
    _parent = parent;
    _typeArguments = typeArguments;
  }

  public override GeneratorContext Root => _parent;

  public override object CreateInstance(Type type, object?[] arguments)
  {
    throw new NotSupportedException();
  }

  public override Type GetContextType(ITypeParameterSymbol symbol)
  {
    return symbol.TypeParameterKind is TypeParameterKind.Type
      ? _typeArguments[symbol.Ordinal]
      : CreateTypeDelegator(symbol);
  }

  public override SymbolNamedType GetGenericTypeDefinition(SymbolNamedType type)
  {
    SymbolNamedType result = _parent.CreateTypeDelegator(type.Symbol.ConstructedFrom);
    Debug.Assert(result.IsGenericTypeDefinition, "Failed to create a generic type definition for the current context.");
    return result;
  }

  public override SymbolNamedType MakeGenericType(SymbolNamedType type, Type[] typeArguments)
  {
    GenericTypeContext context = new GenericTypeContext(_parent, typeArguments);
    return context.CreateTypeDelegator(type.Symbol);
  }

  public override SymbolMethodInfo GetGenericMethodDefinition(SymbolMethodInfo method)
  {
    SymbolMethodInfo result = CreateMethodInfoDelegator(method.Symbol.ConstructedFrom);
    Debug.Assert(result.IsGenericMethodDefinition, "Failed to create a generic method definition for the current context.");
    return result;
  }

  public override SymbolMethodInfo MakeGenericMethod(SymbolMethodInfo method, Type[] typeArguments, SymbolType? reflectedType)
  {
    GenericMethodContext context = new GenericMethodContext(this, typeArguments);
    return context.CreateMethodInfoDelegator(method.Symbol, reflectedType);
  }

  public override SymbolMethodInfo GetBaseDefinition(SymbolMethodInfo method, SymbolType? reflectedType)
  {
    IMethodSymbol symbol = method.Symbol;
    if (symbol.IsOverride)
    {
      IMethodSymbol overriddenMethod = method.IsGenericMethod && !method.IsGenericMethodDefinition
        ? symbol.ConstructedFrom.OverriddenMethod!
        : symbol.OverriddenMethod!;
      INamedTypeSymbol containingType = overriddenMethod.ContainingType;
      GeneratorContext context = containingType.IsGenericType
        ? new GenericTypeContext(_parent, containingType.TypeArguments.Map(GetContextType))
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

  #region Implemented through parent

  public override Compilation Compilation => _parent.Compilation;

  public override bool IsGenericMethodDefinition(IMethodSymbol symbol)
  {
    return _parent.IsGenericMethodDefinition(symbol);
  }

  public override bool ContainsGenericParameters(IMethodSymbol symbol)
  {
    return _parent.ContainsGenericParameters(symbol);
  }

  public override Type GetRuntimeType(SymbolType type)
  {
    return _parent.GetRuntimeType(type);
  }

  public override object InvokeConstructor(SymbolConstructorInfo constructor, object?[] arguments)
  {
    return _parent.InvokeConstructor(constructor, arguments);
  }

  public override object? InvokeMethod(SymbolMethodInfo method, object? instance, object?[] arguments)
  {
    return _parent.InvokeMethod(method, instance, arguments);
  }

  public override object? GetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments)
  {
    return _parent.GetProperty(property, instance, arguments);
  }

  public override void SetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments, object? value)
  {
    _parent.SetProperty(property, instance, arguments, value);
  }

  public override object? GetField(SymbolFieldInfo field, object? instance)
  {
    return _parent.GetField(field, instance);
  }

  public override void SetField(SymbolFieldInfo field, object? instance, object? value)
  {
    _parent.SetField(field, instance, value);
  }

  #endregion
}
