using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Reflection;

internal sealed class SymbolNamedType : SymbolType
{
  private readonly INamedTypeSymbol _symbol;

  public SymbolNamedType(IGeneratorRuntime runtime, Compilation compilation, INamedTypeSymbol symbol)
    : base(runtime, compilation)
  {
    _symbol = symbol;
  }

  public override ITypeSymbol Symbol => _symbol;
}