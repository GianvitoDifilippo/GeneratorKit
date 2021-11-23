using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolAssembly : Assembly
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

  public override MethodInfo? EntryPoint => throw new NotImplementedException();

  public override string FullName => Symbol.Identity.GetDisplayName();

  public override Type[] GetExportedTypes()
  {
    throw new NotImplementedException();
  }

  public override Type? GetType(string name)
  {
    throw new NotImplementedException();
  }

  public override Type? GetType(string name, bool throwOnError)
  {
    throw new NotImplementedException();
  }

  public override Type? GetType(string name, bool throwOnError, bool ignoreCase)
  {
    throw new NotImplementedException();
  }

  public override Type[] GetTypes()
  {
    throw new NotImplementedException();
  }
}