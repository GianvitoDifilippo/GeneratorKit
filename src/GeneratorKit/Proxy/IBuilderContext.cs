using GeneratorKit.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GeneratorKit.Proxy;

internal interface IBuilderContext
{
  TypeBuilder TypeBuilder { get; }
  SymbolType Type { get; }
  GeneratorRuntime Runtime { get; }

  Type ResolveType(SymbolType type, IReadOnlyDictionary<string, Type>? genericTypes = null);
}