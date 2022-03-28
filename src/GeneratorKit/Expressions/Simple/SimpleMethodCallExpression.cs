using System;
using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Expressions.Simple;

internal sealed class SimpleMethodCallExpression : IMethodCallExpression
{
  public SimpleMethodCallExpression(MethodInfo method, IExpression? @object, IEnumerable<IExpression> arguments)
  {
    Method = method;
    Object = @object;
    Arguments = arguments;
  }

  public IEnumerable<IExpression> Arguments { get; }

  public MethodInfo Method { get; }

  public IExpression? Object { get; }

  public Type Type => Method.ReturnType;
}
