using GeneratorKit.Comparers;
using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GeneratorKit.TestHelpers;

internal class FakeGeneratorRuntime : GeneratorRuntime
{
  private readonly Dictionary<Type, Type> _typeMap;

  public FakeGeneratorRuntime(Compilation compilation)
    : base(compilation)
  {
    _typeMap = new Dictionary<Type, Type>(TypeEqualityComparer.Default);
  }

  public override SymbolAssembly CompilationAssembly => throw new NotImplementedException();

  public override CancellationToken CancellationToken => CancellationToken.None;

  public override Type GetRuntimeType(IRuntimeType type)
  {
    if (type.AssemblyQualifiedName is string name && Type.GetType(name) is Type result)
      return result;

    return _typeMap[(Type)type];
  }

  public void AddType(ITypeSymbol symbol, Type value)
  {
    SymbolType symbolType = CreateTypeDelegator(symbol);
    _typeMap[symbolType] = value;
  }

  public void AddType(Type key, Type value)
  {
    _typeMap[key] = value;
  }

  public override T CreateInstance<T>(Type type, params object?[] arguments) => throw new NotImplementedException();
  public override object InvokeConstructor(IRuntimeConstructor constructor, object?[] arguments) => throw new NotImplementedException();
  public override object? InvokeMethod(IRuntimeMethod method, object? instance, object?[] arguments) => throw new NotImplementedException();
  public override object? GetProperty(IRuntimeProperty property, object? instance, object?[] arguments) => throw new NotImplementedException();
  public override void SetProperty(IRuntimeProperty property, object? instance, object?[] arguments, object? value) => throw new NotImplementedException();
  public override object? GetField(IRuntimeField field, object? instance) => throw new NotImplementedException();
  public override void SetField(IRuntimeField field, object? instance, object? value) => throw new NotImplementedException();
}