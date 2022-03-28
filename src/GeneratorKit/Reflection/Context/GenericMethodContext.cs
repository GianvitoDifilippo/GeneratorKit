using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;

namespace GeneratorKit.Reflection.Context;

internal class GenericMethodContext : GeneratorContext
{
  private readonly GeneratorContext _parent;
  private readonly Type[] _typeArguments;

  public GenericMethodContext(GeneratorContext parent, Type[] typeArguments)
  {
    _parent = parent;
    _typeArguments = typeArguments;
  }

  public override GeneratorContext Root => _parent.Root;

  public override object CreateInstance(Type type, object?[] arguments)
  {
    throw new NotSupportedException();
  }

  public override Type GetContextType(ITypeParameterSymbol symbol)
  {
    return symbol.TypeParameterKind is TypeParameterKind.Method
      ? _typeArguments[symbol.Ordinal]
      : _parent.GetContextType(symbol);
  }

  public override SymbolNamedType GetGenericTypeDefinition(SymbolNamedType type)
  {
    throw new InvalidOperationException();
  }

  public override SymbolNamedType MakeGenericType(SymbolNamedType type, Type[] typeArguments)
  {
    throw new InvalidOperationException();
  }

  public override SymbolMethodInfo GetGenericMethodDefinition(SymbolMethodInfo method)
  {
    SymbolMethodInfo result = _parent.CreateMethodInfoDelegator(method.Symbol.ConstructedFrom);
    Debug.Assert(result.IsGenericMethodDefinition, "Failed to create a generic method definition for the current context.");
    return result;
  }

  public override SymbolMethodInfo MakeGenericMethod(SymbolMethodInfo method, Type[] typeArguments, SymbolType? reflectedType)
  {
    GenericMethodContext context = new GenericMethodContext(_parent, typeArguments);
    return context.CreateMethodInfoDelegator(method.Symbol, reflectedType);
  }

  public override SymbolNamedType GetDeclaringType(SymbolMethodInfo method)
  {
    return _parent.CreateTypeDelegator(method.Symbol.ContainingType);
  }

  #region Implemented through parent

  public override Compilation Compilation => _parent.Compilation;

  public override SymbolMethodInfo GetBaseDefinition(SymbolMethodInfo method, SymbolType? reflectedType)
  {
    return _parent.GetBaseDefinition(method, reflectedType);
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
