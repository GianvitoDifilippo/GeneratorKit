using Microsoft.CodeAnalysis;

namespace GeneratorKit.Reflection;

internal sealed class SymbolArrayType : SymbolType
{
  private readonly IArrayTypeSymbol _symbol;

  public SymbolArrayType(IGeneratorRuntime runtime, Compilation compilation, IArrayTypeSymbol symbol)
    : base(runtime, compilation)
  {
    _symbol = symbol;
  }

  public override ITypeSymbol Symbol => _symbol;
}