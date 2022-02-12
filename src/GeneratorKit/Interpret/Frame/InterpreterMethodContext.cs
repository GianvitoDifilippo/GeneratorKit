using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Interpret.Frame;

internal class InterpreterMethodContext : GeneratorContext
{
  private readonly GeneratorContext _parent;
  private readonly Type[] _typeArguments;

  public InterpreterMethodContext(IReflectionRuntime runtime, GeneratorContext parent, Type[] typeArguments)
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
    return symbol.TypeParameterKind is TypeParameterKind.Method
      ? _typeArguments[symbol.Ordinal]
      : _parent.GetContextType(symbol);
  }

  public override SymbolType GetDeclaringType(SymbolMethodInfo method)
  {
    return CreateTypeDelegator(method.OriginalSymbol.ContainingType);
  }

  public override SymbolMethodInfo GetGenericMethodDefinition(SymbolMethodInfo method)
  {
    throw new NotSupportedException();
  }

  public override SymbolType GetGenericTypeDefinition(SymbolNamedType type)
  {
    return _parent.GetGenericTypeDefinition(type);
  }

  public override SymbolMethodInfo MakeGenericMethod(SymbolMethodInfo method, Type[] typeArguments, SymbolType? reflectedType)
  {
    throw new NotSupportedException();
  }

  public override SymbolType MakeGenericType(SymbolNamedType type, Type[] typeArguments)
  {
    throw new NotSupportedException();
  }

  public override SymbolMethodInfo GetBaseDefinition(SymbolMethodInfo method, SymbolType? reflectedType)
  {
    throw new NotSupportedException();
  }
}
