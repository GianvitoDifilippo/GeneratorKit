using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolModule : Module
{
  private readonly GeneratorRuntime _runtime;

  public SymbolModule(GeneratorRuntime runtime, IModuleSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IModuleSymbol Symbol { get; }


  public override Assembly Assembly => _runtime.CreateAssemblyDelegator(Symbol.ContainingAssembly);

  public override string Name => Symbol.Name;

  public override string ScopeName => Symbol.Name;

  public override Type? GetType(string className)
  {
    throw new NotImplementedException();
  }

  public override Type? GetType(string className, bool ignoreCase)
  {
    throw new NotImplementedException();
  }

  public override Type? GetType(string className, bool throwOnError, bool ignoreCase)
  {
    throw new NotImplementedException();
  }

  public override Type[] GetTypes()
  {
    throw new NotImplementedException();
  }
}