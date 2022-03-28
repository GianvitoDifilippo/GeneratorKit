using System;
using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Expressions;

public interface IIndexExpression : IExpression
{
  public IEnumerable<IExpression> Arguments { get; }
  public PropertyInfo? Indexer { get; }
  public IExpression Object { get; }
}
