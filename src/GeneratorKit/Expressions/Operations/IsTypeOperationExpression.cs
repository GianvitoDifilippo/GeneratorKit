using Microsoft.CodeAnalysis.Operations;
using System;

namespace GeneratorKit.Expressions.Operations;

internal class IsTypeOperationExpression : ITypeBinaryExpression
{
  private readonly IExpressionContext _context;
  private readonly IIsTypeOperation _operation;

  public IsTypeOperationExpression(IExpressionContext context, IIsTypeOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public IExpression Expression => _context.GetExpression(_operation.ValueOperand);

  public Type TypeOperand => _context.GetType(_operation.TypeOperand);

  public Type Type => typeof(bool);
}
