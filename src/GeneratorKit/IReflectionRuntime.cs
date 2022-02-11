using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit;

internal interface IReflectionRuntime
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
