using System.Diagnostics;
using System;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GeneratorKit.Reflection;

internal class SymbolReturnParameterInfo : SymbolReturnParameterInfoBase
{
  private readonly GeneratorRuntime _runtime;

  public SymbolReturnParameterInfo(GeneratorRuntime runtime, IMethodSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IMethodSymbol Symbol { get; }


  // System.Reflection.ParameterInfo overrides

  public override ParameterAttributes Attributes => ParameterAttributes.Retval;

  public override object? DefaultValue => DBNull.Value;

  public override bool HasDefaultValue => false;

  public override MemberInfo Member => _runtime.CreateMethodInfoDelegator(Symbol);

  public override string? Name => null;

  public override object RawDefaultValue => DBNull.Value;

  public override int Position => -1;

  public override IList<CustomAttributeData> GetCustomAttributesData()
  {
    List<CustomAttributeData> result = Symbol
      .GetReturnTypeAttributes()
      .Select(x => (CustomAttributeData)CompilationCustomAttributeData.FromAttributeData(_runtime, x))
      .ToList();
    return new ReadOnlyCollection<CustomAttributeData>(result);
  }

  public override object[] GetCustomAttributes(bool inherit)
  {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  // SymbolReturnParameterInfoBase overrides

  protected override SymbolType ParameterTypeCore => _runtime.CreateTypeDelegator(Symbol.ReturnType);

  protected override SymbolType[] GetOptionalCustomModifiersCore()
  {
    throw new NotSupportedException();
  }

  protected override SymbolType[] GetRequiredCustomModifiersCore()
  {
    throw new NotSupportedException();
  }
}

internal abstract class SymbolReturnParameterInfoBase : ParameterInfo
{
  private protected SymbolReturnParameterInfoBase() { }


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
