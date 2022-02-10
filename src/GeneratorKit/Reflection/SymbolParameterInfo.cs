using GeneratorKit.Comparers;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal abstract class SymbolParameterInfo : SymbolParameterInfoBase
{
  public SymbolParameterInfo(IGeneratorContext runtime)
  {
    Context = runtime;
  }

  protected IGeneratorContext Context { get; }


  // System.Object overrides

  public sealed override bool Equals(object? obj)
  {
    if (obj is not ParameterInfo parameterInfo)
      return false;

    return ParameterInfoEqualityComparer.Default.Equals(this, parameterInfo);
  }

  public sealed override int GetHashCode()
  {
    return ParameterInfoEqualityComparer.Default.GetHashCode(this);
  }


  // New members

  public new SymbolType[] GetOptionalCustomModifiers() => GetOptionalCustomModifiersCore();

  public new SymbolType[] GetRequiredCustomModifiers() => GetRequiredCustomModifiersCore();
}

internal abstract class SymbolParameterInfoBase : ParameterInfo
{
  // System.Reflection.ParameterInfo overrides

  public sealed override Type[] GetOptionalCustomModifiers() => GetOptionalCustomModifiersCore();

  public sealed override Type[] GetRequiredCustomModifiers() => GetRequiredCustomModifiersCore();


  // Abstract members

  protected abstract SymbolType[] GetOptionalCustomModifiersCore();

  protected abstract SymbolType[] GetRequiredCustomModifiersCore();
}