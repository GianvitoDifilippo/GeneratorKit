using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace GeneratorKit.Interpret;

internal class InterpreterVisitor : OperationVisitor<Optional<object?>, object>
{
  private readonly GeneratorRuntime _runtime;
  private readonly Stack<Environment> _environments;
  private readonly Stack<object> _implicitReceivers;
  private readonly Stack<object> _conditionalAccessInstances; // TODO: Maybe it can work with a single reference
  private Optional<object?> _returnValue;
  private bool _hitBreak;
  private bool _hitContinue;

  public InterpreterVisitor(GeneratorRuntime runtime, Environment environment)
  {
    _runtime = runtime;
    _environments = new Stack<Environment>(1);
    _environments.Push(environment);
    _implicitReceivers = new Stack<object>();
    _conditionalAccessInstances = new Stack<object>();
  }

  public override object? DefaultVisit(IOperation operation, Optional<object?> argument)
  {
    throw new NotSupportedException($"Operation of kind {operation.Kind} cannot be interpreted.");
  }

  public override object? VisitAnonymousFunction(IAnonymousFunctionOperation operation, Optional<object?> argument)
  {
    LambdaExpression lambda = new OperationToLambdaVisitor(_runtime, Environment, operation.Symbol).CreateLambda(operation.Body);

    if (operation.Parent is not IDelegateCreationOperation)
      return lambda;

    // TODO: Interpret the operation instead of compiling the lambda
    return lambda.Compile();
  }

  public override object? VisitArgument(IArgumentOperation operation, Optional<object?> argument)
  {
    return operation.ArgumentKind switch
    {
      ArgumentKind.Explicit => operation.Value.Accept(this, default),
      _                     => throw new NotImplementedException()
    };
  }

  public override object? VisitArrayCreation(IArrayCreationOperation operation, Optional<object?> argument)
  {
    int rank = operation.DimensionSizes.Length;
    int[] dimensionSizes = operation.DimensionSizes.Map(x => (int)x.Accept(this, default)!);
    Type elementType = _runtime.CreateTypeDelegator(operation.Type!).GetElementType()!.UnderlyingSystemType;
    Array array = Array.CreateInstance(elementType, dimensionSizes);
    int[] indices = new int[rank];

    if (operation.Initializer is IArrayInitializerOperation initializerOperation)
    {
      VisitArrayInitializer(initializerOperation, new ArrayInitializerContext(array, indices, 0));
    }
    return array;
  }

  public override object? VisitArrayElementReference(IArrayElementReferenceOperation operation, Optional<object?> argument)
  {
    Array array = (Array)operation.ArrayReference.Accept(this, default)!;

    if (operation.Indices.Length == 1)
    {
      object index = operation.Indices[0].Accept(this, default)!;
      if (index is int intIndex)
      {
        return array.GetValue(intIndex);
      }
      Range range = (Range)index;
      Type elementType = _runtime.CreateTypeDelegator(operation.Type!).GetElementType()!.UnderlyingSystemType;

      return array.GetSubArray(range, elementType);
    }

    int[] indices = operation.Indices.Map(index => (int)index.Accept(this, default)!);
    return array.GetValue(indices);
  }

  public override object? VisitArrayInitializer(IArrayInitializerOperation operation, Optional<object?> argument)
  {
    // TODO: Needs refactoring
    (Array array, int[] indices, int dimension) = (ArrayInitializerContext)argument.Value!;
    if (dimension == indices.Length - 1)
    {
      for (int i = 0; i < operation.ElementValues.Length; i++)
      {
        indices[dimension] = i;
        object? value = operation.ElementValues[i].Accept(this, default);
        array.SetValue(value, indices);
      }
    }
    else
    {
      for (int i = 0; i < operation.ElementValues.Length; i++)
      {
        indices[dimension] = i;
        operation.ElementValues[i].Accept(this, new ArrayInitializerContext(array, indices, dimension + 1));
      }
    }
    return null;
  }

  public override object? VisitBinaryOperator(IBinaryOperation operation, Optional<object?> argument)
  {
    object? leftOperand = operation.LeftOperand.Accept(this, default);
    object? rightOperand = operation.RightOperand.Accept(this, default);
    object? result;

    if (operation.OperatorMethod is not null)
    {
      MethodInfo method = _runtime.CreateMethodInfoDelegator(operation.OperatorMethod);

      result = method.Invoke(null, new object?[2] { leftOperand, rightOperand });
    }
    else
    {
      result = operation.OperatorKind switch
      {
        BinaryOperatorKind.Add                => BinaryOperations.Add(leftOperand, rightOperand),
        BinaryOperatorKind.Subtract           => BinaryOperations.Subtract(leftOperand, rightOperand),
        BinaryOperatorKind.Multiply           => BinaryOperations.Multiply(leftOperand, rightOperand),
        BinaryOperatorKind.Divide             => BinaryOperations.Divide(leftOperand, rightOperand),
        BinaryOperatorKind.Remainder          => BinaryOperations.Remainder(leftOperand, rightOperand),
        BinaryOperatorKind.LeftShift          => BinaryOperations.LeftShift(leftOperand, rightOperand),
        BinaryOperatorKind.RightShift         => BinaryOperations.RightShift(leftOperand, rightOperand),
        BinaryOperatorKind.And                => BinaryOperations.And(leftOperand, rightOperand),
        BinaryOperatorKind.Or                 => BinaryOperations.Or(leftOperand, rightOperand),
        BinaryOperatorKind.ExclusiveOr        => BinaryOperations.ExclusiveOr(leftOperand, rightOperand),
        BinaryOperatorKind.ConditionalAnd     => BinaryOperations.ConditionalAnd(leftOperand, rightOperand),
        BinaryOperatorKind.ConditionalOr      => BinaryOperations.ConditionalOr(leftOperand, rightOperand),
        BinaryOperatorKind.Equals             => BinaryOperations.Equals(leftOperand, rightOperand),
        BinaryOperatorKind.NotEquals          => BinaryOperations.NotEquals(leftOperand, rightOperand),
        BinaryOperatorKind.LessThan           => BinaryOperations.LessThan(leftOperand, rightOperand),
        BinaryOperatorKind.LessThanOrEqual    => BinaryOperations.LessThanOrEqual(leftOperand, rightOperand),
        BinaryOperatorKind.GreaterThanOrEqual => BinaryOperations.GreaterThanOrEqual(leftOperand, rightOperand),
        BinaryOperatorKind.GreaterThan        => BinaryOperations.GreaterThan(leftOperand, rightOperand),
        _                                     => throw Errors.Unreacheable
      };
    }

    return result;
  }

  public override object? VisitBlock(IBlockOperation operation, Optional<object?> argument)
  {
    foreach (IOperation statement in operation.Operations)
    {
      statement.Accept(this, default);
      if (_returnValue.HasValue || _hitBreak || _hitContinue)
      {
        break;
      }
    }

    return _returnValue.HasValue
      ? _returnValue.Value
      : Unit.Instance;
  }

  public override object? VisitBranch(IBranchOperation operation, Optional<object?> argument)
  {
    switch (operation.BranchKind)
    {
      case BranchKind.Continue:
        _hitContinue = true;
        break;
      case BranchKind.Break:
        _hitBreak = true;
        break;
      default:
        throw new NotImplementedException();
    }
    return null;
  }

  public override object? VisitCoalesce(ICoalesceOperation operation, Optional<object?> argument)
  {
    object? value = operation.Value.Accept(this, default);
    return value is not null
      ? value
      : operation.WhenNull.Accept(this, default);
  }

  public override object? VisitCoalesceAssignment(ICoalesceAssignmentOperation operation, Optional<object?> argument)
  {
    object? target = operation.Target.Accept(this, default);
    if (target is null)
    {
      object? value = operation.Value.Accept(this, default);
      operation.Target.Accept(this, value);
      return value;
    }
    return target;
  }

  public override object? VisitConditionalAccess(IConditionalAccessOperation operation, Optional<object?> argument)
  {
    object? instance = operation.Operation.Accept(this, default);
    if (instance is null)
      return null;

    BeginConditionalAccessInstance(instance);
    object? whenNotNull = operation.WhenNotNull.Accept(this, default);
    EndConditionalAccessInstance();

    return whenNotNull;
  }

  public override object? VisitConditionalAccessInstance(IConditionalAccessInstanceOperation operation, Optional<object?> argument)
  {
    return _conditionalAccessInstances.Peek();
  }

  public override object? VisitCompoundAssignment(ICompoundAssignmentOperation operation, Optional<object?> argument)
  {
    object? target = operation.Target.Accept(this, default);
    object? value = operation.Value.Accept(this, default);
    object? result;

    if (operation.OperatorMethod is not null)
    {
      MethodInfo method = _runtime.CreateMethodInfoDelegator(operation.OperatorMethod);

      result = method.Invoke(null, new object?[2] { target, value });
    }
    else
    {
      result = operation.OperatorKind switch
      {
        BinaryOperatorKind.Add                => BinaryOperations.Add(target, value),
        BinaryOperatorKind.Subtract           => BinaryOperations.Subtract(target, value),
        BinaryOperatorKind.Multiply           => BinaryOperations.Multiply(target, value),
        BinaryOperatorKind.Divide             => BinaryOperations.Divide(target, value),
        BinaryOperatorKind.Remainder          => BinaryOperations.Remainder(target, value),
        BinaryOperatorKind.LeftShift          => BinaryOperations.LeftShift(target, value),
        BinaryOperatorKind.RightShift         => BinaryOperations.RightShift(target, value),
        BinaryOperatorKind.And                => BinaryOperations.And(target, value),
        BinaryOperatorKind.Or                 => BinaryOperations.Or(target, value),
        BinaryOperatorKind.ExclusiveOr        => BinaryOperations.ExclusiveOr(target, value),
        BinaryOperatorKind.ConditionalAnd     => BinaryOperations.ConditionalAnd(target, value),
        BinaryOperatorKind.ConditionalOr      => BinaryOperations.ConditionalOr(target, value),
        BinaryOperatorKind.Equals             => BinaryOperations.Equals(target, value),
        BinaryOperatorKind.NotEquals          => BinaryOperations.NotEquals(target, value),
        BinaryOperatorKind.LessThan           => BinaryOperations.LessThan(target, value),
        BinaryOperatorKind.LessThanOrEqual    => BinaryOperations.LessThanOrEqual(target, value),
        BinaryOperatorKind.GreaterThanOrEqual => BinaryOperations.GreaterThanOrEqual(target, value),
        BinaryOperatorKind.GreaterThan        => BinaryOperations.GreaterThan(target, value),
        _                                     => throw Errors.Unreacheable
      };
    }

    return operation.Target.Accept(this, result);
  }

  public override object? VisitConditional(IConditionalOperation operation, Optional<object?> argument)
  {
    bool condition = (bool)operation.Condition.Accept(this, default)!;
    object? result = null;
    if (condition)
    {
      BeginScope();
      result = operation.WhenTrue.Accept(this, default);
      EndScope();
      return result;
    }
    else if (operation.WhenFalse is not null)
    {
      BeginScope();
      result = operation.WhenFalse.Accept(this, default);
      EndScope();
      return result;
    }
    return result;
  }

  public override object? VisitConstructorBodyOperation(IConstructorBodyOperation operation, Optional<object?> argument)
  {
    IBlockOperation body = operation.BlockBody ?? operation.ExpressionBody ?? throw new ArgumentException("Constructor body operation did not have a body.", nameof(operation));

    return VisitBlock(body, default);
  }

  public override object? VisitConversion(IConversionOperation operation, Optional<object?> argument)
  {
    object? operand = operation.Operand.Accept(this, default);

    CommonConversion conversion = operation.Conversion;
    if (conversion.IsIdentity)
    {
      return operand;
    }
    if (conversion.IsUserDefined)
    {
      MethodInfo method = _runtime.CreateMethodInfoDelegator(operation.OperatorMethod!);

      return method.Invoke(null, new object?[1] { operand });
    }
    if (conversion.IsImplicit && operation.Type!.ContainingNamespace.Name == "System")
    {
      switch (operation.Type!.Name)
      {
        case "Boolean":
          return Convert.ToBoolean(operand);
        case "Char":   
          return Convert.ToChar(operand);
        case "Byte":   
          return Convert.ToByte(operand);
        case "SByte":  
          return Convert.ToSByte(operand);
        case "Int16":  
          return Convert.ToInt16(operand);
        case "UInt16": 
          return Convert.ToUInt16(operand);
        case "Int32":  
          return Convert.ToInt32(operand);
        case "UInt32": 
          return Convert.ToUInt32(operand);
        case "Int64":  
          return Convert.ToInt64(operand);
        case "UInt64": 
          return Convert.ToUInt64(operand);
        case "Single": 
          return Convert.ToSingle(operand);
        case "Double": 
          return Convert.ToDouble(operand);
        case "Decimal":
          return Convert.ToDecimal(operand);
      }
    }

    if (operand is null)
      return null;

    Type type = _runtime.CreateTypeDelegator(operation.Type!);
    Type operandType = operand.GetType();

    if (!type.IsAssignableFrom(operandType))
      throw new InvalidCastException($"Unable to cast object of type '{operandType}' to type '{type}'.");

    return operand;
  }

  public override object? VisitDefaultValue(IDefaultValueOperation operation, Optional<object?> argument)
  {
    return operation.ConstantValue.Value;
  }

  public override object? VisitDelegateCreation(IDelegateCreationOperation operation, Optional<object?> argument)
  {
    return operation.Target.Accept(this, default);
  }

  public override object? VisitDiscardOperation(IDiscardOperation operation, Optional<object?> argument)
  {
    return null;
  }

  public override object? VisitExpressionStatement(IExpressionStatementOperation operation, Optional<object?> argument)
  {
    return operation.Operation.Accept(this, default);
  }

  public override object? VisitFieldReference(IFieldReferenceOperation operation, Optional<object?> argument)
  {
    object? instance = operation.Field.IsStatic ? null : operation.Instance!.Accept(this, default);
    FieldInfo field = _runtime.CreateFieldInfoDelegator(operation.Field);

    if (argument.HasValue)
    {
      field.SetValue(instance, argument.Value);
      return null;
    }
    else
    {
      return field.GetValue(instance);
    }
  }

  public override object? VisitForEachLoop(IForEachLoopOperation operation, Optional<object?> argument)
  {
    if (operation.IsAsynchronous)
      throw new NotSupportedException("'await foreach' is not supported.");

    IEnumerable collection = (IEnumerable)operation.Collection.Accept(this, default)!;
    IEnumerator enumerator = collection.GetEnumerator();
    BeginScope();

    while (enumerator.MoveNext())
    {
      operation.LoopControlVariable.Accept(this, enumerator.Current);
      operation.Body.Accept(this, default);
      if (CheckBranch()) break;
    }

    EndScope();

    return null;
  }

  public override object? VisitForLoop(IForLoopOperation operation, Optional<object?> argument)
  {
    BeginScope();

    foreach (IOperation before in operation.Before)
    {
      before.Accept(this, default);
    }

    while ((operation.Condition is null || (bool)operation.Condition.Accept(this, default)!) && !_returnValue.HasValue)
    {
      operation.Body.Accept(this, default);

      if (CheckBranch()) break;

      foreach (IOperation atLoopBottom in operation.AtLoopBottom)
      {
        atLoopBottom.Accept(this, default);
      }
    }

    EndScope();

    return null;
  }

  public override object? VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation, Optional<object?> argument)
  {
    object? target = operation.Target.Accept(this, default);
    object? result;

    if (operation.OperatorMethod is not null)
    {
      MethodInfo method = _runtime.CreateMethodInfoDelegator(operation.OperatorMethod);

      result = method.Invoke(null, new object?[1] { target });
    }
    else
    {
      result = operation.Kind switch
      {
        OperationKind.Increment => UnaryOperations.Increment(target),
        OperationKind.Decrement => UnaryOperations.Decrement(target),
        _                       => throw Errors.Unreacheable
      };
    }

    operation.Target.Accept(this, result);

    return operation.IsPostfix ? target : result;
  }

  public override object? VisitInstanceReference(IInstanceReferenceOperation operation, Optional<object?> argument)
  {
    return operation.ReferenceKind switch
    {
      InstanceReferenceKind.ContainingTypeInstance => Environment.ContainingTypeInstance,
      InstanceReferenceKind.ImplicitReceiver       => ImplicitReceiver,
      _                                            => throw new NotImplementedException()
    };
  }

  public override object? VisitInterpolatedString(IInterpolatedStringOperation operation, Optional<object?> argument)
  {
    string result = "";

    foreach (IInterpolatedStringContentOperation part in operation.Parts)
    {
      result += (string)part.Accept(this, default)!;
    }

    return result;
  }

  public override object? VisitInterpolatedStringText(IInterpolatedStringTextOperation operation, Optional<object?> argument)
  {
    return operation.Text.Accept(this, default);
  }

  public override object? VisitInterpolation(IInterpolationOperation operation, Optional<object?> argument)
  {
    object? expression = operation.Expression.Accept(this, default);

    if (operation.Alignment is IOperation alignmentOperation)
    {
      int alignment = (int)alignmentOperation.Accept(this, default)!;
      if (operation.FormatString is IOperation formatOperation)
      {
        string format = (string)formatOperation.Accept(this, default)!; 
        return string.Format($"{{0,{alignment}:{format}}}", expression);
      }
      else
      {
        return string.Format($"{{0,{alignment}}}", expression);
      }
    }
    else
    {
      if (operation.FormatString is IOperation formatOperation)
      {
        string format = (string)formatOperation.Accept(this, default)!;
        return string.Format($"{{0:{format}}}", expression);
      }
      else
      {
        return expression?.ToString() ?? string.Empty;
      }
    }
  }

  public override object? VisitInvocation(IInvocationOperation operation, Optional<object?> argument)
  {
    object? instance = operation.TargetMethod.IsStatic ? null : operation.Instance!.Accept(this, default);
    object?[] arguments = operation.Arguments.Map(arg => arg.Accept(this, default));
    MethodInfo method = _runtime.CreateMethodInfoDelegator(operation.TargetMethod);

    return method.Invoke(instance, arguments);
  }

  public override object? VisitIsType(IIsTypeOperation operation, Optional<object?> argument)
  {
    object? valueOperand = operation.ValueOperand.Accept(this, default);
    if (valueOperand is null)
      return false;

    Type typeOperand = _runtime.CreateTypeDelegator(operation.TypeOperand);

    return typeOperand.IsInstanceOfType(valueOperand);
  }

  public override object? VisitLiteral(ILiteralOperation operation, Optional<object?> argument)
  {
    Optional<object?> constantValue = operation.ConstantValue;

    return constantValue.HasValue
      ? constantValue.Value
      : throw new ArgumentException($"Literal operation did not have a value.");
  }

  public override object? VisitLocalReference(ILocalReferenceOperation operation, Optional<object?> argument)
  {
    if (argument.HasValue)
    {
      Environment.Assign(operation.Local, argument.Value);
      return null;
    }
    else
    {
      return Environment.Get(operation.Local);
    }
  }

  public override object? VisitMethodBodyOperation(IMethodBodyOperation operation, Optional<object?> argument)
  {
    IBlockOperation body = operation.BlockBody ?? operation.ExpressionBody ?? throw new ArgumentException("Method body operation did not have a body.", nameof(operation));

    return VisitBlock(body, default);
  }

  public override object? VisitMethodReference(IMethodReferenceOperation operation, Optional<object?> argument)
  {
    object? instance = operation.Method.IsStatic ? null : operation.Instance!.Accept(this, default);
    MethodInfo method = _runtime.CreateMethodInfoDelegator(operation.Method).UnderlyingSystemMethod;

    return Delegate.CreateDelegate(DelegateHelper.GetDelegateType(_runtime, operation.Method), instance, method);
  }

  public override object? VisitObjectCreation(IObjectCreationOperation operation, Optional<object?> argument)
  {
    ConstructorInfo constructor = _runtime.CreateConstructorInfoDelegator(operation.Constructor!);
    object?[] arguments = operation.Arguments.Map(arg => arg.Accept(this, default));
    
    object result = constructor.Invoke(arguments);

    if (operation.Initializer is IObjectOrCollectionInitializerOperation initializerOperation)
    {
      BeginReceiver(result);
      VisitObjectOrCollectionInitializer(initializerOperation, default);
      EndReceiver();
    }

    return result;
  }

  public override object? VisitNameOf(INameOfOperation operation, Optional<object?> argument)
  {
    // TODO: I fear we will need another visitor for this
    return "";
  }

  public override object? VisitObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, Optional<object?> argument)
  {
    foreach (IOperation initializer in operation.Initializers)
    {
      initializer.Accept(this, default);
    }

    return null;
  }

  public override object? VisitParameterReference(IParameterReferenceOperation operation, Optional<object?> argument)
  {
    if (argument.HasValue)
    {
      Environment.Assign(operation.Parameter, argument.Value);
      return null;
    }
    else
    {
      return Environment.Get(operation.Parameter);
    }
  }

  public override object? VisitParenthesized(IParenthesizedOperation operation, Optional<object?> argument)
  {
    return operation.Operand.Accept(this, default);
  }

  public override object? VisitPropertyReference(IPropertyReferenceOperation operation, Optional<object?> argument)
  {
    object? instance = operation.Property.IsStatic ? null : operation.Instance!.Accept(this, default);
    PropertyInfo property = _runtime.CreatePropertyInfoDelegator(operation.Property);
    object?[]? indices = operation.Arguments.Length != 0
      ? operation.Arguments.Map(arg => arg.Accept(this, default))
      : null;

    if (argument.HasValue)
    {
      property.SetValue(instance, argument.Value, indices);
      return null;
    }
    else
    {
      return property.GetValue(instance, indices);
    }
  }

  public override object? VisitRangeOperation(IRangeOperation operation, Optional<object?> argument)
  {
    Index leftOperand = operation.LeftOperand is not null
      ? (Index)operation.LeftOperand.Accept(this, default)!
      : 0;
    Index rightOperand = operation.RightOperand is not null
      ? (Index)operation.RightOperand.Accept(this, default)!
      : ^0;

    return leftOperand..rightOperand;
  }

  public override object? VisitReturn(IReturnOperation operation, Optional<object?> argument)
  {
    object? returnValue = operation.ReturnedValue is null
      ? Unit.Instance
      : operation.ReturnedValue.Accept(this, default);
    _returnValue = returnValue;
    return null;
  }

  public override object? VisitSimpleAssignment(ISimpleAssignmentOperation operation, Optional<object?> argument)
  {
    object? value = operation.Value.Accept(this, default);
    operation.Target.Accept(this, value);
    return value;
  }

  public override object? VisitTypeOf(ITypeOfOperation operation, Optional<object?> argument)
  {
    return _runtime.CreateTypeDelegator(operation.TypeOperand); // Here it's where the executed code realizes it's running in a simulation :)
  }

  public override object? VisitUnaryOperator(IUnaryOperation operation, Optional<object?> argument)
  {
    object? operand = operation.Operand.Accept(this, default);
    object? result;

    if (operation.OperatorMethod is not null)
    {
      MethodInfo method = _runtime.CreateMethodInfoDelegator(operation.OperatorMethod);

      result = method.Invoke(null, new object?[1] { operand });
    }
    else
    {
      result = operation.OperatorKind switch
      {
        UnaryOperatorKind.BitwiseNegation => UnaryOperations.BitwiseNegation(operand),
        UnaryOperatorKind.Not             => UnaryOperations.Not(operand),
        UnaryOperatorKind.Plus            => UnaryOperations.Plus(operand),
        UnaryOperatorKind.Minus           => UnaryOperations.Minus(operand),
        UnaryOperatorKind.Hat             => UnaryOperations.Hat(operand),
        _                                 => throw Errors.Unreacheable
      };
    }

    return result;
  }

  public override object? VisitVariableDeclaration(IVariableDeclarationOperation operation, Optional<object?> argument)
  {
    foreach (IVariableDeclaratorOperation declarator in operation.Declarators)
    {
      VisitVariableDeclarator(declarator, default);
    }
    return null;
  }

  public override object? VisitVariableDeclarationGroup(IVariableDeclarationGroupOperation operation, Optional<object?> argument)
  {
    foreach (IVariableDeclarationOperation declaration in operation.Declarations)
    {
      VisitVariableDeclaration(declaration, default);
    }
    return null;
  }

  public override object? VisitVariableDeclarator(IVariableDeclaratorOperation operation, Optional<object?> argument)
  {
    if (operation.Initializer is IVariableInitializerOperation initializerOperation)
    {
      object? initializer = VisitVariableInitializer(initializerOperation, default);
      Environment.Define(operation.Symbol, initializer);
    }
    else if (argument.HasValue)
    {
      Environment.DefineOrAssign(operation.Symbol, argument.Value);
    }
    else
    {
      Environment.Declare(operation.Symbol);
    }
    return null;
  }

  public override object? VisitVariableInitializer(IVariableInitializerOperation operation, Optional<object?> argument)
  {
    return operation.Value.Accept(this, default);
  }

  public override object? VisitWhileLoop(IWhileLoopOperation operation, Optional<object?> argument)
  {
    BeginScope();

    if (operation.ConditionIsTop)
    {
      while ((bool)operation.Condition.Accept(this, default)!)
      {
        operation.Body.Accept(this, default);
        if (CheckBranch()) break;
      }
    }
    else
    {
      do
      {
        operation.Body.Accept(this, default);
        if (CheckBranch()) break;
      }
      while ((bool)operation.Condition.Accept(this, default)!);
    }

    EndScope();

    return null;
  }


  // Private members

  private Environment Environment => _environments.Peek();

  private object ImplicitReceiver => _implicitReceivers.Peek();

  private void BeginScope()
  {
    _environments.Push(new Environment(Environment));
  }

  private void EndScope()
  {
    _environments.Pop();
  }

  private void BeginReceiver(object receiver)
  {
    _implicitReceivers.Push(receiver);
  }

  private void EndReceiver()
  {
    _implicitReceivers.Pop();
  }

  private void BeginConditionalAccessInstance(object instance)
  {
    _conditionalAccessInstances.Push(instance);
  }

  private void EndConditionalAccessInstance()
  {
    _conditionalAccessInstances.Pop();
  }

  private bool CheckBranch()
  {
    if (_hitContinue)
    {
      _hitContinue = false;
    }
    if (_hitBreak)
    {
      _hitBreak = false;
      return true;
    }

    return false;
  }

  private record ArrayInitializerContext(Array Array, int[] Indices, int Dimension);
}