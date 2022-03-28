using GeneratorKit.Comparers;
using GeneratorKit.Exceptions;
using GeneratorKit.Interpret.Context;
using GeneratorKit.Interpret.Frame;
using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Interpret;

internal class Interpreter : IInterpreter
{
  private readonly GeneratorContext _context;
  private readonly IOperationManager _operationManager;
  private readonly IFrameProvider _frameProvider;
  private readonly Dictionary<SymbolType, InterpreterFrame> _typeFrames;

  public Interpreter(GeneratorContext context, IOperationManager operationManager, IFrameProvider frameProvider)
  {
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
    InterpreterMethodContext context = new InterpreterMethodContext(typeFrame.TypeContext, Type.EmptyTypes);

    return (object?[])new ConstructorInitializerVisitor(context, _frameProvider, constructorFrame).Visit(operation, default)!;
  }

  public object? InterpretMethod(IMethodSymbol method, InterpreterFrame frame, Type[] typeArguments, object?[] arguments)
  {
    if (!_operationManager.TryGetOperation(method.OriginalDefinition, out IOperation? operation))
      throw new InvalidUserCodeException(); // TODO: Message

    InterpreterFrame methodFrame = GetMethodFrame(frame, method, arguments);
    InterpreterMethodContext context = new InterpreterMethodContext(frame.TypeContext, typeArguments);

    return new InterpreterVisitor(context, _frameProvider, methodFrame).Visit(operation, default);
  }

  public void InterpretConstructor(IMethodSymbol constructor, InterpreterFrame frame, object?[] arguments)
  {
    if (!_operationManager.TryGetOperation(constructor.OriginalDefinition, out IOperation? operation))
      throw new InvalidUserCodeException(); // TODO: Message

    InterpreterFrame methodFrame = GetConstructorFrame(frame, constructor, arguments);
    InterpreterMethodContext context = new InterpreterMethodContext(frame.TypeContext, Type.EmptyTypes);

    new InterpreterVisitor(context, _frameProvider, methodFrame).Visit(operation, default);
  }

  private void InitField(IFieldSymbol field, IInterpreterContext context, InterpreterFrame frame)
  {
    object? value = _operationManager.TryGetOperation(field, out IOperation? operation)
      ? new InterpreterVisitor(context, _frameProvider, frame).Visit(operation, default)
      : _context.CreateTypeDelegator(field.Type).GetDefaultValue();

    frame.Define(field, value);
  }

  private void InitProperty(IPropertySymbol property, IInterpreterContext context, InterpreterFrame frame)
  {
    object? value = _operationManager.TryGetOperation(property, out IOperation? operation)
      ? new InterpreterVisitor(context, _frameProvider, frame).Visit(operation, default)
      : _context.CreateTypeDelegator(property.Type).GetDefaultValue();

    frame.Define(property, value);
  }

  public InterpreterFrame GetTypeFrame(SymbolNamedType type)
  {
    Debug.Assert(type.IsSource, "Type must be source.");
    Debug.Assert(!type.ContainsGenericParameters, "Type must not contain generic parameters.");

    if (_typeFrames.TryGetValue(type, out InterpreterFrame? typeFrame))
      return typeFrame;

    InterpreterFrame? parentFrame = null;
    SymbolNamedType? baseType = type.BaseType;
    if (baseType is not null && baseType.IsSource)
    {
      parentFrame = GetTypeFrame(baseType);
    }

    InterpreterTypeContext typeContext = new InterpreterTypeContext(_context, type.GetGenericArguments());
    typeFrame = InterpreterFrame.NewTypeFrame(parentFrame, typeContext, _frameProvider.GetFrame());
    _typeFrames.Add(type, typeFrame);

    INamedTypeSymbol symbol = type.Symbol;
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

      InterpreterMethodContext methodContext = new InterpreterMethodContext(typeContext, Type.EmptyTypes);
      InterpreterFrame methodFrame = InterpreterFrame.NewMethodFrame(typeFrame, _frameProvider.GetFrame());

      new InterpreterVisitor(methodContext, _frameProvider, methodFrame).Visit(operation, default);
    }

    return typeFrame;
  }

  public InterpreterFrame GetInstanceFrame(InterpreterFrame typeFrame, SymbolNamedType type, object instance)
  {
    InterpreterFrame instanceFrame = InterpreterFrame.NewInstanceFrame(typeFrame, _frameProvider.GetFrame(), instance);

    IEnumerable<ISymbol> fields = type.Symbol.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, MemberFlags.Fields | MemberFlags.DeepSearch, true);
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
    Debug.Assert(method.IsSource(), "Method must be source.");

    ImmutableArray<IParameterSymbol> parameters = method.Parameters;
    int length = parameters.Length;
    if (arguments.Length != length)
      throw new ArgumentException($"Wrong number of arguments supplied to method {method}.", nameof(arguments));

    IDictionary<ISymbol, object?> values = _frameProvider.GetFrame(arguments.Length);
    InterpreterFrame methodFrame = InterpreterFrame.NewMethodFrame(parent, values);
    for (int i = 0; i < length; i++)
    {
      methodFrame.Define(parameters[i], arguments[i]);
    }

    return methodFrame;
  }

  private InterpreterFrame GetConstructorFrame(InterpreterFrame typeFrame, IMethodSymbol constructor, object?[] arguments)
  {
    Debug.Assert(constructor.IsSource(), "Constructor must be source.");

    ImmutableArray<IParameterSymbol> parameters = constructor.Parameters;
    int length = parameters.Length;
    if (arguments.Length != length)
      throw new ArgumentException($"Wrong number of arguments supplied to method {constructor}.", nameof(arguments));

    IDictionary<ISymbol, object?> values = _frameProvider.GetFrame(arguments.Length);
    InterpreterFrame constructorFrame = InterpreterFrame.NewMethodFrame(typeFrame, values);
    for (int i = 0; i < length; i++)
    {
      constructorFrame.Define(parameters[i], arguments[i]);
    }

    return constructorFrame;
  }
}
