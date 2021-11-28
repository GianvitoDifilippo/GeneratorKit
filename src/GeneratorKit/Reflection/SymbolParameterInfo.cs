using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolParameterInfo : SymbolParameterInfoBase
{
  private readonly GeneratorRuntime _runtime;
  private readonly MemberInfo? _member;

  public SymbolParameterInfo(GeneratorRuntime runtime, IParameterSymbol symbol)
  {
    _runtime = runtime;
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
        case RefKind.Ref:
          result |= ParameterAttributes.Retval;
          break;
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
    return new ReadOnlyCollection<CustomAttributeData>(result);
  }

  public override object? DefaultValue => Symbol.HasExplicitDefaultValue ? Symbol.ExplicitDefaultValue : DBNull.Value;

  public override MemberInfo Member => _member ?? Symbol.ContainingSymbol switch
  {
    IPropertySymbol propertySymbol => _runtime.CreatePropertyInfoDelegator(propertySymbol),
    IMethodSymbol methodSymbol     => methodSymbol.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor
                                        ? _runtime.CreateConstructorInfoDelegator(methodSymbol)
                                        : _runtime.CreateMethodInfoDelegator(methodSymbol),
    _                              => throw new NotSupportedException()
  };

  public override string Name => Symbol.Name;

  public override int Position => Symbol.Ordinal;

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