using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

public sealed class SymbolParameterInfo : SymbolParameterInfoBase
{
  private readonly IGeneratorRuntime _runtime;

  internal SymbolParameterInfo(IGeneratorRuntime runtime, IParameterSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IParameterSymbol Symbol { get; }


  // System.Reflection.ParameterInfo overrides


  // SymbolParameterInfoBase overrides

  protected override SymbolType ParameterTypeCore => throw new NotImplementedException();

  protected override SymbolType[] GetOptionalCustomModifiersCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolType[] GetRequiredCustomModifiersCore()
  {
    throw new NotImplementedException();
  }
}

#region Base

public abstract class SymbolParameterInfoBase : ParameterInfo
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

#endregion