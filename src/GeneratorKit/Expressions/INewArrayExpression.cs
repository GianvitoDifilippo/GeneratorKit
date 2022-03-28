using System.Collections.Generic;

namespace GeneratorKit.Expressions;

internal interface INewArrayExpression : IExpression
{
  IEnumerable<IExpression> Expressions { get; }
}
