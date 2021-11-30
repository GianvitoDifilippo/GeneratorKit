using System;
using System.Diagnostics;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal abstract class SymbolParameterInfo : SymbolParameterInfoBase
{
  protected readonly GeneratorRuntime _runtime;

  public SymbolParameterInfo(GeneratorRuntime runtime)
  {
    _runtime = runtime;
  }
}

internal abstract class SymbolParameterInfoBase : ParameterInfo
{
  private protected SymbolParameterInfoBase() { }


  // System.Reflection.ParameterInfo overrides

  public sealed override Type ParameterType => ParameterTypeCore;

  public sealed override Type[] GetOptionalCustomModifiers() => GetOptionalCustomModifiersCore();

  public sealed override Type[] GetRequiredCustomModifiers() => GetRequiredCustomModifiersCore();


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ParameterTypeCore { get; }

  protected abstract SymbolType[] GetOptionalCustomModifiersCore();

  protected abstract SymbolType[] GetRequiredCustomModifiersCore();
}