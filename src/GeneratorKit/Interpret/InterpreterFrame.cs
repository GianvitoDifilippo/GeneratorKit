using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GeneratorKit.Interpret;

public abstract class InterpreterFrame
{
  private readonly Dictionary<string, object?> _values;

  public InterpreterFrame(Dictionary<string, object?> values)
  {
    _values = values;
  }

  public abstract object? Instance { get; }

  public abstract Type GetGenericArgument(ITypeParameterSymbol parameter);

  protected abstract bool TryAssignToParent(string name, object? value);

  protected abstract bool TryGetFromParent(string name, out object? value);

  protected abstract bool IsDefinedInParent(string name);

  public void Declare(string name)
  {
    _values[name] = Unit.Instance;
  }

  public void Define(string name, object? value)
  {
    if (IsDefined(name))
    {
      throw new InvalidOperationException($"A local variable or function named '{name}' is already defined in this scope.");
    }
    _values[name] = value;
  }

  public void DefineOrAssign(string name, object? value)
  {
    if (IsDefined(name))
    {
      Assign(name, value);
      return;
    }
    Define(name, value);
  }

  public void Assign(string name, object? value)
  {
    if (!TryAssign(name, value))
    {
      throw new InvalidOperationException($"The symbol {name} does not exist in the current context.");
    }
  }

  public object? Get(string name)
  {
    if (!TryGet(name, out object? value))
    {
      throw new InvalidOperationException($"The symbol {name} does not exist in the current context.");
    }
    if (ReferenceEquals(value, Unit.Instance))
    {
      throw new InvalidOperationException($"Use of unassigned local variable '{name}'.");
    }

    return value;
  }

  public bool IsDefined(string name)
  {
    return _values.ContainsKey(name) || IsDefinedInParent(name);
  }

  private bool TryGet(string name, out object? value)
  {
    if (_values.ContainsKey(name))
    {
      value = _values[name];
      return true;
    }

    return TryGetFromParent(name, out value);
  }

  private bool TryAssign(string name, object? value)
  {
    if (_values.ContainsKey(name))
    {
      _values[name] = value;
      return true;
    }
    return TryAssignToParent(name, value);
  }

  public static InterpreterFrame NewClassFrame(InterpreterFrame? parent, Dictionary<string, object?> values, Type[] typeArguments)
  {
    return new ClassFrame(parent, values, typeArguments);
  }

  public static InterpreterFrame NewInstanceFrame(InterpreterFrame parent, Dictionary<string, object?> values, object instance)
  {
    return new InstanceFrame(parent, values, instance);
  }

  public static InterpreterFrame NewMethodFrame(InterpreterFrame parent, Dictionary<string, object?> values, Type[] typeArguments)
  {
    return new MethodFrame(parent, values, typeArguments);
  }

  public static InterpreterFrame NewScopeFrame(InterpreterFrame parent, Dictionary<string, object?> values)
  {
    return new ScopeFrame(parent, values);
  }

  private class ClassFrame : InterpreterFrame
  {
    private readonly InterpreterFrame? _parent;
    private readonly Type[] _typeArguments;

    public ClassFrame(InterpreterFrame? parent, Dictionary<string, object?> values, Type[] typeArguments)
      : base(values)
    {
      _parent = parent;
      _typeArguments = typeArguments;
    }

    public override object? Instance => null;

    public override Type GetGenericArgument(ITypeParameterSymbol parameter)
    {
      Debug.Assert(parameter.TypeParameterKind is TypeParameterKind.Type);

      return _typeArguments[parameter.Ordinal];
    }

    protected override bool IsDefinedInParent(string symbol)
    {
      return _parent is not null && _parent.IsDefined(symbol);
    }

    protected override bool TryAssignToParent(string symbol, object? value)
    {
      return _parent is not null && _parent.TryAssign(symbol, value);
    }

    protected override bool TryGetFromParent(string symbol, out object? value)
    {
      if (_parent is not null)
        return _parent.TryGet(symbol, out value);

      value = null;
      return false;
    }
  }

  private class InstanceFrame : InterpreterFrame
  {
    private readonly InterpreterFrame _parent;

    public InstanceFrame(InterpreterFrame parent, Dictionary<string, object?> values, object instance)
      : base(values)
    {
      _parent = parent;
      Instance = instance;
    }

    public override object? Instance { get; }

    public override Type GetGenericArgument(ITypeParameterSymbol parameter)
    {
      return _parent.GetGenericArgument(parameter);
    }

    protected override bool IsDefinedInParent(string symbol)
    {
      return _parent.IsDefined(symbol);
    }

    protected override bool TryAssignToParent(string symbol, object? value)
    {
      return _parent.TryAssign(symbol, value);
    }

    protected override bool TryGetFromParent(string symbol, out object? value)
    {
      return _parent.TryGet(symbol, out value);
    }
  }

  private class MethodFrame : InterpreterFrame
  {
    private readonly InterpreterFrame _parent;
    private readonly Type[] _typeArguments;

    public MethodFrame(InterpreterFrame parent, Dictionary<string, object?> values, Type[] typeArguments)
      : base(values)
    {
      _parent = parent;
      _typeArguments = typeArguments;
      Instance = parent.Instance;
    }

    public override object? Instance { get; }

    public override Type GetGenericArgument(ITypeParameterSymbol parameter)
    {
      if (parameter.TypeParameterKind is TypeParameterKind.Method)
        return _typeArguments[parameter.Ordinal];

      return _parent.GetGenericArgument(parameter);
    }

    protected override bool IsDefinedInParent(string symbol)
    {
      return _parent.IsDefined(symbol);
    }

    protected override bool TryAssignToParent(string symbol, object? value)
    {
      return _parent.TryAssign(symbol, value);
    }

    protected override bool TryGetFromParent(string symbol, out object? value)
    {
      return _parent.TryGet(symbol, out value);
    }
  }

  private class ScopeFrame : InterpreterFrame
  {
    private readonly InterpreterFrame _parent;

    public ScopeFrame(InterpreterFrame parent, Dictionary<string, object?> values)
      : base(values)
    {
      _parent = parent;
      Instance = parent.Instance;
    }

    public override object? Instance { get; }

    public override Type GetGenericArgument(ITypeParameterSymbol parameter)
    {
      return _parent.GetGenericArgument(parameter);
    }

    protected override bool IsDefinedInParent(string symbol)
    {
      return _parent.IsDefined(symbol);
    }

    protected override bool TryAssignToParent(string symbol, object? value)
    {
      return _parent.TryAssign(symbol, value);
    }

    protected override bool TryGetFromParent(string symbol, out object? value)
    {
      return _parent.TryGet(symbol, out value);
    }
  }
}