using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace GeneratorKit.Interpret.Frame;

internal interface IFrameProvider
{
  IDictionary<ISymbol, object?> GetValues(int capacity = 0);
}
