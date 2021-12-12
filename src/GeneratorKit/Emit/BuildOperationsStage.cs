using Microsoft.CodeAnalysis;
using System.Reflection.Emit;
using System.Reflection;
using System;
using GeneratorKit.Interpret;
using GeneratorKit.Utils;

namespace GeneratorKit.Emit;

internal class BuildOperationsStage
{
  private static readonly MethodInfo s_interpretMethod;

  static BuildOperationsStage()
  {
    s_interpretMethod = typeof(Interpreter).GetMethod(
      nameof(Interpreter.Interpret),
      BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
      null,
      new Type[] { typeof(int), typeof(object), typeof(object[]) },
      null)
      ?? throw new InvalidOperationException("Could not find the interpret method.");
  }

  protected IBuildContext _context;

  public BuildOperationsStage(IBuildContext context)
  {
    _context = context;
  }
  
  protected void BuildMethodBody(ILGenerator il, Type returnType, Type[] parameterTypes, IOperation operation, IMethodSymbol symbol)
  {
    // TODO: Adapt for static methods

    int operationId = Interpreter.s_instance.RegisterOperation(_context.Runtime, operation, symbol);

    int arity = parameterTypes.Length;

    il.Emit(OpCodes.Ldc_I4, operationId);                                        // Push operationId onto the stack                                     | Stack: operationId

    if (symbol.IsStatic)
      il.Emit(OpCodes.Ldnull);                                                   // Push null := target onto the stack                                  | Stack: operationId, target
    else
      il.Emit(OpCodes.Ldarg_0);                                                  // Push this := target onto the stack                                  | Stack: operationId, target

    il.Emit(OpCodes.Ldc_I4, arity);                                              // Push arity onto the stack                                           | Stack: operationId, target, arity
    il.Emit(OpCodes.Newarr, typeof(object));                                     // Call new object[arity] := arguments                                 | Stack: operationId, target, arguments
    if (arity >= 1)
    {
      il.Emit(OpCodes.Dup);                                                      // Duplicate the topmost value of the stack                            | Stack: operationId, target, arguments, arguments
      il.Emit(OpCodes.Ldc_I4, 0);                                                // Push 0 onto the stack                                               | Stack: operationId, target, arguments, arguments, 0
      il.Emit(OpCodes.Ldarg_1);                                                  // Push the first argument onto the stack                              | Stack: operationId, target, arguments, arguments, 0, 1stArgument
      if (parameterTypes[0].IsValueType)
        il.Emit(OpCodes.Box, parameterTypes[0]);                                 // Box the value if needed
      il.Emit(OpCodes.Stelem_Ref);                                               // Assign the element of the array of index 0                          | Stack: operationId, target, arguments
    }
    if (arity >= 2)
    {
      il.Emit(OpCodes.Dup);                                                      // Duplicate the topmost value of the stack                            | Stack: operationId, target, arguments, arguments
      il.Emit(OpCodes.Ldc_I4, 1);                                                // Push 1 onto the stack                                               | Stack: operationId, target, arguments, arguments, 1
      il.Emit(OpCodes.Ldarg_2);                                                  // Push the second argument onto the stack                             | Stack: operationId, target, arguments, arguments, 1, 2ndArgument
      if (parameterTypes[1].IsValueType)
        il.Emit(OpCodes.Box, parameterTypes[1]);                                 // Box the value if needed
      il.Emit(OpCodes.Stelem_Ref);                                               // Assign the element of the array of index 1                          | Stack: operationId, target, arguments
    }
    if (arity >= 3)
    {
      il.Emit(OpCodes.Dup);                                                      // Duplicate the topmost value of the stack                            | Stack: operationId, target, arguments, arguments
      il.Emit(OpCodes.Ldc_I4, 2);                                                // Push 2 onto the stack                                               | Stack: operationId, target, arguments, arguments, 2
      il.Emit(OpCodes.Ldarg_3);                                                  // Push the third argument onto the stac k                             | Stack: operationId, target, arguments, arguments, 2, 3rdArgument
      if (parameterTypes[2].IsValueType)
        il.Emit(OpCodes.Box, parameterTypes[2]);                                 // Box the value if needed
      il.Emit(OpCodes.Stelem_Ref);                                               // Assign the element of the array of index 2                          | Stack: operationId, target, arguments
    }
    if (arity >= 4)
    {
      for (int i = 3; i < arity; i++)
      {
        il.Emit(OpCodes.Dup);                                                    // Duplicate the topmost value of the stack                            | Stack: operationId, target, arguments, arguments
        il.Emit(OpCodes.Ldc_I4, i);                                              // Push i onto the stack                                               | Stack: operationId, target, arguments, arguments, i
        il.Emit(OpCodes.Ldarg, i + 1);                                           // Push the (i+1)-th argument onto the stack                           | Stack: operationId, target, arguments, arguments, i, (i+1)thArgument
        if (parameterTypes[i].IsValueType)
          il.Emit(OpCodes.Box, parameterTypes[i]);                               // Box the value if needed
        il.Emit(OpCodes.Stelem_Ref);                                             // Assign the element of the array of index i                          | Stack: operationId, target, arguments
      }
    }

    if (returnType == typeof(void))
    {
      il.Emit(OpCodes.Call, s_interpretMethod.MakeGenericMethod(typeof(Unit)));  // Call Interpreter.Interpret(operationId, target, arguments) := unit  | Stack: unit
      il.Emit(OpCodes.Pop);                                                      // Pop the topmost value of the stack                                  | Stack: -
    }
    else
    {
      il.Emit(OpCodes.Call, s_interpretMethod.MakeGenericMethod(returnType));    // Call Interpreter.Interpret(operationId, this, arguments) := result  | Stack: result
    }
  }
}