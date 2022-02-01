using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace GeneratorKit.Utils;

internal class SymbolDefinitionEqualityComparer : IEqualityComparer<ISymbol>
{
  public static readonly SymbolDefinitionEqualityComparer Default = new SymbolDefinitionEqualityComparer();

  private SymbolDefinitionEqualityComparer() { }

  public bool Equals(ISymbol x, ISymbol y)
  {
    return SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, y.OriginalDefinition);
  }

  public int GetHashCode(ISymbol obj)
  {
    return SymbolEqualityComparer.Default.GetHashCode(obj.OriginalDefinition);
  }
}