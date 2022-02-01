using GeneratorKit.Reflection;
using GeneratorKit.Reflection.Binders;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Interpret;

internal class InterpreterRuntimeMethod : IRuntimeMethod
{
  private readonly GeneratorRuntime _runtime;
  private readonly IMethodSymbol _symbol;
  private readonly InterpreterFrame _frame;
  private SymbolMethodInfo? _definition;
  private Type[]? _typeArguments;
  private InterpreterRuntimeType? _declaringType;
  private MethodInfo? _underlyingSystemMethod;

  public InterpreterRuntimeMethod(GeneratorRuntime runtime, IMethodSymbol symbol, InterpreterFrame frame)
  {
    _runtime = runtime;
    _symbol = symbol;
    _frame = frame;
  }

  public SymbolMethodInfo Definition => _definition ??= _runtime.CreateMethodInfoDelegator(_symbol.ConstructedFrom);

  public Type[] TypeArguments => _typeArguments ??= _symbol.TypeArguments.Map(t => t.TypeKind is TypeKind.TypeParameter
    ? _frame.GetGenericArgument((ITypeParameterSymbol)t)
    : _runtime.CreateTypeDelegator(t));

  public IRuntimeType DeclaringType => _declaringType ??= new InterpreterRuntimeType(_runtime, _symbol.ContainingType, _frame);

  public bool IsStatic => Definition.IsStatic;

  public bool IsOpenGeneric => false;

  public bool IsSource => _symbol.IsSource();

  public MethodInfo UnderlyingSystemMethod => _underlyingSystemMethod ??= DelegatorBinder.ResolveMethod(DeclaringType.UnderlyingSystemType, this);

  public bool IsGenericMethod => Definition.IsGenericMethod;

  public bool IsGenericMethodDefinition => false;
}
