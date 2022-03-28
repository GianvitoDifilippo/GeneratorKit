using Microsoft.CodeAnalysis.Operations;
using System;
using System.Reflection;

namespace GeneratorKit.Expressions.Operations;

internal sealed class BinaryOperationExpression : IBinaryExpression
{
  private readonly IExpressionContext _context;
  private readonly IBinaryOperation _operation;

  public BinaryOperationExpression(IExpressionContext context, IBinaryOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public bool IsLifted => _operation.IsLifted;

  public bool IsLiftedToNull => IsLifted && _context.IsNullableType(_operation.Type!);

  public IExpression Left => _context.GetExpression(_operation.LeftOperand);

  public MethodInfo? Method => _operation.OperatorMethod is { } method
    ? _context.GetMethodInfo(method)
    : null;

  public IExpression Right => _context.GetExpression(_operation.RightOperand);

  public Type Type => _context.GetType(_operation.Type!);
}
