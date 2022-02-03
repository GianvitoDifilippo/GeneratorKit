using GeneratorKit.Comparers;
using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
  public override object? InvokeGetter(IRuntimeProperty property, object? instance, object?[] arguments) => throw new NotImplementedException();
  public override void InvokeSetter(IRuntimeProperty property, object? instance, object?[] arguments, object? value) => throw new NotImplementedException();
}