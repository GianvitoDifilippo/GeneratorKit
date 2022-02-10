using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit;

internal interface IGeneratorContext
{
  Type GetContextType(ITypeSymbol symbol);

  SymbolAssembly CreateAssemblyDelegator(IAssemblySymbol symbol);
  SymbolConstructorInfo CreateConstructorInfoDelegator(IMethodSymbol symbol);
  SymbolEventInfo CreateEventInfoDelegator(IEventSymbol symbol, SymbolType? reflectedType = null);
  SymbolFieldInfo CreateFieldInfoDelegator(IFieldSymbol symbol, SymbolType? reflectedType = null);
  SymbolMethodInfo CreateMethodInfoDelegator(IMethodSymbol symbol, SymbolType? reflectedType = null);
  SymbolModule CreateModuleDelegator(IModuleSymbol symbol);
  SymbolPropertyInfo CreatePropertyInfoDelegator(IPropertySymbol symbol, SymbolType? reflectedType = null);
  SymbolType CreateTypeDelegator(ITypeSymbol symbol);

  SymbolType GetGenericTypeDefinition(SymbolNamedType type);
  SymbolType MakeGenericType(SymbolNamedType type, Type[] typeArguments);
  SymbolMethodInfo GetGenericMethodDefinition(SymbolMethodInfo method, SymbolType? reflectedType);
  SymbolMethodInfo MakeGenericMethod(SymbolMethodInfo method, Type[] typeArguments, SymbolType? reflectedType);
  SymbolType DeclaringType(SymbolMethodInfo method);
}
