#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace GeneratorKit.Interpret;

internal class OperationToLambdaVisitor : OperationVisitor<Unit, Expression>
{
  private static readonly MethodInfo s_environmentGetMethod;

  static OperationToLambdaVisitor()
  {
    s_environmentGetMethod = typeof(Environment).GetMethod(
      nameof(Environment.Get),
      BindingFlags.Public | BindingFlags.Instance,
      null,
      new Type[] { typeof(ISymbol) },
      null) ?? throw new MissingMethodException(nameof(Environment), nameof(Environment.Get));
  }

  private readonly GeneratorRuntime _runtime;
  private readonly Environment _closure;
  private readonly ParameterExpression[] _parameters;
  private readonly Dictionary<IParameterSymbol, ParameterExpression> _parameterMap;

  public OperationToLambdaVisitor(GeneratorRuntime runtime, Environment closure, IMethodSymbol method)
  {
    _runtime = runtime;
    _closure = closure;
    ImmutableArray<IParameterSymbol> parameterSymbols = method.Parameters;
    _parameters = parameterSymbols.Map(CreateParameterExpression);
    _parameterMap = new Dictionary<IParameterSymbol, ParameterExpression>(parameterSymbols.Length, SymbolEqualityComparer.Default);
    for (int i = 0; i < parameterSymbols.Length; i++)
    {
      _parameterMap.Add(parameterSymbols[i], _parameters[i]);
    }
  }

  public LambdaExpression CreateLambda(IBlockOperation body)
  {
    Expression bodyExpr = VisitBlock(body, Unit.Instance)!;
    return Expression.Lambda(bodyExpr, _parameters);
  }

  public override Expression? DefaultVisit(IOperation operation, Unit argument)
  {
    throw new NotSupportedException("Cannot convert operation into expression.");
  }

  public override Expression? VisitArgument(IArgumentOperation operation, Unit argument)
  {
    return operation.ArgumentKind switch
    {
      ArgumentKind.Explicit => operation.Value.Accept(this, argument),
      _                     => throw new NotSupportedException()
    };
  }

  public override Expression? VisitBinaryOperator(IBinaryOperation operation, Unit argument)
  {
    Expression leftOperand = operation.LeftOperand.Accept(this, argument)!;
    Expression rightOperand = operation.RightOperand.Accept(this, argument)!;
    MethodInfo? method = operation.OperatorMethod is not null
      ? _runtime.CreateMethodInfoDelegator(operation.OperatorMethod)
      : null;
    
    //TODO: Support lifted operators
    return operation.OperatorKind switch
    {
      BinaryOperatorKind.Add                => Expression.Add(leftOperand, rightOperand, method),
      BinaryOperatorKind.Subtract           => Expression.Subtract(leftOperand, rightOperand, method),
      BinaryOperatorKind.Multiply           => Expression.Multiply(leftOperand, rightOperand, method),
      BinaryOperatorKind.Divide             => Expression.Divide(leftOperand, rightOperand, method),
      BinaryOperatorKind.Remainder          => Expression.Modulo(leftOperand, rightOperand, method),
      BinaryOperatorKind.LeftShift          => Expression.LeftShift(leftOperand, rightOperand, method),
      BinaryOperatorKind.RightShift         => Expression.RightShift(leftOperand, rightOperand, method),
      BinaryOperatorKind.And                => Expression.And(leftOperand, rightOperand, method),
      BinaryOperatorKind.Or                 => Expression.Or(leftOperand, rightOperand, method),
      BinaryOperatorKind.ExclusiveOr        => Expression.ExclusiveOr(leftOperand, rightOperand, method),
      BinaryOperatorKind.ConditionalAnd     => Expression.AndAlso(leftOperand, rightOperand, method),
      BinaryOperatorKind.ConditionalOr      => Expression.OrElse(leftOperand, rightOperand, method),
      BinaryOperatorKind.Equals             => Expression.Equal(leftOperand, rightOperand, false, method),
      BinaryOperatorKind.NotEquals          => Expression.NotEqual(leftOperand, rightOperand, false, method),
      BinaryOperatorKind.LessThan           => Expression.LessThan(leftOperand, rightOperand, false, method),
      BinaryOperatorKind.LessThanOrEqual    => Expression.LessThanOrEqual(leftOperand, rightOperand, false, method),
      BinaryOperatorKind.GreaterThanOrEqual => Expression.GreaterThanOrEqual(leftOperand, rightOperand, false, method),
      BinaryOperatorKind.GreaterThan        => Expression.GreaterThan(leftOperand, rightOperand, false, method),
      _                                     => throw Errors.Unreacheable
    };
  }

  public override Expression? VisitBlock(IBlockOperation operation, Unit argument)
  {
    return operation.Operations[0].Accept(this, argument);
  }

  public override Expression? VisitConversion(IConversionOperation operation, Unit argument)
  {
    Expression operand = operation.Operand.Accept(this, argument)!;
    Type type = _runtime.CreateTypeDelegator(operation.Type!).UnderlyingSystemType;
    MethodInfo? method = operation.OperatorMethod is null ? null : _runtime.CreateMethodInfoDelegator(operation.OperatorMethod).UnderlyingSystemMethod;

    return Expression.Convert(operand, type, method);
  }

  public override Expression? VisitFieldReference(IFieldReferenceOperation operation, Unit argument)
  {
    Expression? instance = operation.Field.IsStatic ? null : operation.Instance!.Accept(this, argument)!;
    FieldInfo field = _runtime.CreateFieldInfoDelegator(operation.Field).UnderlyingSystemField;

    return Expression.Field(instance, field);
  }

  public override Expression? VisitInvocation(IInvocationOperation operation, Unit argument)
  {
    Expression? instance = operation.TargetMethod.IsStatic ? null : operation.Instance!.Accept(this, argument)!;
    MethodInfo method = _runtime.CreateMethodInfoDelegator(operation.TargetMethod).UnderlyingSystemMethod;
    Expression[] arguments = operation.Arguments.Map(arg => arg.Accept(this, argument)!);

    return Expression.Call(instance, method, arguments);
  }

  public override Expression? VisitLiteral(ILiteralOperation operation, Unit argument)
  {
    Type type = _runtime.CreateTypeDelegator(operation.Type!).UnderlyingSystemType;
    return Expression.Constant(operation.ConstantValue.Value, type);
  }

  public override Expression? VisitLocalReference(ILocalReferenceOperation operation, Unit argument)
  {
    Expression closure = Expression.Constant(_closure, typeof(Environment));
    Expression symbol = Expression.Constant(operation.Local, typeof(ISymbol));

    Type type = _runtime.CreateTypeDelegator(operation.Type!).UnderlyingSystemType;
    Expression local = Expression.Call(closure, s_environmentGetMethod, symbol);

    return Expression.Convert(local, type);
  }

  public override Expression? VisitParameterReference(IParameterReferenceOperation operation, Unit argument)
  {
    return _parameterMap[operation.Parameter];
  }

  public override Expression? VisitPropertyReference(IPropertyReferenceOperation operation, Unit argument)
  {
    Expression? instance = operation.Property.IsStatic ? null : operation.Instance!.Accept(this, argument)!;
    PropertyInfo property = _runtime.CreatePropertyInfoDelegator(operation.Property).UnderlyingSystemProperty;

    if (operation.Property.IsIndexer)
    {
      Expression[] arguments = operation.Arguments.Map(arg => arg.Accept(this, argument)!);
      return Expression.Property(instance, property, arguments);
    }
    else
    {
      return Expression.Property(instance, property);
    }
  }

  public override Expression? VisitReturn(IReturnOperation operation, Unit argument)
  {
    return operation.ReturnedValue!.Accept(this, argument);
  }


  // Private members

  private ParameterExpression CreateParameterExpression(IParameterSymbol parameter)
  {
    Type type = _runtime.CreateTypeDelegator(parameter.Type).UnderlyingSystemType;
    return Expression.Parameter(type, parameter.Name);
  }
}