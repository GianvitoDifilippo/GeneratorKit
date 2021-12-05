using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Reflection.Emit;
using System.Reflection;
using System;

namespace GeneratorKit.Proxy;

internal class FieldInitializerOperationVisitor : OperationVisitor
{
  private readonly ILGenerator _il;
  private readonly FieldInfo _field;

  public FieldInitializerOperationVisitor(ILGenerator il, FieldInfo field)
  {
    _il = il;
    _field = field;
  }

  public override void DefaultVisit(IOperation operation)
  {
    throw new NotSupportedException($"{operation.Kind} in field initializer is not supported.");
  }

  public override void VisitLiteral(ILiteralOperation operation)
  {
    _il.Emit(OpCodes.Ldarg_0);
    switch (operation.Type?.Name)
    {
      case "Boolean":
        _il.Emit((bool)operation.ConstantValue.Value! ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        break;
      case "Char":
        _il.Emit(OpCodes.Ldc_I4_S, (char)operation.ConstantValue.Value!);
        break;
      case "SByte":
        _il.Emit(OpCodes.Ldc_I4_S, (sbyte)operation.ConstantValue.Value!);
        break;
      case "Byte":
        _il.Emit(OpCodes.Ldc_I4_S, (byte)operation.ConstantValue.Value!);
        break;
      case "Int16":
        _il.Emit(OpCodes.Ldc_I4_S, (short)operation.ConstantValue.Value!);
        break;
      case "UInt16":
        _il.Emit(OpCodes.Ldc_I4_S, (ushort)operation.ConstantValue.Value!);
        break;
      case "Int32":
        _il.Emit(OpCodes.Ldc_I4_S, (int)operation.ConstantValue.Value!);
        break;
      case "UInt32":
        _il.Emit(OpCodes.Ldc_I4_S, (int)(uint)operation.ConstantValue.Value!);
        break;
      case "Int64":
        _il.Emit(OpCodes.Ldc_I8, (long)operation.ConstantValue.Value!);
        break;
      case "UInt64":
        _il.Emit(OpCodes.Ldc_I8, (long)(ulong)operation.ConstantValue.Value!);
        break;
      case "Single":
        _il.Emit(OpCodes.Ldc_R4, (float)operation.ConstantValue.Value!);
        break;
      case "Double":
        _il.Emit(OpCodes.Ldc_R8, (double)operation.ConstantValue.Value!);
        break;
      case "String":
        _il.Emit(OpCodes.Ldstr, (string)operation.ConstantValue.Value!);
        break;
      case "Decimal":
        throw new NotSupportedException("Decimal literals are not supported yet.");
      default:
        throw new NotSupportedException();
    }
    _il.Emit(OpCodes.Stfld, _field);
  }
}