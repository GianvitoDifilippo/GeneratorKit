using Microsoft.CodeAnalysis.Operations;
using System;
using System.Reflection;

namespace GeneratorKit.Expressions.Operations;

internal sealed class PropertyReferenceOperationExpression : IMemberExpression
{
  private readonly IExpressionContext _context;
  private readonly IPropertyReferenceOperation _operation;

  public PropertyReferenceOperationExpression(IExpressionContext context, IPropertyReferenceOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public IExpression? Expression => _operation.Instance is { } instance
    ? _context.GetExpression(instance)
    : null;

  public MemberInfo Member => _context.GetPropertyInfo(_operation.Property);

  public Type Type => _context.GetType(_operation.Property.Type);
}