using GeneratorKit.Interpret.Context;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GeneratorKit.Interpret.Frame;

internal class ExpressionFrame
{
  private readonly ExpressionFrame? _enclosing;
  private readonly Dictionary<IParameterSymbol, ParameterExpression> _parameters;

  public ExpressionFrame(IInterpreterContext context, IMethodSymbol method)
  {
    _parameters = method.Parameters.ToDictionary(
      keySelector: p => p,
      elementSelector: p => CreateParameterExpression(context, p),
      comparer: (IEqualityComparer<IParameterSymbol>)SymbolEqualityComparer.Default);
  }

  public ExpressionFrame(ExpressionFrame enclosing, IInterpreterContext context, IMethodSymbol method)
    : this(context, method)
  {
    _enclosing = enclosing;
  }

  public ParameterExpression GetParameter(IParameterSymbol symbol)
  {
    if (_parameters.TryGetValue(symbol, out ParameterExpression? parameter))
    {
      return parameter;
    }
    if (_enclosing is not null)
    {
      return _enclosing.GetParameter(symbol);
    }

    throw new InvalidOperationException($"Parameter '{symbol.Name}' does not exist in the current context.");
  }

  public IEnumerable<ParameterExpression> GetParameters()
  {
    return _parameters.Values;
  }

  private ParameterExpression CreateParameterExpression(IInterpreterContext context, IParameterSymbol parameter)
  {
    Type type = context.GetType(parameter.Type);
    return Expression.Parameter(type, parameter.Name);
  }
}