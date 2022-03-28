using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Expressions.Operations;

internal class ObjectCreationOperationExpression : INewExpression
{
  private readonly IExpressionContext _context;
  private readonly IObjectCreationOperation _operation;

  public ObjectCreationOperationExpression(IExpressionContext context, IObjectCreationOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public IEnumerable<IExpression> Arguments => _operation.Arguments.Select(_context.GetExpression);

  public ConstructorInfo? Constructor => _operation.Constructor is { } constructor
    ? _context.GetConstructorInfo(constructor)
    : null;

  public IEnumerable<MemberInfo> Members => throw new NotImplementedException();

  public Type Type => _context.GetType(_operation.Type!);
}
