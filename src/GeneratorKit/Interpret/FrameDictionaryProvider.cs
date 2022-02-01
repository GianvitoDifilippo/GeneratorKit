#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace GeneratorKit.Interpret;

internal class FrameDictionaryProvider : IFrameDictionaryProvider
{
  public IDictionary<ISymbol, object?> GetValues(int capacity = 0)
  {
    return new Dictionary<ISymbol, object?>(capacity, SymbolDefinitionEqualityComparer.Default);
  }
}
