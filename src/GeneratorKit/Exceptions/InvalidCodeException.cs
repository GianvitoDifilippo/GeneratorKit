using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace GeneratorKit.Exceptions;

public class InvalidCodeException : Exception
{
  internal InvalidCodeException(IReadOnlyCollection<Diagnostic> errors)
    : base("User code had errors.")
  {
    Errors = errors;
  }

  public IReadOnlyCollection<Diagnostic> Errors { get; }
}