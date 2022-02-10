using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Reflection;

internal interface IRuntime
{
  Compilation Compilation { get; }
  Type GetRuntimeType(SymbolType type);

  object InvokeConstructor(SymbolConstructorInfo constructor, object?[] arguments);
  object? InvokeMethod(SymbolMethodInfo method, object? instance, object?[] arguments);
  object? GetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments);
  void SetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments, object? value);
  object? GetField(SymbolFieldInfo field, object? instance);
  void SetField(SymbolFieldInfo field, object? instance, object? value);
}
