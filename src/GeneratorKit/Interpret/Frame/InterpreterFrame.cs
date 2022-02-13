using GeneratorKit.Interpret.Context;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace GeneratorKit.Interpret.Frame;

internal abstract class InterpreterFrame
{
  private readonly IDictionary<ISymbol, object?> _values;

  private InterpreterFrame(IDictionary<ISymbol, object?> values)
  {
    _values = values;
  }

  public abstract InterpreterContext TypeContext { get; }

  public abstract object? Instance { get; }

  protected abstract bool TryAssignToParent(ISymbol symbol, object? value);

  protected abstract bool TryGetFromParent(ISymbol symbol, out object? value);

  protected abstract bool IsDefinedInParent(ISymbol symbol);

  public void Declare(ISymbol symbol)
  {
    _values[symbol] = Unit.Instance;
  }

  public void Define(ISymbol symbol, object? value)
  {
    if (IsDefined(symbol))
    {
      throw new InvalidOperationException($"A local variable or function named '{symbol.Name}' is already defined in this scope.");
    }
    _values[symbol] = value;
  }

  public void DefineOrAssign(ISymbol symbol, object? value)
  {
    if (IsDefined(symbol))
    {
      Assign(symbol, value);
      return;
    }
    Define(symbol, value);
  }

  public void Assign(ISymbol symbol, object? value)
  {
    if (!TryAssign(symbol, value))
    {
      throw new InvalidOperationException($"The symbol {symbol.Name} does not exist in the current context.");
    }
  }

  public object? Get(ISymbol symbol)
  {
    if (!TryGet(symbol, out object? value))
    {
      throw new InvalidOperationException($"The symbol {symbol.Name} does not exist in the current context.");
    }
    if (ReferenceEquals(value, Unit.Instance))
    {
      throw new InvalidOperationException($"Use of unassigned local variable '{symbol.Name}'.");
    }

    return value;
  }

  public bool IsDefined(ISymbol symbol)
  {
    return _values.ContainsKey(symbol) || IsDefinedInParent(symbol);
  }

  private bool TryGet(ISymbol symbol, out object? value)
  {
    if (_values.ContainsKey(symbol))
    {
      value = _values[symbol];
      return true;
    }

    return TryGetFromParent(symbol, out value);
  }

  private bool TryAssign(ISymbol symbol, object? value)
  {
    if (_values.ContainsKey(symbol))
    {
      _values[symbol] = value;
      return true;
    }
    return TryAssignToParent(symbol, value);
  }

  public static InterpreterFrame NewTypeFrame(InterpreterFrame? parent, InterpreterContext typeContext, IDictionary<ISymbol, object?> values)
  {
    return new TypeFrame(parent, typeContext, values);
  }

  public static InterpreterFrame NewInstanceFrame(InterpreterFrame parent, IDictionary<ISymbol, object?> values, object instance)
  {
    return new InstanceFrame(parent, values, instance);
  }

  public static InterpreterFrame NewMethodFrame(InterpreterFrame parent, IDictionary<ISymbol, object?> values)
  {
    return new MethodFrame(parent, values);
  }

  public static InterpreterFrame NewScopeFrame(InterpreterFrame parent, IDictionary<ISymbol, object?> values)
  {
    return new ScopeFrame(parent, values);
  }

  private class TypeFrame : InterpreterFrame
  {
    private readonly InterpreterFrame? _parent;

    public TypeFrame(InterpreterFrame? parent, InterpreterContext typeContext, IDictionary<ISymbol, object?> values)
      : base(values)
    {
      _parent = parent;
      TypeContext = typeContext;
    }

    public override InterpreterContext TypeContext { get; }

    public override object? Instance => null;

    protected override bool IsDefinedInParent(ISymbol symbol)
    {
      return _parent is not null && _parent.IsDefined(symbol);
    }

    protected override bool TryAssignToParent(ISymbol symbol, object? value)
    {
      return _parent is not null && _parent.TryAssign(symbol, value);
    }

    protected override bool TryGetFromParent(ISymbol symbol, out object? value)
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

    public InstanceFrame(InterpreterFrame parent, IDictionary<ISymbol, object?> values, object instance)
      : base(values)
    {
      _parent = parent;
      Instance = instance;
    }

    public override InterpreterContext TypeContext => _parent.TypeContext;

    public override object? Instance { get; }

    protected override bool IsDefinedInParent(ISymbol symbol)
    {
      return _parent.IsDefined(symbol);
    }

    protected override bool TryAssignToParent(ISymbol symbol, object? value)
    {
      return _parent.TryAssign(symbol, value);
    }

    protected override bool TryGetFromParent(ISymbol symbol, out object? value)
    {
      return _parent.TryGet(symbol, out value);
    }
  }

  private class MethodFrame : InterpreterFrame
  {
    private readonly InterpreterFrame _parent;

    public MethodFrame(InterpreterFrame parent, IDictionary<ISymbol, object?> values)
      : base(values)
    {
      _parent = parent;
      Instance = parent.Instance;
    }

    public override InterpreterContext TypeContext => throw new NotSupportedException();

    public override object? Instance { get; }

    protected override bool IsDefinedInParent(ISymbol symbol)
    {
      return _parent.IsDefined(symbol);
    }

    protected override bool TryAssignToParent(ISymbol symbol, object? value)
    {
      return _parent.TryAssign(symbol, value);
    }

    protected override bool TryGetFromParent(ISymbol symbol, out object? value)
    {
      return _parent.TryGet(symbol, out value);
    }
  }

  private class ScopeFrame : InterpreterFrame
  {
    private readonly InterpreterFrame _parent;

    public ScopeFrame(InterpreterFrame parent, IDictionary<ISymbol, object?> values)
      : base(values)
    {
      _parent = parent;
      Instance = parent.Instance;
    }

    public override InterpreterContext TypeContext => throw new NotSupportedException();

    public override object? Instance { get; }

    protected override bool IsDefinedInParent(ISymbol symbol)
    {
      return _parent.IsDefined(symbol);
    }

    protected override bool TryAssignToParent(ISymbol symbol, object? value)
    {
      return _parent.TryAssign(symbol, value);
    }

    protected override bool TryGetFromParent(ISymbol symbol, out object? value)
    {
      return _parent.TryGet(symbol, out value);
    }
  }
}