using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Interpret.Frame;

internal class InterpreterTypeContext : GeneratorContext
{
  private readonly GeneratorContext _parent;
  private readonly Type[] _typeArguments;

  public InterpreterTypeContext(IReflectionRuntime runtime, GeneratorContext parent, Type[] typeArguments)
    : base(runtime)
  {
    _parent = parent;
    _typeArguments = typeArguments;
  }

  public override Type GetContextType(ITypeSymbol symbol)
  {
    return symbol.Kind is SymbolKind.TypeParameter
      ? GetContextType((ITypeParameterSymbol)symbol)
      : CreateTypeDelegator(symbol);
  }

  public override Type GetContextType(ITypeParameterSymbol symbol)
  {
    return symbol.TypeParameterKind is TypeParameterKind.Type
      ? _typeArguments[symbol.Ordinal]
      : throw new ArgumentException("TypeParameterKind must be Type.", nameof(symbol));
  }

  public override SymbolType GetDeclaringType(SymbolMethodInfo method)
  {
    throw new NotSupportedException();
  }

  public override SymbolMethodInfo GetGenericMethodDefinition(SymbolMethodInfo method)
  {
    throw new NotSupportedException();
  }

  public override SymbolType GetGenericTypeDefinition(SymbolNamedType type)
  {
    return _parent.CreateTypeDelegator(type.Symbol);
  }

  public override SymbolMethodInfo MakeGenericMethod(SymbolMethodInfo method, Type[] typeArguments, SymbolType? reflectedType)
  {
    throw new NotSupportedException();
  }

  public override SymbolType MakeGenericType(SymbolNamedType type, Type[] typeArguments)
  {
    throw new NotSupportedException();
  }
}
