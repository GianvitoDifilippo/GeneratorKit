using GeneratorKit.Reflection;
using System;

namespace GeneratorKit.Exceptions;

public class ProxyMatchException : Exception
{
  private ProxyMatchException(string message)
    : base(message)
  {
  }

  internal static ProxyMatchException AmbiguousMatch(IRuntimeType type) => new ProxyMatchException($"'{type.FullName}' can be assigned to more than one proxy type.");
  internal static ProxyMatchException NotFound(IRuntimeType type) => new ProxyMatchException($"No proxy type found for '{type.FullName}'.");
}