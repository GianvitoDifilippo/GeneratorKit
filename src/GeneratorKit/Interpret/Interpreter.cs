using GeneratorKit.Comparers;
using GeneratorKit.Exceptions;
using GeneratorKit.Interpret.Frame;
using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace GeneratorKit.Interpret;

internal class Interpreter : IInterpreter
{
  private readonly IReflectionRuntime _runtime;
  private readonly GeneratorContext _context;
  private readonly IOperationManager _operationManager;
  private readonly IFrameProvider _frameProvider;
  private readonly Dictionary<SymbolType, InterpreterFrame> _typeFrames;

  public Interpreter(IReflectionRuntime runtime, GeneratorContext context, IOperationManager operationManager, IFrameProvider frameProvider)
  {
    _runtime = runtime;
    _context = context;
    _operationManager = operationManager;
    _frameProvider = frameProvider;
    _typeFrames = new Dictionary<SymbolType, InterpreterFrame>(TypeEqualityComparer.Default);
  }

  public object?[] GetProxyArguments(IMethodSymbol constructor, InterpreterFrame typeFrame, object?[] arguments)
  {
    if (!_operationManager.TryGetOperation(constructor, out IOperation? operation))
      throw new InvalidUserCodeException(); // TODO: Message

    InterpreterFrame constructorFrame = GetConstructorFrame(typeFrame, constructor, arguments);
    InterpreterMethodContext context = new InterpreterMethodContext(_runtime, typeFrame.TypeContext, Type.EmptyTypes);

    return (object?[])new ConstructorInitializerVisitor(context, constructorFrame).Visit(operation, default)!;
  }

  public object? InterpretMethod(IMethodSymbol method, InterpreterFrame frame, Type[] typeArguments, object?[] arguments)
  {
    if (!_operationManager.TryGetOperation(method.OriginalDefinition, out IOperation? operation))
      throw new InvalidUserCodeException(); // TODO: Message

    InterpreterFrame methodFrame = GetMethodFrame(frame, method, arguments);
    InterpreterMethodContext context = new InterpreterMethodContext(_runtime, frame.TypeContext, typeArguments);

    return new InterpreterVisitor(context, methodFrame).Visit(operation, default);
  }

  public void InterpretConstructor(IMethodSymbol constructor, InterpreterFrame frame, object?[] arguments)
  {
    if (!_operationManager.TryGetOperation(constructor.OriginalDefinition, out IOperation? operation))
      throw new InvalidUserCodeException(); // TODO: Message

    InterpreterFrame methodFrame = GetConstructorFrame(frame, constructor, arguments);
    InterpreterMethodContext context = new InterpreterMethodContext(_runtime, frame.TypeContext, Type.EmptyTypes);

    new InterpreterVisitor(context, methodFrame).Visit(operation, default);
  }

  private void InitField(IFieldSymbol field, IGeneratorContext context, InterpreterFrame frame)
  {
    if (_operationManager.TryGetOperation(field, out IOperation? operation))
    {
      object? value = new InterpreterVisitor(context, frame).Visit(operation, default);
      frame.Define(field, value);
    }
    else
    {
      frame.Declare(field);
    }
  }

  private void InitProperty(IPropertySymbol property, IGeneratorContext context, InterpreterFrame frame)
  {
    if (_operationManager.TryGetOperation(property, out IOperation? operation))
    {
      object? value = new InterpreterVisitor(context, frame).Visit(operation, default);
      frame.Define(property, value);
    }
    else
    {
      frame.Declare(property);
    }
  }

  public InterpreterFrame GetTypeFrame(SymbolType type)
  {
    Debug.Assert(type.Symbol.IsSource());
    Debug.Assert(!type.ContainsGenericParameters);

    if (_typeFrames.TryGetValue(type, out InterpreterFrame? typeFrame))
      return typeFrame;

    InterpreterFrame? parentFrame = null;
    SymbolType? baseType = type.BaseType;
    if (baseType is not null && baseType.Symbol.IsSource())
    {
      parentFrame = GetTypeFrame(baseType);
    }

    InterpreterTypeContext typeContext = new InterpreterTypeContext(_runtime, _context, type.GetGenericArguments());
    typeFrame = InterpreterFrame.NewTypeFrame(parentFrame, typeContext, _frameProvider.GetValues());
    _typeFrames.Add(type, typeFrame);

    INamedTypeSymbol symbol = type.OriginalSymbol;
    IEnumerable<ISymbol> fields = symbol.GetMembers().Where(m => m.Kind is SymbolKind.Field && m.IsStatic);
    foreach (IFieldSymbol field in fields)
    {
      if (!field.IsImplicitlyDeclared)
      {
        InitField(field, typeContext, typeFrame);
      }
      else if (field.AssociatedSymbol is IPropertySymbol propertySymbol)
      {
        InitProperty(propertySymbol, typeContext, typeFrame);
      }
    }

    if (symbol.StaticConstructors.Length != 0)
    {
      IMethodSymbol constructor = symbol.StaticConstructors[0];
      if (!_operationManager.TryGetOperation(constructor, out IOperation? operation))
        throw new InvalidUserCodeException(); // TODO: Message

      InterpreterMethodContext methodContext = new InterpreterMethodContext(_runtime, typeContext, Type.EmptyTypes);
      InterpreterFrame methodFrame = InterpreterFrame.NewMethodFrame(typeFrame, _frameProvider.GetValues());

      new InterpreterVisitor(methodContext, methodFrame).Visit(operation, default);
    }

    return typeFrame;
  }

  public InterpreterFrame GetInstanceFrame(InterpreterFrame typeFrame, SymbolType type, object instance)
  {
    InterpreterFrame instanceFrame = InterpreterFrame.NewInstanceFrame(typeFrame, _frameProvider.GetValues(), instance);

    IEnumerable<ISymbol> fields = type.Symbol.GetMembers().Where(m => m.Kind is SymbolKind.Field && !m.IsStatic);
    foreach (IFieldSymbol field in fields)
    {
      if (!field.IsImplicitlyDeclared)
      {
        InitField(field, typeFrame.TypeContext, instanceFrame);
      }
      else if (field.AssociatedSymbol is IPropertySymbol propertySymbol)
      {
        InitProperty(propertySymbol, typeFrame.TypeContext, instanceFrame);
      }
    }

    return instanceFrame;
  }

  private InterpreterFrame GetMethodFrame(InterpreterFrame parent, IMethodSymbol method, object?[] arguments)
  {
    Debug.Assert(method.IsSource());

    ImmutableArray<IParameterSymbol> parameters = method.Parameters;
    int length = parameters.Length;
    if (arguments.Length != length)
      throw new ArgumentException($"Wrong number of arguments supplied to method {method}.", nameof(arguments));

    IDictionary<ISymbol, object?> values = _frameProvider.GetValues(arguments.Length);
    InterpreterFrame methodFrame = InterpreterFrame.NewMethodFrame(parent, values);
    for (int i = 0; i < length; i++)
    {
      methodFrame.Define(parameters[i], arguments[i]);
    }

    return methodFrame;
  }

  private InterpreterFrame GetConstructorFrame(InterpreterFrame typeFrame, IMethodSymbol constructor, object?[] arguments)
  {
    Debug.Assert(constructor.IsSource());

    ImmutableArray<IParameterSymbol> parameters = constructor.Parameters;
    int length = parameters.Length;
    if (arguments.Length != length)
      throw new ArgumentException($"Wrong number of arguments supplied to method {constructor}.", nameof(arguments));

    IDictionary<ISymbol, object?> values = _frameProvider.GetValues(arguments.Length);
    InterpreterFrame constructorFrame = InterpreterFrame.NewMethodFrame(typeFrame, values);
    for (int i = 0; i < length; i++)
    {
      constructorFrame.Define(parameters[i], arguments[i]);
    }

    return constructorFrame;
  }
}
