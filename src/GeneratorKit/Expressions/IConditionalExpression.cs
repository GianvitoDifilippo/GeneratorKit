namespace GeneratorKit.Expressions;

public interface IConditionalExpression : IExpression
{
  IExpression IfFalse { get; }
  IExpression IfTrue { get; }
  IExpression Test { get; }
}
