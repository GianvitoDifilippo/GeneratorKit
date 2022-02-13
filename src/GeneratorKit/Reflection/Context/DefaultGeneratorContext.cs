using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Reflection.Context;

internal class DefaultGeneratorContext : GeneratorContext
{
  // TODO: Cache

  public DefaultGeneratorContext(IReflectionRuntime runtime)
    : base(runtime)
  {
  }

  public override Type GetContextType(ITypeParameterSymbol symbol)
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

  public override SymbolMethodInfo GetGenericMethodDefinition(SymbolMethodInfo method)
  {
    return CreateMethodInfoDelegator(method.OriginalSymbol.ConstructedFrom, null);
  }

  public override SymbolMethodInfo MakeGenericMethod(SymbolMethodInfo method, Type[] typeArguments, SymbolType? reflectedType)
  {
    GenericMethodContext context = new GenericMethodContext(Runtime, this, typeArguments);
    return context.CreateMethodInfoDelegator(method.OriginalSymbol, reflectedType);
  }

  public override SymbolType GetDeclaringType(SymbolMethodInfo method)
  {
    return CreateTypeDelegator(method.OriginalSymbol.ContainingType);
  }

  public override SymbolMethodInfo GetBaseDefinition(SymbolMethodInfo method, SymbolType? reflectedType)
  {
    IMethodSymbol symbol = method.OriginalSymbol;
    if (symbol.IsOverride)
    {
      IMethodSymbol overriddenMethod = method.IsGenericMethod && !method.IsGenericMethodDefinition
        ? symbol.ConstructedFrom.OverriddenMethod!
        : symbol.OverriddenMethod!;
      INamedTypeSymbol containingType = overriddenMethod.ContainingType;
      GeneratorContext context = containingType.IsGenericType
        ? new GenericTypeContext(Runtime, this, containingType.TypeArguments.Map(GetContextType))
        : this;

      return context.CreateMethodInfoDelegator(overriddenMethod);
    }
    else
    {
      return symbol.IsVirtual && reflectedType is not null
        ? CreateMethodInfoDelegator(symbol)
        : method;
    }
  }

  public override bool IsGenericTypeDefinition(INamedTypeSymbol symbol)
  {
    if (!symbol.IsGenericType)
      return false;

    foreach (ITypeSymbol typeArgument in symbol.TypeArguments)
    {
      if (typeArgument.Kind is not SymbolKind.TypeParameter)
        return false;
    }

    return true;
  }

  public override bool ContainsGenericParameters(INamedTypeSymbol symbol)
  {
    if (!symbol.IsGenericType)
      return false;

    foreach (ITypeSymbol typeArgument in symbol.TypeArguments)
    {
      if (typeArgument.Kind is SymbolKind.TypeParameter)
        return true;
    }

    return false;
  }

  public override bool IsGenericMethodDefinition(IMethodSymbol symbol)
  {
    if (!symbol.IsGenericMethod)
      return false;

    foreach (ITypeSymbol typeArgument in symbol.TypeArguments)
    {
      if (typeArgument.Kind is not SymbolKind.TypeParameter)
        return false;
    }

    return true;
  }

  public override bool ContainsGenericParameters(IMethodSymbol symbol)
  {
    if (symbol.ContainingType is not null && ContainsGenericParameters(symbol.ContainingType))
      return true;
    if (!symbol.IsGenericMethod)
      return false;

    foreach (ITypeSymbol typeArgument in symbol.TypeArguments)
    {
      if (typeArgument.Kind is SymbolKind.TypeParameter)
        return true;
    }

    return false;
  }
}
