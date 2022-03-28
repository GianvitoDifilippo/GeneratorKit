using System.Reflection;

namespace GeneratorKit.Expressions;

public interface IMemberExpression : IExpression
{
  IExpression? Expression { get; }
  MemberInfo Member { get; }
}
