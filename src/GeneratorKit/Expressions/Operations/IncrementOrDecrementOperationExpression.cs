using Microsoft.CodeAnalysis.Operations;
using System;
using System.Reflection;

namespace GeneratorKit.Expressions.Operations;

internal class IncrementOrDecrementOperationExpression : IUnaryExpression
{
  private readonly IExpressionContext _context;
  private readonly IIncrementOrDecrementOperation _operation;

  public IncrementOrDecrementOperationExpression(IExpressionContext context, IIncrementOrDecrementOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public bool IsLifted => _operation.IsLifted;

  public bool IsLiftedToNull => IsLifted && _context.IsNullableType(_operation.Type!);

  public MethodInfo? Method => _operation.OperatorMethod is { } method
    ? _context.GetMethodInfo(method)
    : null;

  public IExpression Operand => _context.GetExpression(_operation.Target);

  public Type Type => _context.GetType(_operation.Type!);
}
