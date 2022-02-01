namespace Microsoft.CodeAnalysis;

internal static class SymbolExtensions
{
  public static bool IsSource(this ISymbol symbol) => symbol.ContainingAssembly is ISourceAssemblySymbol;
}
