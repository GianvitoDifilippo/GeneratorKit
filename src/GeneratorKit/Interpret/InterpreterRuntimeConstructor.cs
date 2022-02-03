using GeneratorKit.Reflection;
using GeneratorKit.Reflection.Binders;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Interpret;

internal class InterpreterRuntimeConstructor : IRuntimeConstructor
{
  private readonly GeneratorRuntime _runtime;
  private readonly InterpreterFrame _frame;
  private IRuntimeType? _declaringType;
  private Type[]? _parameterTypes;
  private ConstructorInfo? _underlyingSystemConstructor;

  public InterpreterRuntimeConstructor(GeneratorRuntime runtime, IMethodSymbol symbol, InterpreterFrame frame)
  {
    _runtime = runtime;
    Symbol = symbol;
    _frame = frame;
  }

  public IMethodSymbol Symbol { get; }

  public IRuntimeType DeclaringType => _declaringType ??= new InterpreterRuntimeType(_runtime, Symbol.ContainingType, _frame);

  public ConstructorInfo UnderlyingSystemConstructor => _underlyingSystemConstructor ??= DelegatorBinder.ResolveConstructor(DeclaringType.RuntimeType.UnderlyingSystemType, this);

  public CallingConventions CallingConvention => Symbol.IsStatic
    ? CallingConventions.Standard
    : CallingConventions.Standard | CallingConventions.HasThis;

  public Type[] ParameterTypes => _parameterTypes ??= Symbol.Parameters.Map(p => p.Type.TypeKind is TypeKind.TypeParameter
    ? _frame.GetGenericArgument((ITypeParameterSymbol)p.Type)
    : _runtime.CreateTypeDelegator(p.Type));
}
