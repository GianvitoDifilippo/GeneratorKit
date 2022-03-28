using System.Reflection;

namespace GeneratorKit.Expressions;

internal interface IUnaryExpression : IExpression
{
  bool IsLifted { get; }
  bool IsLiftedToNull { get; }
  MethodInfo? Method { get; }
  IExpression Operand { get; }
}
