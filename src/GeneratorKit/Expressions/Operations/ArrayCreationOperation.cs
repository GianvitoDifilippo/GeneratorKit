using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratorKit.Expressions.Operations;

internal class ArrayCreationOperation : INewArrayExpression
{
  private readonly IExpressionContext _context;
  private readonly IArrayCreationOperation _operation;

  public ArrayCreationOperation(IExpressionContext context, IArrayCreationOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public IEnumerable<IExpression> Expressions => _operation.Initializer is { } initializer
    ? initializer.ElementValues.Select(_context.GetExpression)
    : _operation.DimensionSizes.Select(_context.GetExpression);

  public Type Type => _context.GetType(_operation.Type!);
}
