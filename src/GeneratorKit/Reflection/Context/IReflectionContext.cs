using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Reflection.Context;

internal interface IReflectionContext
{
  Compilation Compilation { get; }

  Type GetRuntimeType(SymbolType type);
  object InvokeConstructor(SymbolConstructorInfo constructor, object?[] arguments);
  object? InvokeMethod(SymbolMethodInfo method, object? instance, object?[] arguments);
  object? GetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments);
  void SetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments, object? value);
  object? GetField(SymbolFieldInfo field, object? instance);
  void SetField(SymbolFieldInfo field, object? instance, object? value);

  Type GetContextType(ITypeSymbol symbol);
  SymbolNamedType GetGenericTypeDefinition(SymbolNamedType type);
  SymbolNamedType MakeGenericType(SymbolNamedType type, Type[] typeArguments);
  SymbolMethodInfo GetGenericMethodDefinition(SymbolMethodInfo method);
  SymbolMethodInfo MakeGenericMethod(SymbolMethodInfo method, Type[] typeArguments, SymbolType? reflectedType);
  SymbolNamedType GetDeclaringType(SymbolMethodInfo method);
  SymbolMethodInfo GetBaseDefinition(SymbolMethodInfo method, SymbolType? reflectedType);
  bool IsGenericTypeDefinition(INamedTypeSymbol symbol);
  bool ContainsGenericParameters(INamedTypeSymbol symbol);
  bool ContainsGenericParameters(IArrayTypeSymbol symbol);
  bool IsGenericMethodDefinition(IMethodSymbol symbol);
  bool ContainsGenericParameters(IMethodSymbol symbol);

  SymbolAssembly CreateAssemblyDelegator(IAssemblySymbol symbol);
  SymbolConstructorInfo CreateConstructorInfoDelegator(IMethodSymbol symbol);
  SymbolFieldInfo CreateFieldInfoDelegator(IFieldSymbol symbol, SymbolType? reflectedType = null);
  SymbolMethodInfo CreateMethodInfoDelegator(IMethodSymbol symbol, SymbolType? reflectedType = null);
  SymbolModule CreateModuleDelegator(IModuleSymbol symbol);
  SymbolPropertyInfo CreatePropertyInfoDelegator(IPropertySymbol symbol, SymbolType? reflectedType = null);
  SymbolType CreateTypeDelegator(ITypeSymbol symbol);
  SymbolNamedType CreateTypeDelegator(INamedTypeSymbol symbol);
  SymbolArrayType CreateTypeDelegator(IArrayTypeSymbol symbol);
  SymbolTypeParameter CreateTypeDelegator(ITypeParameterSymbol symbol);
}
