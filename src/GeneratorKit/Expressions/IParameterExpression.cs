namespace GeneratorKit.Expressions;

public interface IParameterExpression : IExpression
{
  bool IsByRef { get; }
  string Name { get; }
}
