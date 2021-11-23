using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal class SymbolParameterType : TypeDelegator
{
  private readonly IParameterSymbol _symbol;

  public SymbolParameterType(Type type, IParameterSymbol symbol)
    : base(type)
  {
    _symbol = symbol;
  }

  protected override bool IsByRefImpl()
  {
    return _symbol.RefKind is not RefKind.None;
  }
}