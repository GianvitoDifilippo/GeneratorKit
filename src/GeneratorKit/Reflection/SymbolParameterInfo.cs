using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolParameterInfo : ParameterInfo
{
  private readonly GeneratorRuntime _runtime;

  public SymbolParameterInfo(GeneratorRuntime runtime, IParameterSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IParameterSymbol Symbol { get; }


  public override Type ParameterType => _runtime.CreateTypeDelegator(Symbol.Type);

  public override Type[] GetOptionalCustomModifiers()
  {
    throw new NotImplementedException();
  }

  public override Type[] GetRequiredCustomModifiers()
  {
    throw new NotImplementedException();
  }
}