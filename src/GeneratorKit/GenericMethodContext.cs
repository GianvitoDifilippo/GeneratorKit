using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit;

internal class GenericMethodContext : GeneratorContext
{
  private readonly GeneratorContext _parent;
  private readonly Type[] _typeArguments;

  public GenericMethodContext(IRuntime runtime, GeneratorContext parent, Type[] typeArguments)
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

  private Type GetContextType(ITypeParameterSymbol symbol)
  {
    return symbol.TypeParameterKind is TypeParameterKind.Method
      ? _typeArguments[symbol.Ordinal]
      : _parent.GetContextType(symbol);
  }

  public override SymbolType GetGenericTypeDefinition(SymbolNamedType type)
  {
    throw new InvalidOperationException();
  }

  public override SymbolType MakeGenericType(SymbolNamedType type, Type[] typeArguments)
  {
    throw new InvalidOperationException();
  }

  public override SymbolMethodInfo GetGenericMethodDefinition(SymbolMethodInfo method, SymbolType? reflectedType)
  {
    return _parent.CreateMethodInfoDelegator(method.OriginalSymbol.ConstructedFrom, reflectedType);
  }

  public override SymbolMethodInfo MakeGenericMethod(SymbolMethodInfo method, Type[] typeArguments, SymbolType? reflectedType)
  {
    GenericMethodContext context = new GenericMethodContext(Runtime, _parent, typeArguments);
    return context.CreateMethodInfoDelegator(method.OriginalSymbol.ConstructedFrom, reflectedType);
  }

  public override SymbolType DeclaringType(SymbolMethodInfo method)
  {
    return _parent.CreateTypeDelegator(method.OriginalSymbol.ContainingType);
  }
}
