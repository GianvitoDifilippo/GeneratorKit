using GeneratorKit.Reflection;
using GeneratorKit.Reflection.Binders;
using Microsoft.CodeAnalysis;
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

  public bool IsSource => _symbol.IsSource();

  public bool IsStatic => _symbol.IsStatic;

  public IRuntimeType DeclaringType => _declaringType ??= new InterpreterRuntimeType(_runtime, _symbol.ContainingType, _frame);

  public PropertyInfo UnderlyingSystemProperty => _underlyingSystemProperty ??= DelegatorBinder.ResolveProperty(DeclaringType.UnderlyingSystemType, _runtime.CreatePropertyInfoDelegator(_symbol));

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
