using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Reflection;

namespace GeneratorKit.Expressions.Operations;

internal class ConversionOperationExpression : IUnaryExpression
{
  private readonly IExpressionContext _context;
  private readonly IConversionOperation _operation;

  public ConversionOperationExpression(IExpressionContext context, IConversionOperation operation)
  {
    _context = context;
    _operation = operation;
  }

  public bool IsLifted
  {
    get
    {
      bool operandIsNullable = _context.IsNullableType(_operation.Operand.Type!);
      bool resultIsNullable = _context.IsNullableType(_operation.Type!);

      if (_operation.OperatorMethod is { } method)
      {
        return
          (operandIsNullable && !method.Parameters[0].Equals(_operation.Operand.Type, SymbolEqualityComparer.Default)) ||
          (resultIsNullable && !method.ReturnType.Equals(_operation.Type, SymbolEqualityComparer.Default));
      }

      return operandIsNullable || resultIsNullable;
    }
  }

  public bool IsLiftedToNull => IsLifted && _context.IsNullableType(_operation.Type!);

  public MethodInfo? Method => _operation.OperatorMethod is { } method
    ? _context.GetMethodInfo(method)
    : null;

  public IExpression Operand => _context.GetExpression(_operation.Operand);

  public Type Type => _context.GetType(_operation.Type!);
}
