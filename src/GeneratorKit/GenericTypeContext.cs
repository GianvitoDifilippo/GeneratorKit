using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit;

internal class GenericTypeContext : GeneratorContext
{
  private readonly DefaultGeneratorContext _parent;
  private readonly Type[] _typeArguments;

  public GenericTypeContext(IRuntime runtime, DefaultGeneratorContext parent, Type[] typeArguments)
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

  private Type GetContextType(ITypeParameterSymbol symbol) // TODO: Make abstract on superclass and call it directly from GenericMethodContext
  {
    return symbol.TypeParameterKind is TypeParameterKind.Type
      ? _typeArguments[symbol.Ordinal]
      : CreateTypeDelegator(symbol);
  }

  public override SymbolType GetGenericTypeDefinition(SymbolNamedType type)
  {
    return _parent.CreateTypeDelegator(type.OriginalSymbol.ConstructedFrom);
  }

  public override SymbolType MakeGenericType(SymbolNamedType type, Type[] typeArguments)
  {
    GenericTypeContext context = new GenericTypeContext(Runtime, _parent, typeArguments);
    return context.CreateTypeDelegator(type.OriginalSymbol.ConstructedFrom);
  }

  public override SymbolMethodInfo GetGenericMethodDefinition(SymbolMethodInfo method, SymbolType? reflectedType)
  {
    return CreateMethodInfoDelegator(method.OriginalSymbol.ConstructedFrom, reflectedType);
  }

  public override SymbolMethodInfo MakeGenericMethod(SymbolMethodInfo method, Type[] typeArguments, SymbolType? reflectedType)
  {
    GenericMethodContext context = new GenericMethodContext(Runtime, this, typeArguments);
    return context.CreateMethodInfoDelegator(method.OriginalSymbol.ConstructedFrom, reflectedType);
  }

  public override SymbolType DeclaringType(SymbolMethodInfo method)
  {
    return CreateTypeDelegator(method.OriginalSymbol.ContainingType);
  }
}
