using GeneratorKit.Reflection;
using System;

namespace GeneratorKit.Exceptions;

public class ProxyMatchException : Exception
{
  private ProxyMatchException(Type type, string message)
    : base(message)
  {
    Type = type;
  }

  public Type Type { get; }

  internal static ProxyMatchException AmbiguousMatch(SymbolType type) => new ProxyMatchException(type, $"'{type.FullName}' can be assigned to more than one proxy type.");
  internal static ProxyMatchException NotFound(SymbolType type) => new ProxyMatchException(type, $"No proxy type found for '{type.FullName}'.");
}