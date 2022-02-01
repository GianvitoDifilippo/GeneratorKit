using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace GeneratorKit.Interpret;

internal interface IFrameDictionaryProvider
{
  IDictionary<ISymbol, object?> GetValues(int capacity = 0);
}
