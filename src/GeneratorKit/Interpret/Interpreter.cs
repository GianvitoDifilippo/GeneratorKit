using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GeneratorKit.Interpret;

internal partial class Interpreter
{
  private readonly List<OperationContext> _operations;

  public Interpreter()
  {
    _operations = new List<OperationContext>();
  }

  public int RegisterOperation(GeneratorRuntime runtime, IOperation operation, IMethodSymbol symbol)
  {
    int index = _operations.Count;
    _operations.Add(new OperationContext(runtime, operation, symbol));
    return index;
  }

  public T Interpret<T>(OperationContext context, object? target, object?[] arguments)
  {
    context.Deconstruct(out GeneratorRuntime runtime, out IOperation operation, out IMethodSymbol method);

    ImmutableArray<IParameterSymbol> parameters = method.Parameters;
    if (arguments.Length != parameters.Length)
      throw new ArgumentException($"Wrong number of arguments supplied to method {method}.", nameof(arguments));

    Environment environment = new Environment();
    for (int i = 0; i < arguments.Length; i++)
    {
      environment.Define(parameters[i], arguments[i]);
    }

    InterpreterVisitor visitor = new InterpreterVisitor(runtime, this, target, environment);

    object? result = visitor.Visit(operation, default);

    return (T)result!;
  }

  private T InterpretImpl<T>(int operationId, object? target, object?[] arguments)
  {
    if (operationId < 0 || operationId >= _operations.Count)
      throw new ArgumentException($"No registered operation matches id '{operationId}'.", nameof(operationId));

    return Interpret<T>(_operations[operationId], target, arguments);
  }
}