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

  public abstract Type? GetRuntimeType(SymbolType type);

  public abstract ITypeSymbol? GetTypeSymbol(Type type);

  public Type TypeOf<T>()
  {
    Type type = typeof(T);
    ITypeSymbol? symbol = GetTypeSymbol(type);
    return symbol is null
      ? type
      : CreateTypeDelegator(symbol);
  }

  public SymbolAssembly CreateAssemblyDelegator(IAssemblySymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));

    return new SymbolAssembly(this, symbol, null);
  }

  public SymbolConstructorInfo CreateConstructorInfoDelegator(IMethodSymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));
    if (symbol.MethodKind is not MethodKind.Constructor and not MethodKind.StaticConstructor)
      throw new ArgumentException($"Method kind must be {MethodKind.Constructor} or {MethodKind.StaticConstructor}.");

    return new SymbolConstructorInfo(this, symbol);
  }

  public SymbolEventInfo CreateEventInfoDelegator(IEventSymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));

    return new SymbolEventInfo(this, symbol);
  }

  public SymbolFieldInfo CreateFieldInfoDelegator(IFieldSymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));

    return new SymbolFieldInfo(this, symbol);
  }

  public SymbolMethodInfo CreateMethodInfoDelegator(IMethodSymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));
    if (symbol.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor)
      throw new ArgumentException($"Method kind should not be {MethodKind.Constructor} or {MethodKind.StaticConstructor}.", nameof(symbol));

    return new SymbolMethodInfo(this, symbol);
  }

  public SymbolModule CreateModuleDelegator(IModuleSymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));

    return new SymbolModule(this, symbol);
  }

  public SymbolPropertyInfo CreatePropertyInfoDelegator(IPropertySymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));

    return new SymbolPropertyInfo(this, symbol);
  }

  public SymbolType CreateTypeDelegator(ITypeSymbol symbol)
  {
    if (symbol is null)
      throw new ArgumentNullException(nameof(symbol));

    return symbol.Kind switch
    {
      SymbolKind.NamedType     => new SymbolNamedType(this, (INamedTypeSymbol)symbol),
      SymbolKind.ArrayType     => new SymbolArrayType(this, (IArrayTypeSymbol)symbol),
      SymbolKind.TypeParameter => new SymbolTypeParameter(this, (ITypeParameterSymbol)symbol),
      _                        => throw new NotSupportedException($"Symbol of kind {symbol.Kind} is not supported.")
    };
  }

  // For now we don't expose the real types

  Assembly IGeneratorRuntime.CompilationAssembly => CompilationAssembly;

  Assembly IGeneratorRuntime.CreateAssemblyDelegator(IAssemblySymbol symbol)
  {
    return CreateAssemblyDelegator(symbol);
  }

  ConstructorInfo IGeneratorRuntime.CreateConstructorInfoDelegator(IMethodSymbol symbol)
  {
    return CreateConstructorInfoDelegator(symbol);
  }

  EventInfo IGeneratorRuntime.CreateEventInfoDelegator(IEventSymbol symbol)
  {
    return CreateEventInfoDelegator(symbol);
  }

  FieldInfo IGeneratorRuntime.CreateFieldInfoDelegator(IFieldSymbol symbol)
  {
    return CreateFieldInfoDelegator(symbol);
  }

  MethodInfo IGeneratorRuntime.CreateMethodInfoDelegator(IMethodSymbol symbol)
  {
    return CreateMethodInfoDelegator(symbol);
  }

  Module IGeneratorRuntime.CreateModuleDelegator(IModuleSymbol symbol)
  {
    return CreateModuleDelegator(symbol);
  }

  PropertyInfo IGeneratorRuntime.CreatePropertyInfoDelegator(IPropertySymbol symbol)
  {
    return CreatePropertyInfoDelegator(symbol);
  }

  Type IGeneratorRuntime.CreateTypeDelegator(ITypeSymbol symbol)
  {
    return CreateTypeDelegator(symbol);
  }
}