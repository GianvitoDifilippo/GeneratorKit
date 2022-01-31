using GeneratorKit.Interpret;
using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Interpret;

internal class InterpreterRuntimeType : IRuntimeType
{
  private readonly GeneratorRuntime _runtime;
  private readonly INamedTypeSymbol _symbol;
  private readonly InterpreterFrame _frame;

  public InterpreterRuntimeType(GeneratorRuntime runtime, INamedTypeSymbol symbol, InterpreterFrame frame)
  {
    _runtime = runtime;
    _symbol = symbol;
    _frame = frame;
  }

  public SymbolNamedType Definition => (SymbolNamedType)_runtime.CreateTypeDelegator(_symbol.OriginalDefinition);

  public Type[] TypeArguments => _symbol.TypeArguments.Map(t => t.TypeKind is TypeKind.TypeParameter
    ? _frame.GetGenericArgument((ITypeParameterSymbol)t)
    : _runtime.CreateTypeDelegator(t));

  public IRuntimeType? BaseType => Definition.Symbol.BaseType is { } baseType && baseType.ContainingAssembly is ISourceAssemblySymbol
    ? new InterpreterRuntimeType(_runtime, baseType, _frame)
    : null;

  public bool IsSource => true;

  public bool ContainsGenericParameters => false;

  public bool IsInterface => Definition.IsInterface;

  public bool IsAbstract => Definition.IsAbstract;

  public string? FullName => throw new NotImplementedException();

  public Type UnderlyingSystemType => _symbol.IsGenericType
    ? Definition.MakeGenericType(TypeArguments).UnderlyingSystemType
    : Definition.UnderlyingSystemType;
}
