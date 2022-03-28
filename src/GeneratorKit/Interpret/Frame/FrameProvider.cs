#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace GeneratorKit.Interpret.Frame;

internal class FrameProvider : IFrameProvider
{
  public IDictionary<ISymbol, object?> GetFrame(int capacity = 0)
  {
    return new Dictionary<ISymbol, object?>(capacity, SymbolDefinitionEqualityComparer.Default);
  }
}
