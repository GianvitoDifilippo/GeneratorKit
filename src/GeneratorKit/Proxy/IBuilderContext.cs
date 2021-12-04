using GeneratorKit.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GeneratorKit.Proxy;

internal interface IBuilderContext
{
  public TypeBuilder TypeBuilder { get; }
  public SymbolType Type { get; }

  Type ResolveType(SymbolType type, IReadOnlyDictionary<string, Type>? genericTypes = null);
}