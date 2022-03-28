namespace GeneratorKit.Expressions;

internal interface IMemberInitExpression : IExpression
{
  // IEnumerable<MemberBinding> Bindings { get; }
  INewExpression NewExpression { get; }
}
