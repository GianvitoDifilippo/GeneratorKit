using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Exceptions;

public class OperationResolutionException : Exception
{
  internal OperationResolutionException(ISymbol symbol)
    : base($"Could not resolve operation for symbol {symbol.Name}.")
  {
    Symbol = symbol;
  }

  public ISymbol Symbol { get; }
}
