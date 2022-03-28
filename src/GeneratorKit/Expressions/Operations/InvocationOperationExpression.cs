using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Expressions.Operations;

internal sealed class InvocationOperationExpression : IMethodCallExpression
{
  private readonly IExpressionContext _context;
  private readonly IInvocationOperation _operation;

  internal InvocationOperationExpression(IExpressionContext context, IInvocationOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public IEnumerable<IExpression> Arguments => _operation.Arguments.Select(_context.GetExpression);
  
  public MethodInfo Method => _context.GetMethodInfo(_operation.TargetMethod);
  
  public IExpression? Object => _operation.Instance is { } instance
    ? _context.GetExpression(instance)
    : null;

  public Type Type => _context.GetType(_operation.TargetMethod.ReturnType);
}
