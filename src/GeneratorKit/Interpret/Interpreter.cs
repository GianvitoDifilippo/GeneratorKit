using GeneratorKit.Exceptions;
using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;

namespace GeneratorKit.Interpret;

internal class Interpreter : IInterpreter
{
  private readonly GeneratorRuntime _runtime;
  private readonly IOperationManager _operationManager;
  private readonly IFrameProvider _frameProvider;

  public Interpreter(GeneratorRuntime runtime, IOperationManager operationManager, IFrameProvider frameProvider)
  {
    _runtime = runtime;
    _operationManager = operationManager;
    _frameProvider = frameProvider;
  }

  public object?[] GetProxyArguments(SymbolConstructorInfo constructor, InterpreterFrame classFrame, object?[] arguments)
  {
    if (!_operationManager.TryGetOperation(constructor.Symbol, out IOperation? operation))
      throw new InvalidUserCodeException(); // TODO: Message

    InterpreterFrame constructorFrame = _frameProvider.GetConstructorFrame(classFrame, constructor, arguments);

    return (object?[])new ConstructorInitializerVisitor(_runtime, this, _frameProvider, constructorFrame).Visit(operation, default)!;
  }

  public object? Interpret(IRuntimeMethod method, InterpreterFrame frame, object?[] arguments)
  {
    if (!_operationManager.TryGetOperation(method.Definition.Symbol, out IOperation? operation))
      throw new InvalidUserCodeException(); // TODO: Message

    InterpreterFrame methodFrame = _frameProvider.GetMethodFrame(frame, method, arguments);

    return new InterpreterVisitor(_runtime, this, _frameProvider, methodFrame).Visit(operation, default);
  }
}
