using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolModule : SymbolModuleBase
{
  private readonly GeneratorRuntime _runtime;

  public SymbolModule(GeneratorRuntime runtime, IModuleSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IModuleSymbol Symbol { get; }


  // System.Reflection.Module overrides

  public override string Name => Symbol.Name;

  public override string ScopeName => Symbol.Name;


  // SymbolModuleBase overrides

  protected override SymbolAssembly AssemblyCore => _runtime.CreateAssemblyDelegator(Symbol.ContainingAssembly);

  protected override SymbolType? GetTypeCore(string className)
  {
    throw new NotImplementedException();
  }

  protected override SymbolType? GetTypeCore(string className, bool ignoreCase)
  {
    throw new NotImplementedException();
  }

  protected override SymbolType? GetTypeCore(string className, bool throwOnError, bool ignoreCase)
  {
    throw new NotImplementedException();
  }

  protected override SymbolType[] GetTypesCore()
  {
    throw new NotImplementedException();
  }
}

internal abstract class SymbolModuleBase : Module
{
  private protected SymbolModuleBase() { }


  // System.Reflection.Module overrides

  public sealed override Assembly Assembly => AssemblyCore;

  public sealed override Type? GetType(string className) => GetTypeCore(className);

  public sealed override Type? GetType(string className, bool ignoreCase) => GetTypeCore(className, ignoreCase);

  public sealed override Type? GetType(string className, bool throwOnError, bool ignoreCase) => GetTypeCore(className, throwOnError, ignoreCase);

  public sealed override Type[] GetTypes() => GetTypesCore();

  // Abstract members

  protected abstract SymbolAssembly AssemblyCore { get; }

  protected abstract SymbolType? GetTypeCore(string className);

  protected abstract SymbolType? GetTypeCore(string className, bool ignoreCase);

  protected abstract SymbolType? GetTypeCore(string className, bool throwOnError, bool ignoreCase);

  protected abstract SymbolType[] GetTypesCore();
}