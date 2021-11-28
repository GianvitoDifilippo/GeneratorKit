using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal class SymbolReturnParameter : SymbolParameterInfo
{
  public SymbolReturnParameter(GeneratorRuntime runtime, IMethodSymbol symbol)
    : base(runtime)
  {
    Symbol = symbol;
  }

  public IMethodSymbol Symbol { get; }


  // System.Reflection.ParameterInfo overrides

  public override ParameterAttributes Attributes => ParameterAttributes.None;

  public override object? DefaultValue => null;

  public override bool HasDefaultValue => true;

  public override MemberInfo Member => _runtime.CreateMethodInfoDelegator(Symbol);

  public override string? Name => null;

  public override object? RawDefaultValue => null;

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

  protected override SymbolType ParameterTypeCore
  {
    get
    {
      SymbolType type = _runtime.CreateTypeDelegator(Symbol.ReturnType);
      return Symbol.ReturnsByRef
        ? type.MakeByRefType()
        : type;
    }
  }

  protected override SymbolType[] GetOptionalCustomModifiersCore()
  {
    throw new NotSupportedException();
  }

  protected override SymbolType[] GetRequiredCustomModifiersCore()
  {
    throw new NotSupportedException();
  }
}
