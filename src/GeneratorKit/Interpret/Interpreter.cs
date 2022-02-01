using GeneratorKit.Comparers;
using GeneratorKit.Exceptions;
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
  private readonly GeneratorRuntime _runtime;
  private readonly IOperationManager _operationManager;
  private readonly IFrameProvider _frameProvider;
  private readonly Dictionary<IRuntimeType, InterpreterFrame> _classFrames;

  public Interpreter(GeneratorRuntime runtime, IOperationManager operationManager, IFrameProvider frameProvider)
  {
    _runtime = runtime;
    _operationManager = operationManager;
    _frameProvider = frameProvider;
    _classFrames = new Dictionary<IRuntimeType, InterpreterFrame>(RuntimeTypeEqualityComparer.Default);
  }

  public object?[] GetProxyArguments(IRuntimeConstructor constructor, InterpreterFrame classFrame, object?[] arguments)
  {
    if (!_operationManager.TryGetOperation(constructor.Symbol, out IOperation? operation))
      throw new InvalidUserCodeException(); // TODO: Message

    InterpreterFrame constructorFrame = GetConstructorFrame(classFrame, constructor, arguments);

    return (object?[])new ConstructorInitializerVisitor(_runtime, constructorFrame).Visit(operation, default)!;
  }

  public object? Interpret(IRuntimeMethod method, InterpreterFrame frame, object?[] arguments)
  {
    if (!_operationManager.TryGetOperation(method.Definition.Symbol, out IOperation? operation))
      throw new InvalidUserCodeException(); // TODO: Message

    InterpreterFrame methodFrame = GetMethodFrame(frame, method, arguments);

    return new InterpreterVisitor(_runtime, methodFrame).Visit(operation, default);
  }

  public void Interpret(IRuntimeConstructor constructor, InterpreterFrame frame, object?[] arguments)
  {
    if (!_operationManager.TryGetOperation(constructor.Symbol, out IOperation? operation))
      throw new InvalidUserCodeException(); // TODO: Message

    InterpreterFrame methodFrame = GetConstructorFrame(frame, constructor, arguments);

    new InterpreterVisitor(_runtime, methodFrame).Visit(operation, default);
  }

  private void InitField(IFieldSymbol field, InterpreterFrame frame)
  {
    if (_operationManager.TryGetOperation(field, out IOperation? operation))
    {
      object? value = new InterpreterVisitor(_runtime, frame).Visit(operation, default);
      frame.Define(field, value);
    }
    else
    {
      frame.Declare(field);
    }
  }

  private void InitProperty(IPropertySymbol property, InterpreterFrame frame)
  {
    if (_operationManager.TryGetOperation(property, out IOperation? operation))
    {
      object? value = new InterpreterVisitor(_runtime, frame).Visit(operation, default);
      frame.Define(property, value);
    }
    else
    {
      frame.Declare(property);
    }
  }

  public InterpreterFrame GetClassFrame(IRuntimeType type)
  {
    Debug.Assert(type.IsSource);
    Debug.Assert(!type.ContainsGenericParameters);

    if (_classFrames.TryGetValue(type, out InterpreterFrame? classFrame))
      return classFrame;

    InterpreterFrame? parentFrame = null;
    IRuntimeType? baseType = type.BaseType;
    if (baseType is not null && baseType.IsSource)
    {
      parentFrame = GetClassFrame(baseType);
    }

    classFrame = InterpreterFrame.NewClassFrame(parentFrame, _frameProvider.GetValues(), type.TypeArguments);

    INamedTypeSymbol symbol = type.Definition.Symbol;
    IEnumerable<ISymbol> fields = symbol.GetMembers().Where(m => m.Kind is SymbolKind.Field && m.IsStatic);
    foreach (IFieldSymbol field in fields)
    {
      if (!field.IsImplicitlyDeclared)
      {
        InitField(field, classFrame);
      }
      else if (field.AssociatedSymbol is IPropertySymbol propertySymbol)
      {
        InitProperty(propertySymbol, classFrame);
      }
    }

    if (symbol.StaticConstructors.Length != 0)
    {
      IMethodSymbol constructor = symbol.StaticConstructors[0];
      if (!_operationManager.TryGetOperation(constructor, out IOperation? operation))
        throw new InvalidUserCodeException(); // TODO: Message

      InterpreterFrame methodFrame = InterpreterFrame.NewMethodFrame(classFrame, _frameProvider.GetValues(), Type.EmptyTypes);

      new InterpreterVisitor(_runtime, methodFrame).Visit(operation, default);
    }

    _classFrames.Add(type, classFrame);

    return classFrame;
  }

  public InterpreterFrame GetInstanceFrame(InterpreterFrame classFrame, IRuntimeType type, object instance)
  {
    Debug.Assert(type.IsSource);
    
    InterpreterFrame instanceFrame = InterpreterFrame.NewInstanceFrame(classFrame, _frameProvider.GetValues(), instance);

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

    return instanceFrame;
  }

  private InterpreterFrame GetMethodFrame(InterpreterFrame parent, IRuntimeMethod method, object?[] arguments)
  {
    Debug.Assert(method.IsSource);
    Debug.Assert(!method.IsOpenGeneric);

    ImmutableArray<IParameterSymbol> parameters = method.Definition.Symbol.Parameters;
    int length = parameters.Length;
    if (arguments.Length != length)
      throw new ArgumentException($"Wrong number of arguments supplied to method {method}.", nameof(arguments));

    IDictionary<ISymbol, object?> values = _frameProvider.GetValues(arguments.Length);
    InterpreterFrame methodFrame = InterpreterFrame.NewMethodFrame(parent, values, method.TypeArguments);
    for (int i = 0; i < length; i++)
    {
      methodFrame.Define(parameters[i], arguments[i]);
    }

    return methodFrame;
  }

  private InterpreterFrame GetConstructorFrame(InterpreterFrame classFrame, IRuntimeConstructor constructor, object?[] arguments)
  {
    Debug.Assert(constructor.IsSource);

    ImmutableArray<IParameterSymbol> parameters = constructor.Symbol.Parameters;
    int length = parameters.Length;
    if (arguments.Length != length)
      throw new ArgumentException($"Wrong number of arguments supplied to method {constructor}.", nameof(arguments));

    IDictionary<ISymbol, object?> values = _frameProvider.GetValues(arguments.Length);
    InterpreterFrame constructorFrame = InterpreterFrame.NewMethodFrame(classFrame, values, Type.EmptyTypes);
    for (int i = 0; i < length; i++)
    {
      constructorFrame.Define(parameters[i], arguments[i]);
    }

    return constructorFrame;
  }

  private class RuntimeTypeEqualityComparer : IEqualityComparer<IRuntimeType>
  {
    public static RuntimeTypeEqualityComparer Default = new RuntimeTypeEqualityComparer();

    private RuntimeTypeEqualityComparer() { }

    public bool Equals(IRuntimeType t1, IRuntimeType t2)
    {
      if (ReferenceEquals(t1, t2))
        return true;

      if (!SymbolEqualityComparer.Default.Equals(t1.Definition.Symbol, t2.Definition.Symbol))
        return false;

      Type[] typeArguments1 = t1.TypeArguments;
      Type[] typeArguments2 = t2.TypeArguments;

      if (typeArguments1.Length != typeArguments2.Length)
        return false;

      for (int i = 0; i < typeArguments1.Length; i++)
      {
        if (!TypeEqualityComparer.Default.Equals(typeArguments1[i], typeArguments2[i]))
          return false;
      }

      return true;
    }

    public int GetHashCode(IRuntimeType obj)
    {
      unchecked
      {
        int hashCode = 391 + SymbolEqualityComparer.Default.GetHashCode(obj.Definition.Symbol);
        foreach (Type typeArgument in obj.TypeArguments)
        {
          hashCode = hashCode * 23 + TypeEqualityComparer.Default.GetHashCode(typeArgument);
        }

        return hashCode;
      }
    }
  }
}
