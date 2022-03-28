using Microsoft.CodeAnalysis.Operations;
using System;

namespace GeneratorKit.Expressions.Operations;

public sealed class LiteralOperationExpression : IConstantExpression
{
  private readonly IExpressionContext _context;
  private readonly ILiteralOperation _operation;

  internal LiteralOperationExpression(IExpressionContext context, ILiteralOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public object? Value => _operation.ConstantValue.Value;

  public Type Type => _operation.Type is { } type
    ? _context.GetType(type)
    : typeof(object);
}
