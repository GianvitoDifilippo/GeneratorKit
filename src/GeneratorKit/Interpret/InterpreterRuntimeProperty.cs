using GeneratorKit.Reflection;
using GeneratorKit.Reflection.Binders;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Interpret;

internal class InterpreterRuntimeProperty : IRuntimeProperty
{
  private readonly GeneratorRuntime _runtime;
  private readonly IPropertySymbol _symbol;
  private readonly InterpreterFrame _frame;
  private InterpreterRuntimeMethod? _getter;
  private InterpreterRuntimeMethod? _setter;
  private InterpreterRuntimeType? _declaringType;
  private Type[]? _parameterTypes;
  private Type? _propertyType;
  private PropertyInfo? _underlyingSystemProperty;

  public InterpreterRuntimeProperty(GeneratorRuntime runtime, IPropertySymbol symbol, InterpreterFrame frame)
  {
    _runtime = runtime;
    _symbol = symbol;
    _frame = frame;
  }

  public IRuntimeMethod? Getter => _getter ??= GetGetter();

  public IRuntimeMethod? Setter => _setter ??= GetSetter();

  public IPropertySymbol Symbol => _symbol;

  public bool IsStatic => _symbol.IsStatic;

  public IRuntimeType DeclaringType => _declaringType ??= new InterpreterRuntimeType(_runtime, _symbol.ContainingType, _frame);

  public PropertyInfo UnderlyingSystemProperty => _underlyingSystemProperty ??= DelegatorBinder.ResolveProperty(DeclaringType.RuntimeType.UnderlyingSystemType, this);

  public string Name => Symbol.Name is "this[]" ? "Item" : Symbol.Name;

  public Type[] ParameterTypes => _parameterTypes ??= Symbol.Parameters.Map(p => p.Type.TypeKind is TypeKind.TypeParameter
    ? _frame.GetGenericArgument((ITypeParameterSymbol)p.Type)
    : _runtime.CreateTypeDelegator(p.Type));

  public Type PropertyType => _propertyType ??= Symbol.Type.TypeKind is TypeKind.TypeParameter
    ? _frame.GetGenericArgument((ITypeParameterSymbol)Symbol.Type)
    : _runtime.CreateTypeDelegator(Symbol.Type);

  private InterpreterRuntimeMethod? GetGetter()
  {
    return _symbol.GetMethod is IMethodSymbol getMethod
      ? new InterpreterRuntimeMethod(_runtime, getMethod, _frame)
      : null;
  }

  private InterpreterRuntimeMethod? GetSetter()
  {
    return _symbol.SetMethod is IMethodSymbol setMethod
      ? new InterpreterRuntimeMethod(_runtime, setMethod, _frame)
      : null;
  }
}
