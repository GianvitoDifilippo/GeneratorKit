using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit;

internal class DefaultGeneratorContext : GeneratorContext
{
  // TODO: Cache

  public DefaultGeneratorContext(IRuntime runtime)
    : base(runtime)
  {
  }

  public override Type GetContextType(ITypeSymbol symbol)
  {
    return CreateTypeDelegator(symbol);
  }

  public override SymbolType GetGenericTypeDefinition(SymbolNamedType type)
  {
    return CreateTypeDelegator(type.Symbol.ConstructedFrom);
  }

  public override SymbolType MakeGenericType(SymbolNamedType type, Type[] typeArguments)
  {
    GenericTypeContext context = new GenericTypeContext(Runtime, this, typeArguments);
    return context.CreateTypeDelegator(type.Symbol);
  }

  public override SymbolMethodInfo GetGenericMethodDefinition(SymbolMethodInfo method, SymbolType? reflectedType)
  {
    return CreateMethodInfoDelegator(method.OriginalSymbol.ConstructedFrom, reflectedType);
  }

  public override SymbolMethodInfo MakeGenericMethod(SymbolMethodInfo method, Type[] typeArguments, SymbolType? reflectedType)
  {
    GenericMethodContext context = new GenericMethodContext(Runtime, this, typeArguments);
    return context.CreateMethodInfoDelegator(method.OriginalSymbol, reflectedType);
  }

  public override SymbolType DeclaringType(SymbolMethodInfo method)
  {
    return CreateTypeDelegator(method.OriginalSymbol.ContainingType);
  }
}
