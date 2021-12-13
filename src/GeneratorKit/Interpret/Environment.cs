#pragma warning disable RS1024 // Compare symbols correctly

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace GeneratorKit.Interpret;

internal class Environment
{
  private static readonly Undefined s_undefined = Undefined.Instance;

  private readonly Environment? _enclosing;
  private readonly Dictionary<ISymbol, object?> _values;

  public Environment(object? containingTypeInstance)
  {
    _values = new Dictionary<ISymbol, object?>(SymbolEqualityComparer.Default);
    ContainingTypeInstance = containingTypeInstance;
  }

  public Environment(Environment enclosing)
    : this(enclosing.ContainingTypeInstance)
  {
    _enclosing = enclosing;
  }

  public object? ContainingTypeInstance { get; }

  public void Declare(ISymbol name)
  {
    _values[name] = s_undefined;
  }

  public void Define(ISymbol name, object? value)
  {
    if (IsDefined(name))
    {
      throw new InvalidOperationException($"A local variable or function named '{name.Name}' is already defined in this scope.");
    }
    _values[name] = value;
  }

  public void DefineOrAssign(ISymbol name, object? value)
  {
    if (IsDefined(name))
    {
      Assign(name, value);
      return;
    }
    Define(name, value);
  }

  public void Assign(ISymbol name, object? value)
  {
    if (_values.ContainsKey(name))
    {
      _values[name] = value;
      return;
    }
    if (_enclosing is not null)
    {
      _enclosing.Assign(name, value);
      return;
    }

    throw new InvalidOperationException($"The name {name.Name} does not exist in the current context.");
  }

  public object? Get(ISymbol name)
  {
    if (_values.ContainsKey(name))
    {
      object? value = _values[name];
      if (ReferenceEquals(value, s_undefined))
      {
        throw new InvalidOperationException($"Use of unassigned local variable '{name.Name}'.");
      }

      return value;
    }
    if (_enclosing is not null)
    {
      return _enclosing.Get(name);
    }

    throw new InvalidOperationException($"The name {name.Name} does not exist in the current context.");
  }

  private bool IsDefined(ISymbol name)
  {
    return _values.ContainsKey(name) || _enclosing is not null && _enclosing.IsDefined(name);
  }

  private class Undefined
  {
    public static readonly Undefined Instance = new Undefined();

    private Undefined() { }

    public override string ToString() => "<undefined>";
  }
}