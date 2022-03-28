namespace GeneratorKit.Expressions;

internal interface IListInitExpression : IExpression
{
  // IEnumerable<ElementInit> Initializers { get; }
  INewExpression NewExpression { get; }
}
