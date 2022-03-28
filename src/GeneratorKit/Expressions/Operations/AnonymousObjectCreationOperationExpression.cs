using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Expressions.Operations;

internal class AnonymousObjectCreationOperationExpression : INewExpression
{
  private readonly IExpressionContext _context;
  private readonly IAnonymousObjectCreationOperation _operation;

  public AnonymousObjectCreationOperationExpression(IExpressionContext context, IAnonymousObjectCreationOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public IEnumerable<IExpression> Arguments => throw new NotSupportedException();

  public ConstructorInfo? Constructor => throw new NotSupportedException();

  public IEnumerable<MemberInfo> Members => throw new NotSupportedException();

  public Type Type => _context.GetType(_operation.Type!);
}
