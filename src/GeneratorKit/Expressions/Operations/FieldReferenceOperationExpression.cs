using Microsoft.CodeAnalysis.Operations;
using System;
using System.Reflection;

namespace GeneratorKit.Expressions.Operations;

internal sealed class FieldReferenceOperationExpression : IMemberExpression
{
  private readonly IExpressionContext _context;
  private readonly IFieldReferenceOperation _operation;

  public FieldReferenceOperationExpression(IExpressionContext context, IFieldReferenceOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public IExpression? Expression => _operation.Instance is { } instance
    ? _context.GetExpression(instance)
    : null;

  public MemberInfo Member => _context.GetFieldInfo(_operation.Field);

  public Type Type => _context.GetType(_operation.Field.Type);
}
