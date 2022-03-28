using System;
using System.Linq.Expressions;

namespace GeneratorKit.Expressions;

internal class LambdaExpressionWrapper : Expression
{
  public LambdaExpressionWrapper(ILambdaExpression lambdaExpression)
  {
    Expression = lambdaExpression;
  }

  public ILambdaExpression Expression { get; }

  public override bool CanReduce => false;

  public override ExpressionType NodeType => (ExpressionType)(-1);

  public override Type Type => typeof(void);

  protected override Expression Accept(ExpressionVisitor visitor)
  {
    throw new NotSupportedException();
  }

  public override Expression Reduce()
  {
    throw new NotSupportedException();
  }

  protected override Expression VisitChildren(ExpressionVisitor visitor)
  {
    throw new NotSupportedException();
  }
}
