using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace GeneratorKit.Emit;

internal class ConstructorInitializerVisitor : OperationVisitor
{
  private const BindingFlags s_allInstanceConstructors = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

  private readonly GeneratorRuntime _runtime;
  private readonly ILGenerator _il;
  private readonly Type _baseType;

  public ConstructorInitializerVisitor(GeneratorRuntime runtime, ILGenerator il, Type baseType)
  {
    _runtime = runtime;
    _il = il;
    _baseType = baseType;
  }

  public override void DefaultVisit(IOperation operation)
  {
    // For anything else... we'll need an interpreter.
  }

  public override void VisitConstructorBodyOperation(IConstructorBodyOperation operation)
  {
    if (operation.Initializer is not null)
    {
      Visit(operation.Initializer);
    }
    else
    {
      ConstructorInfo constructor = _baseType.GetConstructor(s_allInstanceConstructors, null, Type.EmptyTypes, null);
      _il.Emit(OpCodes.Ldarg_0);
      _il.Emit(OpCodes.Call, constructor);
    }
  }

  public override void VisitExpressionStatement(IExpressionStatementOperation operation)
  {
    Visit(operation.Operation);
  }

  public override void VisitInvocation(IInvocationOperation operation)
  {
    ConstructorInfo constructor = GetBaseConstructor(operation.TargetMethod);

    _il.Emit(OpCodes.Ldarg_0);

    foreach (IArgumentOperation argument in operation.Arguments)
    {
      IOperation value = argument.Value;
      if (value.ConstantValue.HasValue && value.ConstantValue.Value is null) // To avoid dealing with a converion operation
      {
        _il.Emit(OpCodes.Ldnull);
      }
      else
      {
        Visit(argument.Value);
      }
    }

    _il.Emit(OpCodes.Call, constructor);
  }

  public override void VisitLiteral(ILiteralOperation operation)
  {
    object constantValue = operation.ConstantValue.Value!;
    switch (operation.Type!.Name)
    {
      case "Boolean":
        _il.Emit((bool)constantValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        break;
      case "Char":
        _il.Emit(OpCodes.Ldc_I4_S, (char)constantValue);
        break;
      case "SByte":
        _il.Emit(OpCodes.Ldc_I4_S, (sbyte)constantValue);
        break;
      case "Byte":
        _il.Emit(OpCodes.Ldc_I4_S, (byte)constantValue);
        break;
      case "Int16":
        _il.Emit(OpCodes.Ldc_I4_S, (short)constantValue);
        break;
      case "UInt16":
        _il.Emit(OpCodes.Ldc_I4_S, (ushort)constantValue);
        break;
      case "Int32":
        _il.Emit(OpCodes.Ldc_I4_S, (int)constantValue);
        break;
      case "UInt32":
        _il.Emit(OpCodes.Ldc_I4_S, (int)(uint)constantValue);
        break;
      case "Int64":
        _il.Emit(OpCodes.Ldc_I8, (long)constantValue);
        break;
      case "UInt64":
        _il.Emit(OpCodes.Ldc_I8, (long)(ulong)constantValue);
        break;
      case "Single":
        _il.Emit(OpCodes.Ldc_R4, (float)constantValue);
        break;
      case "Double":
        _il.Emit(OpCodes.Ldc_R8, (double)constantValue);
        break;
      case "String":
        _il.Emit(OpCodes.Ldstr, (string)constantValue);
        break;
      case "Decimal":
        throw new NotSupportedException("Decimal literals are not supported yet.");
      default:
        throw new NotSupportedException();
    }
  }

  public override void VisitParameterReference(IParameterReferenceOperation operation)
  {
    int position = operation.Parameter.Ordinal;
    switch (position)
    {
      case 0:
        _il.Emit(OpCodes.Ldarg_1);
        break;
      case 1:
        _il.Emit(OpCodes.Ldarg_2);
        break;
      case 2:
        _il.Emit(OpCodes.Ldarg_3);
        break;
      default:
        _il.Emit(OpCodes.Ldarg_S, position + 1);
        break;
    }
  }

  private ConstructorInfo GetBaseConstructor(IMethodSymbol baseConstructorSymbol)
  {
    if (_baseType.ContainsGenericParameters)
    {
      SymbolConstructorInfo symbolConstructorDefinition = _runtime.CreateConstructorInfoDelegator(baseConstructorSymbol.OriginalDefinition);
      ConstructorInfo constructorDefinition = MemberResolver.ResolveConstructor(_baseType.GetGenericTypeDefinition(), symbolConstructorDefinition);
      return TypeBuilder.GetConstructor(_baseType, constructorDefinition);
    }

    return MemberResolver.ResolveConstructor(_baseType, _runtime.CreateConstructorInfoDelegator(baseConstructorSymbol));
  }
}