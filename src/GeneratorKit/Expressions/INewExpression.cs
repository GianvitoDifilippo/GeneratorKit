using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Expressions;

internal interface INewExpression : IExpression
{
  IEnumerable<IExpression> Arguments { get; }
  ConstructorInfo? Constructor { get; }
  IEnumerable<MemberInfo> Members { get; }
}
