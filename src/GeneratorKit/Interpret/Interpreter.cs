using GeneratorKit.Exceptions;
using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

  public object?[] GetProxyArguments(IRuntimeConstructor constructor, InterpreterFrame classFrame, object?[] arguments)
  {
    if (!_operationManager.TryGetOperation(constructor.Symbol, out IOperation? operation))
      throw new InvalidUserCodeException(); // TODO: Message

    InterpreterFrame constructorFrame = _frameProvider.GetConstructorFrame(classFrame, constructor, arguments);

    return (object?[])new ConstructorInitializerVisitor(_runtime, _frameProvider, constructorFrame).Visit(operation, default)!;
  }

  public object? Interpret(IRuntimeMethod method, InterpreterFrame frame, object?[] arguments)
  {
    if (!_operationManager.TryGetOperation(method.Definition.Symbol, out IOperation? operation))
      throw new InvalidUserCodeException(); // TODO: Message

    InterpreterFrame methodFrame = _frameProvider.GetMethodFrame(frame, method, arguments);

    return new InterpreterVisitor(_runtime, _frameProvider, methodFrame).Visit(operation, default);
  }

  public void Interpret(IRuntimeConstructor constructor, InterpreterFrame frame, object?[] arguments)
  {
    if (!_operationManager.TryGetOperation(constructor.Symbol, out IOperation? operation))
      throw new InvalidUserCodeException(); // TODO: Message

    InterpreterFrame methodFrame = _frameProvider.GetConstructorFrame(frame, constructor, arguments);

    new InterpreterVisitor(_runtime, _frameProvider, methodFrame).Visit(operation, default);
  }

  public void InitInstance(IRuntimeType type, InterpreterFrame instanceFrame)
  {
    IEnumerable<ISymbol> fields = type.Definition.Symbol.GetMembers().Where(m => m.Kind is SymbolKind.Field && !m.IsStatic);
    foreach (IFieldSymbol field in fields)
    {
      if (!field.IsImplicitlyDeclared)
      {
        InitField(field, instanceFrame);
      }
      else if (field.AssociatedSymbol is IPropertySymbol propertySymbol)
      {
        InitProperty(propertySymbol, instanceFrame);
      }
    }
  }

  private void InitField(IFieldSymbol field, InterpreterFrame instanceFrame)
  {
    if (_operationManager.TryGetOperation(field, out IOperation? operation))
    {
      object? value = new InterpreterVisitor(_runtime, _frameProvider, instanceFrame).Visit(operation, default);
      instanceFrame.Define(field, value);
    }
    else
    {
      instanceFrame.Declare(field);
    }
  }

  private void InitProperty(IPropertySymbol property, InterpreterFrame instanceFrame)
  {
    if (_operationManager.TryGetOperation(property, out IOperation? operation))
    {
      object? value = new InterpreterVisitor(_runtime, _frameProvider, instanceFrame).Visit(operation, default);
      instanceFrame.Define(property, value);
    }
    else
    {
      instanceFrame.Declare(property);
    }
  }
}
