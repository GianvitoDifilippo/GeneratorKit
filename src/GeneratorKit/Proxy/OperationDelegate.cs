using GeneratorKit.Interpret;
using GeneratorKit.Interpret.Frame;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace GeneratorKit.Proxy;

internal class OperationDelegate : IOperationDelegate
{
  private readonly IInterpreter _interpreter;
  private readonly IReadOnlyDictionary<int, IMethodSymbol> _methods;

  public OperationDelegate(IInterpreter interpreter, InterpreterFrame instanceFrame, IReadOnlyDictionary<int, IMethodSymbol> methods)
  {
    _interpreter = interpreter;
    InstanceFrame = instanceFrame;
    _methods = methods;
  }

  public InterpreterFrame InstanceFrame { get; }

  public void Invoke(int operationId, Type[] typeArguments, object?[] arguments)
  {
    if (!_methods.TryGetValue(operationId, out IMethodSymbol? method))
      throw new ArgumentException("Invalid operation id.", nameof(operationId));

    _interpreter.InterpretMethod(method, InstanceFrame, typeArguments, arguments);
  }

  public T Invoke<T>(int operationId, Type[] typeArguments, object?[] arguments)
  {
    if (!_methods.TryGetValue(operationId, out IMethodSymbol? method))
      throw new ArgumentException("Invalid operation id.", nameof(operationId));

    return (T)_interpreter.InterpretMethod(method, InstanceFrame, typeArguments, arguments)!;
  }
}
