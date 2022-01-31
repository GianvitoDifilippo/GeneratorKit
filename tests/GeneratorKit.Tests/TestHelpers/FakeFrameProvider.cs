using GeneratorKit.Interpret;
using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Xunit;

namespace GeneratorKit.TestHelpers;

internal class FakeFrameProvider : IFrameProvider
{
  private readonly Dictionary<string, object?> _values;

  public FakeFrameProvider()
  {
    _values = new Dictionary<string, object?>();
  }

  public IReadOnlyDictionary<string, object?> Values => _values;

  public InterpreterFrame GetClassFrame(IRuntimeType type)
  {
    return InterpreterFrame.NewClassFrame(null, new Dictionary<string, object?>(), type.TypeArguments);
  }

  public InterpreterFrame GetConstructorFrame(InterpreterFrame classFrame, SymbolConstructorInfo constructor, object?[] arguments)
  {
    ImmutableArray<IParameterSymbol> parameters = constructor.Symbol.Parameters;
    Assert.Equal(parameters.Length, arguments.Length);
    for (int i = 0; i < arguments.Length; i++)
    {
      _values[parameters[i].Name] = arguments[i];
    }
    return InterpreterFrame.NewMethodFrame(classFrame, _values, Type.EmptyTypes);
  }

  public InterpreterFrame GetInstanceFrame(InterpreterFrame classFrame, object instance)
  {
    return InterpreterFrame.NewInstanceFrame(classFrame, new Dictionary<string, object?>(), instance);
  }

  public InterpreterFrame GetMethodFrame(InterpreterFrame parent, IRuntimeMethod method, object?[] arguments)
  {
    ImmutableArray<IParameterSymbol> parameters = method.Definition.Symbol.Parameters;
    Assert.Equal(parameters.Length, arguments.Length);
    for (int i = 0; i < arguments.Length; i++)
    {
      _values[parameters[i].Name] = arguments[i];
    }
    return InterpreterFrame.NewMethodFrame(parent, _values, method.TypeArguments);
  }

  public InterpreterFrame NewScopeFrame(InterpreterFrame parent)
  {
    return InterpreterFrame.NewScopeFrame(parent, new Dictionary<string, object?>());
  }
}