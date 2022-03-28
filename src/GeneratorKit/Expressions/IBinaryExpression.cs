using System.Reflection;

namespace GeneratorKit.Expressions;

public interface IBinaryExpression : IExpression
{
  bool IsLifted { get; }
  bool IsLiftedToNull { get; }
  IExpression Left { get; }
  MethodInfo? Method { get; }
  IExpression Right { get; }
}
