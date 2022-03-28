using Microsoft.CodeAnalysis.Operations;
using System;

namespace GeneratorKit.Expressions.Operations;

internal class ConditionalOperationExpression : IConditionalExpression
{
  private readonly IExpressionContext _context;
  private readonly IConditionalOperation _operation;

  public ConditionalOperationExpression(IExpressionContext context, IConditionalOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public IExpression IfFalse => _context.GetExpression(_operation.WhenFalse!);

  public IExpression IfTrue => _context.GetExpression(_operation.WhenTrue);

  public IExpression Test => _context.GetExpression(_operation.Condition);

  public Type Type => _context.GetType(_operation.Type!);
}
