using System;

namespace GeneratorKit.Utils;

internal static class Errors
{
  public static Exception Unreacheable => new InvalidOperationException("Code was supposed to be unreacheable.");
}