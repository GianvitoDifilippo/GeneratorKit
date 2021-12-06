using GeneratorKit.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GeneratorKit.Emit;

internal interface IBuildContext
{
  GeneratorRuntime Runtime { get; }
  TypeBuilder TypeBuilder { get; }

  Type ResolveType(SymbolType type, IReadOnlyDictionary<string, Type>? genericParameters = null);
}