using GeneratorKit.Reflection;
using GeneratorKit.Reflection.Binders;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace GeneratorKit.Interpret;

internal class InterpreterRuntimeField : IRuntimeField
{
  private readonly GeneratorRuntime _runtime;
  private readonly InterpreterFrame _frame;
  private InterpreterRuntimeType? _declaringType;
  private FieldInfo? _underlyingSystemField;

  public InterpreterRuntimeField(GeneratorRuntime runtime, IFieldSymbol symbol, InterpreterFrame frame)
  {
    _runtime = runtime;
    Symbol = symbol;
    _frame = frame;
  }

  public IFieldSymbol Symbol { get; }

  public IRuntimeType DeclaringType => _declaringType ??= new InterpreterRuntimeType(_runtime, Symbol.ContainingType, _frame);

  public bool IsStatic => Symbol.IsStatic;

  public FieldInfo UnderlyingSystemField => _underlyingSystemField ??= DelegatorBinder.ResolveField(DeclaringType.RuntimeType.UnderlyingSystemType, this);

  public string Name => Symbol.Name;
}
