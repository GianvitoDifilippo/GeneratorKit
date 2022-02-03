using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratorKit.Interpret;

internal class InterpreterRuntimeType : IRuntimeType
{
  private readonly GeneratorRuntime _runtime;
  private readonly INamedTypeSymbol _symbol;
  private readonly InterpreterFrame _frame;
  private SymbolNamedType? _definition;
  private Type[]? _typeArguments;
  private Type[]? _typeParameters;
  private Type? _runtimeType;

  public InterpreterRuntimeType(GeneratorRuntime runtime, INamedTypeSymbol symbol, InterpreterFrame frame)
  {
    _runtime = runtime;
    _symbol = symbol;
    _frame = frame;
  }

  public IRuntimeType? BaseType => _symbol.BaseType is { } baseType
    ? new InterpreterRuntimeType(_runtime, baseType, _frame)
    : null;

  public IRuntimeType ElementType => throw new InvalidOperationException(); // TODO: Message

  public IRuntimeType DeclaringType => throw new InvalidOperationException(); // TODO: Message

  public IEnumerable<IRuntimeType> Interfaces => _symbol.AllInterfaces.Select(i => new InterpreterRuntimeType(_runtime, i, _frame));

  public SymbolType Definition => _definition ??= _runtime.CreateTypeDelegator(_symbol.ConstructedFrom);

  public Type[] TypeArguments => _typeArguments ??= _symbol.TypeArguments.Map(t => t.TypeKind is TypeKind.TypeParameter
    ? _frame.GetGenericArgument((ITypeParameterSymbol)t)
    : _runtime.CreateTypeDelegator(t));

  public Type[] TypeParameters => _typeParameters ??= _symbol.TypeParameters.Map(_runtime.CreateTypeDelegator);

  public string AssemblyQualifiedName => throw new NotImplementedException();

  public bool HasElementType => false;

  public bool IsConstructedGenericType => _symbol.IsGenericType;

  public bool IsGenericParameter => false;

  public Type RuntimeType => _runtimeType ??= _runtime.GetRuntimeType(this);

  public bool IsArray => false;

  public int ArrayRank => throw new InvalidOperationException(); // TODO: Message

  public bool IsPointer => false;

  public bool IsByRef => false;

  public bool ContainsGenericParameters => false;

  public int GenericParameterPosition => throw new InvalidOperationException(); // TODO: Message

  public string? FullName => throw new NotImplementedException();
}
