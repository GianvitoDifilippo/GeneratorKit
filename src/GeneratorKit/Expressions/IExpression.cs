using System;

namespace GeneratorKit.Expressions;

public interface IExpression
{
  Type Type { get; }
}
