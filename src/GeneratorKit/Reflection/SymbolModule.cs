using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

public sealed class SymbolModule : SymbolModuleBase
{
  private readonly IGeneratorRuntime _runtime;

  internal SymbolModule(IGeneratorRuntime runtime, IModuleSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IModuleSymbol Symbol { get; }


  // System.Reflection.Module overrides


  // SymbolModuleBase overrides

  protected override SymbolAssembly AssemblyCore => throw new NotImplementedException();

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

#region Base

public abstract class SymbolModuleBase : Module
{
  private protected SymbolModuleBase() { }


  // System.Reflection.Module overrides

  public sealed override Assembly Assembly => AssemblyCore;

  public sealed override Type[] GetTypes() => GetTypesCore();

  public sealed override Type? GetType(string className) => GetTypeCore(className);

  public sealed override Type? GetType(string className, bool ignoreCase) => GetTypeCore(className, ignoreCase);

  public sealed override Type? GetType(string className, bool throwOnError, bool ignoreCase) => GetTypeCore(className, throwOnError, ignoreCase);

  // Abstract members

  protected abstract SymbolAssembly AssemblyCore { get; }

  protected abstract SymbolType[] GetTypesCore();

  protected abstract SymbolType? GetTypeCore(string className);

  protected abstract SymbolType? GetTypeCore(string className, bool ignoreCase);

  protected abstract SymbolType? GetTypeCore(string className, bool throwOnError, bool ignoreCase);
}

#endregion