namespace GeneratorKit.Expressions;

public interface IConstantExpression : IExpression
{
  object? Value { get; }
}