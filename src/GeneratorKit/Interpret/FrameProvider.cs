using GeneratorKit.Comparers;
using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace GeneratorKit.Interpret;

internal class FrameProvider : IFrameProvider
{
  private readonly Dictionary<IRuntimeType, InterpreterFrame> _classFrames;

  public FrameProvider()
  {
    _classFrames = new Dictionary<IRuntimeType, InterpreterFrame>(new RuntimeTypeEqualityComparer());
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

    classFrame = InterpreterFrame.NewClassFrame(parentFrame, CreateValues(), type.TypeArguments);
    _classFrames.Add(type, classFrame);

    return classFrame;
  }

  public InterpreterFrame GetInstanceFrame(InterpreterFrame classFrame, object instance)
  {
    return InterpreterFrame.NewInstanceFrame(classFrame, CreateValues(), instance);
  }

  public InterpreterFrame GetMethodFrame(InterpreterFrame parent, IRuntimeMethod method, object?[] arguments)
  {
    Debug.Assert(method.IsSource);
    Debug.Assert(!method.ContainsGenericParameters);

    ImmutableArray<IParameterSymbol> parameters = method.Definition.Symbol.Parameters;
    int length = parameters.Length;
    if (arguments.Length != length)
      throw new ArgumentException($"Wrong number of arguments supplied to method {method}.", nameof(arguments));

    Dictionary<string, object?> values = CreateValues(arguments.Length);
    for (int i = 0; i < length; i++)
    {
      values[parameters[i].Name] = arguments[i];
    }

    return InterpreterFrame.NewMethodFrame(parent, values, method.TypeArguments);
  }

  public InterpreterFrame GetConstructorFrame(InterpreterFrame classFrame, SymbolConstructorInfo constructor, object?[] arguments)
  {
    ImmutableArray<IParameterSymbol> parameters = constructor.Symbol.Parameters;
    int length = parameters.Length;
    if (arguments.Length != length)
      throw new ArgumentException($"Wrong number of arguments supplied to method {constructor}.", nameof(arguments));

    Dictionary<string, object?> values = CreateValues(arguments.Length);
    for (int i = 0; i < length; i++)
    {
      values[parameters[i].Name] = arguments[i];
    }

    return InterpreterFrame.NewMethodFrame(classFrame, values, Type.EmptyTypes);
  }

  public InterpreterFrame NewScopeFrame(InterpreterFrame parent)
  {
    return InterpreterFrame.NewScopeFrame(parent, CreateValues());
  }

  private static Dictionary<string, object?> CreateValues(int capacity = 0)
  {
    return new Dictionary<string, object?>(capacity);
  }

  private class RuntimeTypeEqualityComparer : IEqualityComparer<IRuntimeType>
  {
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
