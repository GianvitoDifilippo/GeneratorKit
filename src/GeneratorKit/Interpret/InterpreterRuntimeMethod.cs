using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Interpret;

internal class InterpreterRuntimeMethod : IRuntimeMethod
{
  private readonly GeneratorRuntime _runtime;
  private readonly IMethodSymbol _symbol;
  private readonly InterpreterFrame _frame;

  public InterpreterRuntimeMethod(GeneratorRuntime runtime, IMethodSymbol symbol, InterpreterFrame frame)
  {
    _runtime = runtime;
    _symbol = symbol;
    _frame = frame;
  }

  public SymbolMethodInfo Definition => _runtime.CreateMethodInfoDelegator(_symbol.ConstructedFrom);

  public Type[] TypeArguments => _symbol.TypeArguments.Map(t => t.TypeKind is TypeKind.TypeParameter
    ? _frame.GetGenericArgument((ITypeParameterSymbol)t)
    : _runtime.CreateTypeDelegator(t));

  public IRuntimeType DeclaringType => new InterpreterRuntimeType(_runtime, _symbol.ContainingType, _frame);

  public bool IsStatic => Definition.IsStatic;

  public bool ContainsGenericParameters => false;

  public bool IsSource => true;

  public object? Invoke(object? instance, object?[] arguments) => _symbol.IsGenericMethod
    ? Definition.MakeGenericMethod(TypeArguments).Invoke(instance, arguments)
    : Definition.Invoke(instance, arguments);
}
