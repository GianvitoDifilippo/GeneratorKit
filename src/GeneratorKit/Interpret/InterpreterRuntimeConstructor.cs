using GeneratorKit.Reflection;
using GeneratorKit.Reflection.Binders;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace GeneratorKit.Interpret;

internal class InterpreterRuntimeConstructor : IRuntimeConstructor
{
  private readonly GeneratorRuntime _runtime;
  private readonly IMethodSymbol _symbol;
  private readonly InterpreterFrame _frame;
  private IRuntimeType? _declaringType;
  private ConstructorInfo? _underlyingSystemConstructor;

  public InterpreterRuntimeConstructor(GeneratorRuntime runtime, IMethodSymbol symbol, InterpreterFrame frame)
  {
    _runtime = runtime;
    _symbol = symbol;
    _frame = frame;
  }

  public IMethodSymbol Symbol => _symbol;

  public IRuntimeType DeclaringType => _declaringType ??= new InterpreterRuntimeType(_runtime, _symbol.ContainingType, _frame);

  public bool IsSource => _symbol.IsSource();

  public ConstructorInfo UnderlyingSystemConstructor => _underlyingSystemConstructor ??= DelegatorBinder.ResolveConstructor(DeclaringType.UnderlyingSystemType, _runtime.CreateConstructorInfoDelegator(_symbol));
}
