using System;

namespace GeneratorKit.Expressions.Simple;

internal class SimpleConstantExpression : IConstantExpression
{
  public SimpleConstantExpression(object value, Type type)
  {
    Value = value;
    Type = type;
  }

  public object? Value { get; }

  public Type Type { get; }
}
