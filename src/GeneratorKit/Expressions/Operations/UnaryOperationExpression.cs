using Microsoft.CodeAnalysis.Operations;
using System;
using System.Reflection;

namespace GeneratorKit.Expressions.Operations;

internal class UnaryOperationExpression : IUnaryExpression
{
  private readonly IExpressionContext _context;
  private readonly IUnaryOperation _operation;

  public UnaryOperationExpression(IExpressionContext context, IUnaryOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public bool IsLifted => _operation.IsLifted;

  public bool IsLiftedToNull => IsLifted && _context.IsNullableType(_operation.Type!);

  public MethodInfo? Method => _operation.OperatorMethod is { } method
    ? _context.GetMethodInfo(method)
    : null;

  public IExpression Operand => _context.GetExpression(_operation.Operand);

  public Type Type => _context.GetType(_operation.Type!);
}
