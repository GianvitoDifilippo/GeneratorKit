using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratorKit.Expressions.Operations;

internal sealed class AnonymousFunctionOperationExpression : ILambdaExpression
{
  private readonly IExpressionContext _context;
  private readonly IAnonymousFunctionOperation _operation;

  internal AnonymousFunctionOperationExpression(IExpressionContext context, IAnonymousFunctionOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public IExpression Body => _context.GetExpression(_operation.Body);

  public IEnumerable<IParameterExpression> Parameters => _operation.Symbol.Parameters.Select(_context.GetParameter);

  public Type ReturnType => _context.GetType(_operation.Symbol.ReturnType);

  public Type Type => _context.GetDelegateType(_operation.Symbol);
}
