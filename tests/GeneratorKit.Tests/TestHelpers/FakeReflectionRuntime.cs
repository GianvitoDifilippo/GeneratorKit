using GeneratorKit.Comparers;
using GeneratorKit.Reflection;
using GeneratorKit.Reflection.Context;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace GeneratorKit.TestHelpers;

internal class FakeReflectionRuntime : IReflectionRuntime
{
  private readonly Dictionary<Type, Type> _typeMap;

  public FakeReflectionRuntime(Compilation compilation)
  {
    _typeMap = new Dictionary<Type, Type>(TypeEqualityComparer.Default);
    Compilation = compilation;
  }

  public Compilation Compilation { get; }

  public void AddType(Type key, Type value)
  {
    _typeMap[key] = value;
  }

  public void AddType(INamedTypeSymbol symbol, Type value)
  {
    _typeMap[new SymbolNamedType(this, new DefaultGeneratorContext(this), symbol)] = value;
  }

  public Type GetRuntimeType(SymbolType type)
  {
    if (type.AssemblyQualifiedName is string name && Type.GetType(name) is Type result)
      return result;

    return _typeMap[type];
  }

  public object? GetField(SymbolFieldInfo field, object? instance) => throw new NotImplementedException();
  public object? GetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments) => throw new NotImplementedException();
  public object InvokeConstructor(SymbolConstructorInfo constructor, object?[] arguments) => throw new NotImplementedException();
  public object? InvokeMethod(SymbolMethodInfo method, object? instance, object?[] arguments) => throw new NotImplementedException();
  public void SetField(SymbolFieldInfo field, object? instance, object? value) => throw new NotImplementedException();
  public void SetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments, object? value) => throw new NotImplementedException();
}