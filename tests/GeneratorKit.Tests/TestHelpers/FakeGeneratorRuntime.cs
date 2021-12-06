#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;

namespace GeneratorKit.TestHelpers;

internal class FakeGeneratorRuntime : GeneratorRuntime
{
  private readonly Dictionary<ITypeSymbol, Type> _typeMap;
  private readonly Dictionary<Type, ITypeSymbol> _symbolMap;

  public FakeGeneratorRuntime(Compilation compilation)
    : base(compilation)
  {
    _typeMap = new Dictionary<ITypeSymbol, Type>(SymbolEqualityComparer.Default);
    _symbolMap = new Dictionary<Type, ITypeSymbol>();
  }

  public override SymbolAssembly CompilationAssembly => throw new NotImplementedException();

  public override CancellationToken CancellationToken => CancellationToken.None;

  public override Type? GetRuntimeType(SymbolType type)
  {
    return _typeMap.GetValueOrDefault(type.Symbol);
  }

  public override ITypeSymbol? GetTypeSymbol(Type type)
  {
    if (type is SymbolType symbolType)
      return symbolType.Symbol;

    return _symbolMap.GetValueOrDefault(type);
  }

  public void AddType(ITypeSymbol symbol, Type type)
  {
    _typeMap[symbol] = type;
    _symbolMap[type] = symbol;
  }
}