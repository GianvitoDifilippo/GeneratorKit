﻿using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Reflection.Context;

internal class GenericTypeContext : GeneratorContext
{
  private readonly DefaultGeneratorContext _parent;
  private readonly Type[] _typeArguments;

  public GenericTypeContext(IReflectionRuntime runtime, DefaultGeneratorContext parent, Type[] typeArguments)
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

  public override SymbolMethodInfo GetGenericMethodDefinition(SymbolMethodInfo method)
  {
    return CreateMethodInfoDelegator(method.OriginalSymbol.ConstructedFrom, null);
  }

  public override SymbolMethodInfo MakeGenericMethod(SymbolMethodInfo method, Type[] typeArguments, SymbolType? reflectedType)
  {
    GenericMethodContext context = new GenericMethodContext(Runtime, this, typeArguments);
    return context.CreateMethodInfoDelegator(method.OriginalSymbol.ConstructedFrom, reflectedType);
  }

  public override SymbolType GetDeclaringType(SymbolMethodInfo method)
  {
    return CreateTypeDelegator(method.OriginalSymbol.ContainingType);
  }

  public override bool IsGenericMethodDefinition(IMethodSymbol symbol)
  {
    return _parent.IsGenericMethodDefinition(symbol);
  }

  public override bool ContainsGenericParameters(IMethodSymbol symbol)
  {
    return _parent.ContainsGenericParameters(symbol);
  }
}