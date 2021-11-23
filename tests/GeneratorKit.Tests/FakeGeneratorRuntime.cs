using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace GeneratorKit;

#pragma warning disable RS1024 // Compare symbols correctly

internal class FakeGeneratorRuntime : GeneratorRuntime
{
  private readonly Dictionary<ITypeSymbol, Type> _typeMap;

  public FakeGeneratorRuntime(Compilation compilation)
    : base(compilation)
  {
    _typeMap = new Dictionary<ITypeSymbol, Type>(SymbolEqualityComparer.Default);
  }

  public override Type? GetRuntimeType(SymbolType type)
  {
    return _typeMap.GetValueOrDefault(type.Symbol);
  }

  public void AddType(ITypeSymbol symbol, Type type)
  {
    _typeMap[symbol] = type;
  }
}