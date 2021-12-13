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

internal class OperationToLambdaVisitor : OperationVisitor<object?, Expression>
{
  private static readonly MethodInfo s_environmentGetMethod;
  private static readonly MethodInfo s_createDelegateMethod;

  static OperationToLambdaVisitor()
  {
    s_environmentGetMethod = typeof(Environment).GetMethod(
      nameof(Environment.Get),
      BindingFlags.Public | BindingFlags.Instance,
      null,
      new Type[] { typeof(ISymbol) },
      null) ?? throw new MissingMethodException(nameof(Environment), nameof(Environment.Get));

    s_createDelegateMethod = typeof(MethodInfo).GetMethod(
      nameof(MethodInfo.CreateDelegate),
      BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
      null,
      new Type[] { typeof(Type), typeof(object) },
      null) ?? throw new MissingMethodException(nameof(MethodInfo), nameof(MethodInfo.CreateDelegate));
  }

  private readonly GeneratorRuntime _runtime;
  private readonly Environment _closure;
  private readonly ExpressionFrame _frame;

  public OperationToLambdaVisitor(GeneratorRuntime runtime, Environment closure, IMethodSymbol method)
  {
    _runtime = runtime;
    _closure = closure;
    _frame = new ExpressionFrame(runtime, method);
  }

  private OperationToLambdaVisitor(GeneratorRuntime runtime, Environment closure, IMethodSymbol method, ExpressionFrame frame)
  {
    _runtime = runtime;
    _closure = closure;
    _frame = new ExpressionFrame(frame, runtime, method);
  }

  public LambdaExpression CreateLambda(IBlockOperation body)
  {
    Expression bodyExpr = VisitBlock(body, null)!;
    return Expression.Lambda(bodyExpr, _frame.GetParameters());
  }

  public override Expression? DefaultVisit(IOperation operation, object? argument)
  {
    throw new NotSupportedException("Cannot convert operation into expression.");
  }

  public override Expression? VisitAnonymousFunction(IAnonymousFunctionOperation operation, object? argument)
  {
    return new OperationToLambdaVisitor(_runtime, _closure, operation.Symbol, _frame).CreateLambda(operation.Body);
  }

  public override Expression? VisitArgument(IArgumentOperation operation, object? argument)
  {
    return operation.ArgumentKind switch
    {
      ArgumentKind.Explicit => operation.Value.Accept(this, null),
      _                     => throw new NotSupportedException()
    };
  }

  public override Expression? VisitArrayCreation(IArrayCreationOperation operation, object? argument)
  {
    Type elementType = _runtime.CreateTypeDelegator(operation.Type!).GetElementType()!.UnderlyingSystemType;
    Expression size = operation.DimensionSizes[0].Accept(this, null)!;
    return operation.Initializer is null
      ? Expression.NewArrayBounds(elementType, size)
      : VisitArrayInitializer(operation.Initializer, elementType);
  }

  public override Expression? VisitArrayInitializer(IArrayInitializerOperation operation, object? argument)
  {
    Expression[] initializers = operation.ElementValues.Map(el => el.Accept(this, null)!);
    return Expression.NewArrayInit((Type)argument!, initializers);
  }

  public override Expression? VisitBinaryOperator(IBinaryOperation operation, object? argument)
  {
    Expression leftOperand = operation.LeftOperand.Accept(this, null)!;
    Expression rightOperand = operation.RightOperand.Accept(this, null)!;
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

  public override Expression? VisitBlock(IBlockOperation operation, object? argument)
  {
    return operation.Operations[0].Accept(this, null);
  }

  public override Expression? VisitConversion(IConversionOperation operation, object? argument)
  {
    Expression operand = operation.Operand.Accept(this, null)!;
    Type type = _runtime.CreateTypeDelegator(operation.Type!).UnderlyingSystemType;
    MethodInfo? method = operation.OperatorMethod is null ? null : _runtime.CreateMethodInfoDelegator(operation.OperatorMethod).UnderlyingSystemMethod;

    return Expression.Convert(operand, type, method);
  }

  public override Expression? VisitDelegateCreation(IDelegateCreationOperation operation, object? argument)
  {
    return operation.Target.Accept(this, null);
  }

  public override Expression? VisitFieldReference(IFieldReferenceOperation operation, object? argument)
  {
    Expression? instance = operation.Field.IsStatic ? null : operation.Instance!.Accept(this, null)!;
    FieldInfo field = _runtime.CreateFieldInfoDelegator(operation.Field).UnderlyingSystemField;

    return Expression.Field(instance, field);
  }

  public override Expression? VisitInstanceReference(IInstanceReferenceOperation operation, object? argument)
  {
    object? instance = operation.ReferenceKind switch
    {
      InstanceReferenceKind.ContainingTypeInstance => _closure.ContainingTypeInstance,
      _                                            => throw new NotImplementedException()
    };

    return instance is not null
      ? Expression.Constant(instance)
      : null;
  }

  public override Expression? VisitInvocation(IInvocationOperation operation, object? argument)
  {
    Expression? instance = operation.TargetMethod.IsStatic ? null : operation.Instance!.Accept(this, null)!;
    MethodInfo method = _runtime.CreateMethodInfoDelegator(operation.TargetMethod).UnderlyingSystemMethod;
    Expression[] arguments = operation.Arguments.Map(arg => arg.Accept(this, null)!);

    return Expression.Call(instance, method, null);
  }

  public override Expression? VisitLiteral(ILiteralOperation operation, object? argument)
  {
    if (operation.Type is null)
    {
      return Expression.Constant(null);
    }
    Type type = _runtime.CreateTypeDelegator(operation.Type).UnderlyingSystemType;
    return Expression.Constant(operation.ConstantValue.Value, type);
  }

  public override Expression? VisitLocalReference(ILocalReferenceOperation operation, object? argument)
  {
    Expression closure = Expression.Constant(_closure, typeof(Environment));
    Expression symbol = Expression.Constant(operation.Local, typeof(ISymbol));

    Type type = _runtime.CreateTypeDelegator(operation.Type!).UnderlyingSystemType;
    Expression local = Expression.Call(closure, s_environmentGetMethod, symbol);

    return Expression.Convert(local, type);
  }

  public override Expression? VisitMethodReference(IMethodReferenceOperation operation, object? argument)
  {
    MethodInfo method = _runtime.CreateMethodInfoDelegator(operation.Method).UnderlyingSystemMethod;
    Expression methodExpr = Expression.Constant(method, typeof(MethodInfo));

    Type delegateType = DelegateHelper.GetDelegateType(_runtime, operation.Method);
    Expression delegateTypeExpr = Expression.Constant(delegateType, typeof(Type));
    Expression instance = operation.Method.IsStatic
      ? Expression.Constant(null, typeof(object))
      : operation.Instance!.Accept(this, null)!;

    Expression createDelegate = Expression.Call(methodExpr, s_createDelegateMethod, delegateTypeExpr, instance);
    return Expression.Convert(createDelegate, delegateType);
  }

  public override Expression? VisitParameterReference(IParameterReferenceOperation operation, object? argument)
  {
    return _frame.GetParameter(operation.Parameter);
  }

  public override Expression? VisitPropertyReference(IPropertyReferenceOperation operation, object? argument)
  {
    Expression? instance = operation.Property.IsStatic ? null : operation.Instance!.Accept(this, null)!;
    PropertyInfo property = _runtime.CreatePropertyInfoDelegator(operation.Property).UnderlyingSystemProperty;

    if (operation.Property.IsIndexer)
    {
      Expression[] arguments = operation.Arguments.Map(arg => arg.Accept(this, null)!);
      return Expression.Property(instance, property, null);
    }
    else
    {
      return Expression.Property(instance, property);
    }
  }

  public override Expression? VisitReturn(IReturnOperation operation, object? argument)
  {
    return operation.ReturnedValue!.Accept(this, null);
  }
}