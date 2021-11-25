using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolParameterInfo : SymbolParameterInfoBase
{
  private readonly GeneratorRuntime _runtime;

  public SymbolParameterInfo(GeneratorRuntime runtime, IParameterSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IParameterSymbol Symbol { get; }



  // SymbolParameterInfoBase overrides

  protected override SymbolType ParameterTypeCore => _runtime.CreateTypeDelegator(Symbol.Type);

  protected override SymbolType[] GetOptionalCustomModifiersCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolType[] GetRequiredCustomModifiersCore()
  {
    throw new NotImplementedException();
  }
}

internal abstract class SymbolParameterInfoBase : ParameterInfo
{
  private protected SymbolParameterInfoBase() { }


  // System.Reflection.PropertyInfo overrides

  public sealed override Type ParameterType => ParameterTypeCore;

  public sealed override Type[] GetOptionalCustomModifiers() => GetOptionalCustomModifiersCore();

  public sealed override Type[] GetRequiredCustomModifiers() => GetRequiredCustomModifiersCore();


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ParameterTypeCore { get; }

  protected abstract SymbolType[] GetOptionalCustomModifiersCore();

  protected abstract SymbolType[] GetRequiredCustomModifiersCore();
}