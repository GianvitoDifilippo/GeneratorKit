using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Utils;

internal class GetTypeVisitor : SymbolVisitor<SymbolType?>
{
  private readonly GeneratorRuntime _runtime;
  private readonly string[] _segments;
  private readonly StringComparison _comparison;
  private int _index;

  public GetTypeVisitor(GeneratorRuntime runtime, string name, bool ignoreCase)
  {
    _runtime = runtime;
    _segments = name.Split('.');
    _comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    _index = -1;
  }

  public override SymbolType? VisitNamespace(INamespaceSymbol symbol)
  {
    if (_index != -1 && !symbol.Name.Equals(_segments[_index], _comparison))
      return null;

    _index++;
    foreach (INamespaceOrTypeSymbol member in symbol.GetMembers())
    {
      SymbolType? result = member.Accept(this);
      if (result is not null)
      {
        return result;
      }
    }

    return null;
  }

  public override SymbolType? VisitNamedType(INamedTypeSymbol symbol)
  {
    if (_index != _segments.Length - 1)
      return null;

    return symbol.MetadataName.Equals(_segments[_index], _comparison)
      ? _runtime.CreateTypeDelegator(symbol)
      : null;
  }
}