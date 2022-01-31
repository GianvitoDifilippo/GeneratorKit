using GeneratorKit.Interpret;
using GeneratorKit.Reflection;
using System;
using System.Collections.Generic;

namespace GeneratorKit.Proxy;

internal class OperationDelegate : IOperationDelegate
{
  private readonly IInterpreter _interpreter;
  private readonly IReadOnlyDictionary<int, SymbolMethodInfo> _methods;

  public OperationDelegate(IInterpreter interpreter, InterpreterFrame instanceFrame, IReadOnlyDictionary<int, SymbolMethodInfo> methods)
  {
    _interpreter = interpreter;
    InstanceFrame = instanceFrame;
    _methods = methods;
  }

  public InterpreterFrame InstanceFrame { get; }

  public void Invoke(int operationId, Type[] typeArguments, object?[] arguments)
  {
    if (!_methods.TryGetValue(operationId, out SymbolMethodInfo? method))
      throw new ArgumentException("Invalid operation id.", nameof(operationId));

    _interpreter.Interpret(new HybridGenericMethod(method, typeArguments), InstanceFrame, arguments);
  }

  public T Invoke<T>(int operationId, Type[] typeArguments, object?[] arguments)
  {
    if (!_methods.TryGetValue(operationId, out SymbolMethodInfo? method))
      throw new ArgumentException("Invalid operation id.", nameof(operationId));

    return (T)_interpreter.Interpret(new HybridGenericMethod(method, typeArguments), InstanceFrame, arguments)!;
  }
}
