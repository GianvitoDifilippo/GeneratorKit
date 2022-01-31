using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Threading;

namespace GeneratorKit;

internal abstract class GeneratorRuntime : IGeneratorRuntime
{
  public GeneratorRuntime(Compilation compilation)
  {
    Compilation = compilation;
  }

  public Compilation Compilation { get; }

  public abstract CancellationToken CancellationToken { get; }

  public abstract SymbolAssembly CompilationAssembly { get; }

  public abstract T CreateInstance<T>(Type type, params object?[] arguments);

  public abstract Type GetRuntimeType(SymbolType type);

  public abstract Type GetRuntimeType(HybridGenericType type);

  public abstract object? InvokeMethod(IRuntimeMethod method, object? instance, object?[] arguments);

  public SymbolAssembly CreateAssemblyDelegator(IAssemblySymbol symbol)
  {
    return new SymbolAssembly(this, symbol, null);
  }

  public SymbolConstructorInfo CreateConstructorInfoDelegator(IMethodSymbol symbol)
  {
    if (symbol.MethodKind is not MethodKind.Constructor and not MethodKind.StaticConstructor)
      throw new ArgumentException($"Method kind must be {MethodKind.Constructor} or {MethodKind.StaticConstructor}.");

    return new SymbolConstructorInfo(this, symbol);
  }

  public SymbolEventInfo CreateEventInfoDelegator(IEventSymbol symbol)
  {
    return new SymbolEventInfo(this, symbol);
  }

  public SymbolFieldInfo CreateFieldInfoDelegator(IFieldSymbol symbol)
  {
    return new SymbolFieldInfo(this, symbol);
  }

  public SymbolMethodInfo CreateMethodInfoDelegator(IMethodSymbol symbol)
  {
    if (symbol.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor)
      throw new ArgumentException($"Method kind should not be {MethodKind.Constructor} or {MethodKind.StaticConstructor}.", nameof(symbol));

    return new SymbolMethodInfo(this, symbol);
  }

  public SymbolModule CreateModuleDelegator(IModuleSymbol symbol)
  {
    return new SymbolModule(this, symbol);
  }

  public SymbolPropertyInfo CreatePropertyInfoDelegator(IPropertySymbol symbol)
  {
    return new SymbolPropertyInfo(this, symbol);
  }

  public SymbolType CreateTypeDelegator(ITypeSymbol symbol)
  {
    return symbol.Kind switch
    {
      SymbolKind.NamedType => new SymbolNamedType(this, (INamedTypeSymbol)symbol),
      SymbolKind.ArrayType => new SymbolArrayType(this, (IArrayTypeSymbol)symbol),
      SymbolKind.TypeParameter => new SymbolTypeParameter(this, (ITypeParameterSymbol)symbol),
      _ => throw new NotSupportedException($"Symbol of kind {symbol.Kind} is not supported.")
    };
  }

  Assembly IGeneratorRuntime.CompilationAssembly => CompilationAssembly;

  Assembly IGeneratorRuntime.CreateAssemblyDelegator(IAssemblySymbol symbol) => CreateAssemblyDelegator(symbol);

  ConstructorInfo IGeneratorRuntime.CreateConstructorInfoDelegator(IMethodSymbol symbol) => CreateConstructorInfoDelegator(symbol);

  EventInfo IGeneratorRuntime.CreateEventInfoDelegator(IEventSymbol symbol) => CreateEventInfoDelegator(symbol);

  FieldInfo IGeneratorRuntime.CreateFieldInfoDelegator(IFieldSymbol symbol) => CreateFieldInfoDelegator(symbol);

  MethodInfo IGeneratorRuntime.CreateMethodInfoDelegator(IMethodSymbol symbol) => CreateMethodInfoDelegator(symbol);

  Module IGeneratorRuntime.CreateModuleDelegator(IModuleSymbol symbol) => CreateModuleDelegator(symbol);

  PropertyInfo IGeneratorRuntime.CreatePropertyInfoDelegator(IPropertySymbol symbol) => CreatePropertyInfoDelegator(symbol);

  Type IGeneratorRuntime.CreateTypeDelegator(ITypeSymbol symbol) => CreateTypeDelegator(symbol);
}