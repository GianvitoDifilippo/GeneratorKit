using System;
using System.Collections.Generic;

namespace GeneratorKit.Expressions;

public interface ILambdaExpression : IExpression
{
  IExpression Body { get; }
  IEnumerable<IParameterExpression> Parameters { get; }
  Type ReturnType { get; }
}
