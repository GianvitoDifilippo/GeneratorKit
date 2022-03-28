using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Expressions.Operations;

internal class ArrayElementReferenceOperationExpression : IIndexExpression
{
  private readonly IExpressionContext _context;
  private readonly IArrayElementReferenceOperation _operation;

  public ArrayElementReferenceOperationExpression(IExpressionContext context, IArrayElementReferenceOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public IEnumerable<IExpression> Arguments => _operation.Indices.Select(_context.GetExpression);

  public PropertyInfo? Indexer => null;

  public IExpression Object => _context.GetExpression(_operation.ArrayReference);

  public Type Type => _context.GetType(_operation.Type!);
}
