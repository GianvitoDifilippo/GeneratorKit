using GeneratorKit.Reflection.Context;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal class SymbolReturnParameter : SymbolParameterInfo
{
  private readonly SymbolMethodInfo _method;

  public SymbolReturnParameter(IReflectionContext context, SymbolMethodInfo method)
    : base(context)
  {
    _method = method;
  }


  // System.Reflection.ParameterInfo overrides

  public override ParameterAttributes Attributes => ParameterAttributes.None;

  public override object? DefaultValue => null;

  public override bool HasDefaultValue => true;

  public override MemberInfo Member => _method;

  public override string? Name => null;

  public override object? RawDefaultValue => null;

  public override Type ParameterType
  {
    get
    {
      Type type = _method.ReturnType;
      return _method.Symbol.ReturnsByRef
        ? type.MakeByRefType()
        : type;
    }
  }

  public override int Position => -1;

  public override IList<CustomAttributeData> GetCustomAttributesData()
  {
    List<CustomAttributeData> result = _method.Symbol
      .GetReturnTypeAttributes()
      .Select(data => CompilationCustomAttributeData.FromAttributeData(Context, data) as CustomAttributeData)
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

  protected override SymbolType[] GetOptionalCustomModifiersCore()
  {
    throw new NotSupportedException();
  }

  protected override SymbolType[] GetRequiredCustomModifiersCore()
  {
    throw new NotSupportedException();
  }
}
