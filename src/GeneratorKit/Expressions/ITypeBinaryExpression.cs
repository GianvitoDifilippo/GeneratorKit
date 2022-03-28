using System;

namespace GeneratorKit.Expressions;

internal interface ITypeBinaryExpression : IExpression
{
  IExpression Expression { get; }
  Type TypeOperand { get; }
}
