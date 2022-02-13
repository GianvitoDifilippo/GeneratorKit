using GeneratorKit.Exceptions;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace GeneratorKit.Interpret;

internal partial class InterpreterVisitor : OperationVisitor<Optional<object?>, object>
{
  public override object? Visit(IOperation? operation, Optional<object?> argument)
  {
    if (operation is null)
      throw new ArgumentNullException(nameof(operation));

    return operation.Accept(this, argument);
  }

  public override object? DefaultVisit(IOperation operation, Optional<object?> argument)
  {
    throw new NotSupportedException($"Operation of kind {operation.Kind} cannot be interpreted.");
  }


  public override object? VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, Optional<object?> argument)
  {
    Type type = _context.GetType(operation.Type!);
    object instance = _context.CreateInstance(type, Array.Empty<object>());
    BeginReceiver(instance);
    object?[] initializers = operation.Initializers.Map(initializer => initializer.Accept(this, default));
    EndReceiver();

    return instance;
  }

  public override object? VisitArgument(IArgumentOperation operation, Optional<object?> argument)
  {
    if (operation.Parameter is null)
      throw new NotSupportedException("__arglist parameters are not supported.");

    return operation.Value.Accept(this, argument);
  }

  public override object? VisitArrayCreation(IArrayCreationOperation operation, Optional<object?> argument)
  {
    int rank = operation.DimensionSizes.Length;
    int[] dimensionSizes = operation.DimensionSizes.Map(x => (int)x.Accept(this, default)!);
    Type elementType = _context.GetType(operation.Type!).GetElementType()!.UnderlyingSystemType;
    Array array = Array.CreateInstance(elementType, dimensionSizes);
    int[] indices = new int[rank];

    if (operation.Initializer is IArrayInitializerOperation initializerOperation)
    {
      BeginArrayInitializerContext(array, indices, 0);
      VisitArrayInitializer(initializerOperation, default);
      EndArrayInitializerContext();
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

      return array.GetSubArray((Range)index);
    }

    int[] indices = operation.Indices.Map(index => (int)index.Accept(this, default)!);
    return array.GetValue(indices);
  }

  public override object? VisitArrayInitializer(IArrayInitializerOperation operation, Optional<object?> argument)
  {
    ArrayInitializerContext context = ArrayContext;
    (Array array, int[] indices, int dimension) = context;
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
        context.Dimension++;
        operation.ElementValues[i].Accept(this, default);
        context.Dimension--;
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
      MethodInfo method = _context.GetMethodInfo(operation.OperatorMethod);

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
        BinaryOperatorKind.Equals             => BinaryOperations.Equal(leftOperand, rightOperand),
        BinaryOperatorKind.NotEquals          => BinaryOperations.NotEqual(leftOperand, rightOperand),
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
      if (_returnValue.HasValue || _branchState is not BranchKind.None)
      {
        break;
      }
    }

    return _returnValue.HasValue ? _returnValue.Value : Unit.Instance;
  }

  public override object? VisitBranch(IBranchOperation operation, Optional<object?> argument)
  {
    _branchState = operation.BranchKind;
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

    BeginConditionalAccess(instance);
    object? whenNotNull = operation.WhenNotNull.Accept(this, default);
    EndConditionalAccess();

    return whenNotNull;
  }

  public override object? VisitConditionalAccessInstance(IConditionalAccessInstanceOperation operation, Optional<object?> argument)
  {
    return ConditionalAccessInstance;
  }

  public override object? VisitCompoundAssignment(ICompoundAssignmentOperation operation, Optional<object?> argument)
  {
    object? target = operation.Target.Accept(this, default);
    object? value = operation.Value.Accept(this, default);
    object? result;

    if (operation.OperatorMethod is not null)
    {
      MethodInfo method = _context.GetMethodInfo(operation.OperatorMethod);

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
        BinaryOperatorKind.Equals             => BinaryOperations.Equal(target, value),
        BinaryOperatorKind.NotEquals          => BinaryOperations.NotEqual(target, value),
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
    }

    return result;
  }

  public override object? VisitConstructorBodyOperation(IConstructorBodyOperation operation, Optional<object?> argument)
  {
    IBlockOperation body = operation.BlockBody ?? operation.ExpressionBody ?? throw new InvalidUserCodeException(); // TODO: Message

    return VisitBlock(body, default);
  }

  public override object? VisitConversion(IConversionOperation operation, Optional<object?> argument)
  {
    object? operand = operation.Operand.Accept(this, default);

    CommonConversion conversion = operation.Conversion;
    if (conversion.IsIdentity)
      return operand;

    if (conversion.IsUserDefined)
    {
      MethodInfo method = _context.GetMethodInfo(operation.OperatorMethod!);

      return method.Invoke(null, new object?[1] { operand });
    }

    if (conversion.IsImplicit && operation.Type!.ContainingNamespace.Name == "System")
    {
      switch (operation.Type.Name)
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

    Type type = _context.GetType(operation.Type!);
    Type operandType = operand.GetType();

    if (!type.IsAssignableFrom(operandType))
      throw new InvalidCastException($"Unable to cast object of type '{operandType}' to type '{type}'.");

    return operand;
  }

  public override object? VisitDeclarationExpression(IDeclarationExpressionOperation operation, Optional<object?> argument)
  {
    return operation.Expression.Accept(this, argument);
  }

  public override object? VisitDeconstructionAssignment(IDeconstructionAssignmentOperation operation, Optional<object?> argument)
  {
    object? value = operation.Value.Accept(this, default);
    return operation.Target.Accept(this, value);
  }

  public override object? VisitDefaultValue(IDefaultValueOperation operation, Optional<object?> argument)
  {
    if (operation.ConstantValue.HasValue)
      return operation.ConstantValue.Value;
    if (operation.Type is null)
      return null;

    Type type = _context.GetType(operation.Type);
    return type.GetDefaultValue();
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

  public override object? VisitFieldInitializer(IFieldInitializerOperation operation, Optional<object?> argument)
  {
    throw new InvalidOperationException($"Operation of type {operation.Kind} can be interpreted in a constructor interpreter only.");
  }

  public override object? VisitFieldReference(IFieldReferenceOperation operation, Optional<object?> argument)
  {
    object? instance = operation.Field.IsStatic ? null : operation.Instance!.Accept(this, default);
    FieldInfo field = _context.GetFieldInfo(operation.Field);

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
      if (ResetBranchState()) break;
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

      if (ResetBranchState()) break;

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
      MethodInfo method = _context.GetMethodInfo(operation.OperatorMethod);

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
      InstanceReferenceKind.ContainingTypeInstance => Frame.Instance,
      InstanceReferenceKind.ImplicitReceiver       => ImplicitReceiver,
      _                                            => throw new NotSupportedException($"Instance reference of kind {operation.ReferenceKind} is not supported.")
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
      if (operation.FormatString is not null)
      {
        string format = (string)operation.FormatString.Accept(this, default)!;
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
    // TODO: Intercept GetType, ToString, GetHashCode, MethodBase.GetCurrentMethod()
    object? instance = operation.TargetMethod.IsStatic ? null : operation.Instance!.Accept(this, default);
    object?[] arguments = operation.Arguments.Map(arg => arg.Accept(this, default));
    MethodInfo method = _context.GetMethodInfo(operation.TargetMethod);

    return method.Invoke(instance, arguments);
  }

  public override object? VisitIsNull(IIsNullOperation operation, Optional<object?> argument)
  {
    object? operand = operation.Operand.Accept(this, default);
    return operand is null;
  }

  public override object? VisitIsType(IIsTypeOperation operation, Optional<object?> argument)
  {
    object? valueOperand = operation.ValueOperand.Accept(this, default);
    Type typeOperand = _context.GetType(operation.TypeOperand);

    return typeOperand.IsInstanceOfType(valueOperand);
  }

  public override object? VisitLiteral(ILiteralOperation operation, Optional<object?> argument)
  {
    Optional<object?> constantValue = operation.ConstantValue;
    Debug.Assert(constantValue.HasValue, "Expected literal operation to have a constant value.");
    return constantValue.Value;
  }

  public override object? VisitLocalReference(ILocalReferenceOperation operation, Optional<object?> argument)
  {
    if (argument.HasValue)
    {
      if (operation.IsDeclaration)
      {
        Frame.Define(operation.Local, argument.Value);
      }
      else
      {
        Frame.Assign(operation.Local, argument.Value);
      }
      return null;
    }
    else
    {
      return Frame.Get(operation.Local);
    }
  }

  public override object? VisitMemberInitializer(IMemberInitializerOperation operation, Optional<object?> argument)
  {
    object? member = operation.InitializedMember.Accept(this, default);
    if (member is null)
      throw new NullReferenceException();

    BeginReceiver(member);
    VisitObjectOrCollectionInitializer(operation.Initializer, default);
    EndReceiver();

    return null;
  }

  public override object? VisitMethodBodyOperation(IMethodBodyOperation operation, Optional<object?> argument)
  {
    IBlockOperation body = operation.BlockBody ?? operation.ExpressionBody ?? throw new InvalidUserCodeException(); // TODO: Message

    return VisitBlock(body, default);
  }

  public override object? VisitMethodReference(IMethodReferenceOperation operation, Optional<object?> argument)
  {
    object? instance = operation.Method.IsStatic ? null : operation.Instance!.Accept(this, default);
    MethodInfo method = _context.GetMethodInfo(operation.Method);

    throw new NotSupportedException();
  }

  public override object? VisitNameOf(INameOfOperation operation, Optional<object?> argument)
  {
    Optional<object?> constantValue = operation.ConstantValue;
    Debug.Assert(constantValue.HasValue, "Expected nameof operation to have a constant value.");
    return constantValue.Value;
  }

  public override object? VisitObjectCreation(IObjectCreationOperation operation, Optional<object?> argument)
  {
    object?[] arguments = operation.Arguments.Map(arg => arg.Accept(this, default));
    ConstructorInfo constructor = _context.GetConstructorInfo(operation.Constructor); // TODO: Why can it be null?

    object? result = constructor.Invoke(arguments);

    if (operation.Initializer is IObjectOrCollectionInitializerOperation initializerOperation)
    {
      BeginReceiver(result);
      VisitObjectOrCollectionInitializer(initializerOperation, default);
      EndReceiver();
    }

    return result;
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
      Frame.Assign(operation.Parameter, argument.Value);
      return argument.Value;
    }
    else
    {
      return Frame.Get(operation.Parameter);
    }
  }

  public override object? VisitPropertyReference(IPropertyReferenceOperation operation, Optional<object?> argument)
  {
    object? instance = operation.Property.IsStatic ? null : operation.Instance!.Accept(this, default);
    object?[] arguments = operation.Arguments.Map(arg => arg.Accept(this, default));
    PropertyInfo property = _context.GetPropertyInfo(operation.Property);

    if (argument.HasValue)
    {
      property.SetValue(instance, argument.Value, arguments);
      return null;
    }
    else
    {
      return property.GetValue(instance, arguments);
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
    _returnValue = operation.ReturnedValue is null
      ? Unit.Instance
      : operation.ReturnedValue.Accept(this, default);
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
    return _context.GetType(operation.TypeOperand);
  }

  public override object? VisitTuple(ITupleOperation operation, Optional<object?> argument)
  {
    if (argument.HasValue)
    {
      object?[] values = (object?[])argument.Value!;
      Debug.Assert(values.Length == operation.Elements.Length, "Left side and right side of tuple assignment must have the same number of elements.");
      for (int i = 0; i < values.Length; i++)
      {
        operation.Elements[i].Accept(this, values[i]);
      }
      return null;
    }
    else
    {
      return operation.Elements.Map(el => el.Accept(this, default));
    }
  }

  public override object? VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation, Optional<object?> argument)
  {
    Type type = _context.GetType(operation.Type!);
    object instance = _context.CreateInstance(type, Array.Empty<object>());

    if (operation.Initializer is { } initializer)
    {
      BeginReceiver(instance);
      VisitObjectOrCollectionInitializer(initializer, default);
      EndReceiver();
    }

    return instance;
  }

  public override object? VisitUnaryOperator(IUnaryOperation operation, Optional<object?> argument)
  {
    object? operand = operation.Operand.Accept(this, default);
    object? result;

    if (operation.OperatorMethod is not null)
    {
      MethodInfo method = _context.GetMethodInfo(operation.OperatorMethod);

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
      Frame.Define(operation.Symbol, initializer);
    }
    else if (argument.HasValue)
    {
      Frame.DefineOrAssign(operation.Symbol, argument.Value);
    }
    else
    {
      Frame.Declare(operation.Symbol);
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

    if (operation.Condition is null)
      throw new InvalidUserCodeException(); // TODO: Message

    if (operation.ConditionIsTop)
    {
      while ((bool)operation.Condition.Accept(this, default)!)
      {
        operation.Body.Accept(this, default);
        if (ResetBranchState()) break;
      }
    }
    else
    {
      do
      {
        operation.Body.Accept(this, default);
        if (ResetBranchState()) break;
      }
      while ((bool)operation.Condition.Accept(this, default)!);
    }

    EndScope();

    return null;
  }
}