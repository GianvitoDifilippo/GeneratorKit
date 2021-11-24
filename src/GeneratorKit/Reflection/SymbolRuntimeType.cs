using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Reflection;

internal sealed class SymbolRuntimeType : SymbolType
{
  private readonly ITypeSymbol _symbol;
  private readonly Type _runtimeType;

  public SymbolRuntimeType(GeneratorRuntime runtime, Compilation compilation, ITypeSymbol symbol, Type runtimeType)
    : base(runtime, compilation)
  {
    _symbol = symbol;
    _runtimeType = runtimeType;
  }

  public override ITypeSymbol Symbol => _symbol;
}
