using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolAssembly : SymbolAssemblyBase
{
  private readonly GeneratorRuntime _runtime;
  private readonly IMethodSymbol? _entryPoint;

  public SymbolAssembly(GeneratorRuntime runtime, IAssemblySymbol symbol, IMethodSymbol? entryPoint)
  {
    _runtime = runtime;
    Symbol = symbol;
    _entryPoint = entryPoint;
  }

  public IAssemblySymbol Symbol { get; }

  protected override SymbolMethodInfo? EntryPointCore => throw new NotImplementedException();

  public override string FullName => Symbol.Identity.GetDisplayName();

  protected override SymbolType[] GetExportedTypesCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolType? GetTypeCore(string name)
  {
    throw new NotImplementedException();
  }

  protected override SymbolType? GetTypeCore(string name, bool throwOnError)
  {
    throw new NotImplementedException();
  }

  protected override SymbolType? GetTypeCore(string name, bool throwOnError, bool ignoreCase)
  {
    throw new NotImplementedException();
  }

  protected override SymbolType[] GetTypesCore()
  {
    throw new NotImplementedException();
  }
}

internal abstract class SymbolAssemblyBase : Assembly
{
  private protected SymbolAssemblyBase() { }


  // System.Reflection.Assembly overrides

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public sealed override MethodInfo? EntryPoint => EntryPointCore;

  public sealed override Type[] GetExportedTypes() => GetExportedTypesCore();

  public sealed override Type[] GetTypes() => GetTypesCore();

  public sealed override Type? GetType(string name) => GetTypeCore(name);

  public sealed override Type? GetType(string name, bool throwOnError) => GetTypeCore(name, throwOnError);

  public sealed override Type? GetType(string name, bool throwOnError, bool ignoreCase) => GetTypeCore(name, throwOnError, ignoreCase);


  // Abstract members

  protected abstract SymbolMethodInfo? EntryPointCore { get; }

  protected abstract SymbolType[] GetExportedTypesCore();

  protected abstract SymbolType[] GetTypesCore();

  protected abstract SymbolType? GetTypeCore(string name);

  protected abstract SymbolType? GetTypeCore(string name, bool throwOnError);

  protected abstract SymbolType? GetTypeCore(string name, bool throwOnError, bool ignoreCase);
}