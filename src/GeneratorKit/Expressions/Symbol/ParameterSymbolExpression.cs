using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Expressions.Symbol;

internal sealed class ParameterSymbolExpression : IParameterExpression
{
  private readonly IExpressionContext _context;
  private readonly IParameterSymbol _symbol;

  internal ParameterSymbolExpression(IExpressionContext context, IParameterSymbol symbol)
  {
    _context = context;
    _symbol = symbol;
  }

  public bool IsByRef => _symbol.RefKind is not RefKind.None;

  public string Name => _symbol.Name;

  public Type Type => _context.GetType(_symbol.Type);
}
