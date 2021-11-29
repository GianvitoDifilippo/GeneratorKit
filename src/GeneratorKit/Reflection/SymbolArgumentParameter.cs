using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolArgumentParameter : SymbolParameterInfo
{
  public SymbolArgumentParameter(GeneratorRuntime runtime, IParameterSymbol symbol)
    : base(runtime)
  {
    Symbol = symbol;
  }

  public IParameterSymbol Symbol { get; }


  // System.Reflection.ParameterInfo overrides

  public override ParameterAttributes Attributes
  {
    get
    {
      ParameterAttributes result = ParameterAttributes.None;
      switch (Symbol.RefKind)
      {
        case RefKind.Out:
          result |= ParameterAttributes.Out;
          break;
        case RefKind.In:
          result |= ParameterAttributes.In;
          break;
      }
      if (Symbol.HasExplicitDefaultValue)
        result |= ParameterAttributes.HasDefault;
      if (Symbol.IsOptional)
        result |= ParameterAttributes.Optional;
      return result;
    }
  }

  public override IList<CustomAttributeData> GetCustomAttributesData()
  {
    List<CustomAttributeData> result = Symbol
      .GetAttributes()
      .Select(x => (CustomAttributeData)CompilationCustomAttributeData.FromAttributeData(_runtime, x))
      .ToList();
    if (Symbol.IsParams)
    {
      result.Add(CompilationCustomAttributeData.FromParameterlessAttribute(_runtime, _runtime.Compilation.GetTypeByMetadataName("System.ParamArrayAttribute")!));
    }
    if (Symbol.IsOptional)
    {
      result.Add(CompilationCustomAttributeData.FromParameterlessAttribute(_runtime, _runtime.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.OptionalAttribute")!));
    }
    if (Symbol.RefKind is RefKind.In)
    {
      result.Add(CompilationCustomAttributeData.FromParameterlessAttribute(_runtime, _runtime.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.InAttribute")!));
      result.Add(CompilationCustomAttributeData.FromParameterlessAttribute(_runtime, _runtime.Compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.IsReadOnlyAttribute")!));
    }
    if (Symbol.RefKind is RefKind.Out)
    {
      result.Add(CompilationCustomAttributeData.FromParameterlessAttribute(_runtime, _runtime.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.OutAttribute")!));
    }
    return new ReadOnlyCollection<CustomAttributeData>(result);
  }

  public override object? DefaultValue => Symbol.HasExplicitDefaultValue ? Symbol.ExplicitDefaultValue : DBNull.Value;

  public override bool HasDefaultValue => Symbol.HasExplicitDefaultValue;

  public override MemberInfo Member => Symbol.ContainingSymbol switch
  {
    IPropertySymbol propertySymbol => _runtime.CreatePropertyInfoDelegator(propertySymbol),
    IMethodSymbol methodSymbol     => methodSymbol.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor
                                        ? _runtime.CreateConstructorInfoDelegator(methodSymbol)
                                        : _runtime.CreateMethodInfoDelegator(methodSymbol),
    _                              => throw new NotSupportedException()
  };

  public override string Name => Symbol.Name;

  public override int Position => Symbol.Ordinal;

  public override object? RawDefaultValue => Symbol.HasExplicitDefaultValue ? Symbol.ExplicitDefaultValue : DBNull.Value;

  // SymbolParameterInfoBase overrides

  protected override SymbolType ParameterTypeCore
  {
    get
    {
      SymbolType type = _runtime.CreateTypeDelegator(Symbol.Type);
      return Symbol.RefKind is not RefKind.None
        ? type.MakeByRefType()
        : type;
    }
  }

  protected override SymbolType[] GetOptionalCustomModifiersCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolType[] GetRequiredCustomModifiersCore()
  {
    throw new NotImplementedException();
  }
}