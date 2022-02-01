using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GeneratorKit.Interpret;

public abstract class InterpreterFrame
{
  private readonly IDictionary<ISymbol, object?> _values;

  public InterpreterFrame(IDictionary<ISymbol, object?> values)
  {
    _values = values;
  }

  public abstract object? Instance { get; }

  public abstract Type GetGenericArgument(ITypeParameterSymbol parameter);

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

  public static InterpreterFrame NewClassFrame(InterpreterFrame? parent, IDictionary<ISymbol, object?> values, Type[] typeArguments)
  {
    return new ClassFrame(parent, values, typeArguments);
  }

  public static InterpreterFrame NewInstanceFrame(InterpreterFrame parent, IDictionary<ISymbol, object?> values, object instance)
  {
    return new InstanceFrame(parent, values, instance);
  }

  public static InterpreterFrame NewMethodFrame(InterpreterFrame parent, IDictionary<ISymbol, object?> values, Type[] typeArguments)
  {
    return new MethodFrame(parent, values, typeArguments);
  }

  public static InterpreterFrame NewScopeFrame(InterpreterFrame parent, IDictionary<ISymbol, object?> values)
  {
    return new ScopeFrame(parent, values);
  }

  private class ClassFrame : InterpreterFrame
  {
    private readonly InterpreterFrame? _parent;
    private readonly Type[] _typeArguments;

    public ClassFrame(InterpreterFrame? parent, IDictionary<ISymbol, object?> values, Type[] typeArguments)
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

    public override object? Instance { get; }

    public override Type GetGenericArgument(ITypeParameterSymbol parameter)
    {
      return _parent.GetGenericArgument(parameter);
    }

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
    private readonly Type[] _typeArguments;

    public MethodFrame(InterpreterFrame parent, IDictionary<ISymbol, object?> values, Type[] typeArguments)
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

    public override object? Instance { get; }

    public override Type GetGenericArgument(ITypeParameterSymbol parameter)
    {
      return _parent.GetGenericArgument(parameter);
    }

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