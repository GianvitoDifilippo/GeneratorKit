using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Expressions;

public interface IMethodCallExpression : IExpression
{
  IEnumerable<IExpression> Arguments { get; }
  MethodInfo Method { get; }
  IExpression? Object { get; }
}
