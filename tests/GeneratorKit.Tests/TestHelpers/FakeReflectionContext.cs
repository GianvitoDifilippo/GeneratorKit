using GeneratorKit.Comparers;
using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace GeneratorKit.TestHelpers;

internal class FakeReflectionContext : GeneratorContext
{
  private readonly Dictionary<Type, Type> _typeMap;

  public FakeReflectionContext(Compilation compilation)
  {
    _typeMap = new Dictionary<Type, Type>(TypeEqualityComparer.Default);
    Compilation = compilation;
  }

  public override Compilation Compilation { get; }

  public void AddType(Type key, Type value)
  {
    _typeMap[key] = value;
  }

  public void AddType(INamedTypeSymbol symbol, Type value)
  {
    _typeMap[new SymbolNamedType(this, symbol)] = value;
  }

  public override Type GetRuntimeType(SymbolType type)
  {
    if (type.AssemblyQualifiedName is string name && Type.GetType(name) is Type result)
      return result;

    return _typeMap[type];
  }

  #region Not implemented
  public override object CreateInstance(Type type, object?[] arguments) => throw new NotImplementedException();
  public override object? GetField(SymbolFieldInfo field, object? instance) => throw new NotImplementedException();
  public override object? GetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments) => throw new NotImplementedException();
  public override object InvokeConstructor(SymbolConstructorInfo constructor, object?[] arguments) => throw new NotImplementedException();
  public override object? InvokeMethod(SymbolMethodInfo method, object? instance, object?[] arguments) => throw new NotImplementedException();
  public override void SetField(SymbolFieldInfo field, object? instance, object? value) => throw new NotImplementedException();
  public override void SetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments, object? value) => throw new NotImplementedException();
  #endregion
}