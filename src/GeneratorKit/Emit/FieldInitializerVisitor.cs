using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Reflection.Emit;

namespace GeneratorKit.Emit;

internal class FieldInitializerVisitor : OperationVisitor
{
  private readonly ILGenerator _il;

  public FieldInitializerVisitor(ILGenerator il)
  {
    _il = il;
  }

  public override void DefaultVisit(IOperation operation)
  {
    // For anything else... we'll need an interpreter.
  }

  public override void VisitConversion(IConversionOperation operation)
  {
    if (operation.ConstantValue.HasValue && operation.ConstantValue.Value is null)
    {
      _il.Emit(OpCodes.Ldnull);
    }
    else
    {
      DefaultVisit(operation);
    }
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
}