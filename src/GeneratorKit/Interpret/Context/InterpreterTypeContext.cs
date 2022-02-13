using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;

namespace GeneratorKit.Interpret.Context;

internal class InterpreterTypeContext : InterpreterContext
{
  private readonly GeneratorContext _parent;
  private readonly Type[] _typeArguments;

  public InterpreterTypeContext(GeneratorContext parent, Type[] typeArguments)
  {
    _parent = parent;
    _typeArguments = typeArguments;
  }


  // IInterpreterContext

  public override void BeginLambdaContext()
  {
    throw new NotSupportedException();
  }

  public override void EndLambdaContext()
  {
    throw new NotSupportedException();
  }


  // GeneratorContext

  public override object CreateInstance(Type type, object?[] arguments)
  {
    return _parent.CreateInstance(type, arguments);
  }

  public override Type GetContextType(ITypeParameterSymbol symbol)
  {
    return symbol.TypeParameterKind is TypeParameterKind.Type
      ? _typeArguments[symbol.Ordinal]
      : throw new ArgumentException("TypeParameterKind must be Type.", nameof(symbol));
  }

  public override SymbolNamedType GetDeclaringType(SymbolMethodInfo method)
  {
    throw new NotSupportedException();
  }

  public override SymbolNamedType GetGenericTypeDefinition(SymbolNamedType type)
  {
    SymbolNamedType result = _parent.CreateTypeDelegator(type.Symbol.ConstructedFrom);
    Debug.Assert(result.IsGenericTypeDefinition, "Failed to create a generic type definition for the current context.");
    return result;
  }

  public override SymbolNamedType MakeGenericType(SymbolNamedType type, Type[] typeArguments)
  {
    throw new NotSupportedException();
  }

  public override SymbolMethodInfo GetGenericMethodDefinition(SymbolMethodInfo method)
  {
    throw new NotSupportedException();
  }

  public override SymbolMethodInfo MakeGenericMethod(SymbolMethodInfo method, Type[] typeArguments, SymbolType? reflectedType)
  {
    throw new NotSupportedException();
  }

  public override SymbolMethodInfo GetBaseDefinition(SymbolMethodInfo method, SymbolType? reflectedType)
  {
    throw new NotSupportedException();
  }

  #region Implemented through parent

  public override Compilation Compilation => _parent.Compilation;

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
