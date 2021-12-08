using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GeneratorKit.Emit;

internal interface IBuildContext
{
  GeneratorRuntime Runtime { get; }
  TypeBuilder TypeBuilder { get; }

  IOperation? GetOperation(ISymbol symbol);
  IOperation? GetOperation(SyntaxNode node);
  Type ResolveType(SymbolType type, IReadOnlyDictionary<string, Type>? genericParameters = null);
}