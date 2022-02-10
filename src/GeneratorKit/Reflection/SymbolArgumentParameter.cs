using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolArgumentParameter : SymbolParameterInfo
{
  private readonly IRuntime _runtime;
  private readonly MemberInfo _member;

  public SymbolArgumentParameter(IRuntime runtime, IGeneratorContext context, MemberInfo member, IParameterSymbol symbol)
    : base(context)
  {
    _runtime = runtime;
    _member = member;
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
      .Select(data => CompilationCustomAttributeData.FromAttributeData(Context, data) as CustomAttributeData)
      .ToList();

    if (Symbol.IsParams)
    {
      INamedTypeSymbol paramArrayAttributeSymbol = _runtime.Compilation.GetTypeByMetadataName("System.ParamArrayAttribute")!;
      result.Add(CompilationCustomAttributeData.FromParameterlessAttribute(Context, paramArrayAttributeSymbol));
    }

    if (Symbol.IsOptional)
    {
      INamedTypeSymbol optionalAttributeSymbol = _runtime.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.OptionalAttribute")!;
      result.Add(CompilationCustomAttributeData.FromParameterlessAttribute(Context, optionalAttributeSymbol));
    }

    if (Symbol.RefKind is RefKind.In)
    {
      INamedTypeSymbol inAttributeSymbol = _runtime.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.InAttribute")!;
      result.Add(CompilationCustomAttributeData.FromParameterlessAttribute(Context, inAttributeSymbol));

      INamedTypeSymbol isReadOnlyAttributeSymbol = _runtime.Compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.IsReadOnlyAttribute")!;
      result.Add(CompilationCustomAttributeData.FromParameterlessAttribute(Context, isReadOnlyAttributeSymbol));
    }

    if (Symbol.RefKind is RefKind.Out)
    {
      INamedTypeSymbol outAttributeSymbol = _runtime.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.OutAttribute")!;
      result.Add(CompilationCustomAttributeData.FromParameterlessAttribute(Context, outAttributeSymbol));
    }

    return new ReadOnlyCollection<CustomAttributeData>(result);
  }

  public override object? DefaultValue => Symbol.HasExplicitDefaultValue ? Symbol.ExplicitDefaultValue : DBNull.Value;

  public override bool HasDefaultValue => Symbol.HasExplicitDefaultValue;

  public override MemberInfo Member => _member;

  public override string Name => Symbol.Name;

  public override Type ParameterType
  {
    get
    {
      Type type = Context.GetContextType(Symbol.Type);
      return Symbol.RefKind is not RefKind.None
        ? type.MakeByRefType()
        : type;
    }
  }

  public override int Position => Symbol.Ordinal;

  public override object? RawDefaultValue => Symbol.HasExplicitDefaultValue ? Symbol.ExplicitDefaultValue : DBNull.Value;

  // SymbolParameterInfoBase overrides

  protected override SymbolType[] GetOptionalCustomModifiersCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolType[] GetRequiredCustomModifiersCore()
  {
    throw new NotImplementedException();
  }
}