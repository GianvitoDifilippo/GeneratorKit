using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Reflection;

internal sealed class SymbolParameterType : SymbolType
{
  private readonly ITypeParameterSymbol _symbol;

  public SymbolParameterType(IGeneratorRuntime runtime, Compilation compilation, ITypeParameterSymbol symbol)
    : base(runtime, compilation)
  {
    _symbol = symbol;
  }

  public override ITypeSymbol Symbol => _symbol;
}