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

  public abstract Type GetRuntimeType(IRuntimeType type);
  public abstract object InvokeConstructor(IRuntimeConstructor constructor, object?[] arguments);
  public abstract object? InvokeMethod(IRuntimeMethod method, object? instance, object?[] arguments);
  public abstract object? InvokeGetter(IRuntimeProperty property, object? instance, object?[] arguments);
  public abstract void InvokeSetter(IRuntimeProperty property, object? instance, object?[] arguments, object? value);

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
      SymbolKind.NamedType => CreateTypeDelegator((INamedTypeSymbol)symbol),
      SymbolKind.ArrayType => CreateTypeDelegator((IArrayTypeSymbol)symbol),
      SymbolKind.TypeParameter => CreateTypeDelegator((ITypeParameterSymbol)symbol),
      _ => throw new NotSupportedException($"Symbol of kind {symbol.Kind} is not supported.")
    };
  }

  public SymbolNamedType CreateTypeDelegator(INamedTypeSymbol symbol)
  {
    return new SymbolNamedType(this, symbol);
  }

  public SymbolArrayType CreateTypeDelegator(IArrayTypeSymbol symbol)
  {
    return new SymbolArrayType(this, symbol);
  }

  public SymbolTypeParameter CreateTypeDelegator(ITypeParameterSymbol symbol)
  {
    return new SymbolTypeParameter(this, symbol);
  }

  Assembly IGeneratorRuntime.CompilationAssembly => CompilationAssembly;

  Assembly IGeneratorRuntime.CreateAssemblyDelegator(IAssemblySymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));

    return CreateAssemblyDelegator(symbol);
  }

  ConstructorInfo IGeneratorRuntime.CreateConstructorInfoDelegator(IMethodSymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));

    return CreateConstructorInfoDelegator(symbol);
  }

  EventInfo IGeneratorRuntime.CreateEventInfoDelegator(IEventSymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));

    return CreateEventInfoDelegator(symbol);
  }

  FieldInfo IGeneratorRuntime.CreateFieldInfoDelegator(IFieldSymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));

    return CreateFieldInfoDelegator(symbol);
  }

  MethodInfo IGeneratorRuntime.CreateMethodInfoDelegator(IMethodSymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));

    return CreateMethodInfoDelegator(symbol);
  }

  Module IGeneratorRuntime.CreateModuleDelegator(IModuleSymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));

    return CreateModuleDelegator(symbol);
  }

  PropertyInfo IGeneratorRuntime.CreatePropertyInfoDelegator(IPropertySymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));

    return CreatePropertyInfoDelegator(symbol);
  }

  Type IGeneratorRuntime.CreateTypeDelegator(ITypeSymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));

    return CreateTypeDelegator(symbol);
  }
}