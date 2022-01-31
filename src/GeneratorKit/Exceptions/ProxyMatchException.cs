using System;

namespace GeneratorKit.Exceptions;

public class ProxyMatchException : Exception
{
  private ProxyMatchException(string message, Type type)
    : base(message)
  {
    Type = type;
  }

  public Type Type { get; }

  internal static ProxyMatchException AmbiguousMatch(Type type) => new ProxyMatchException($"'{type.FullName}' can be assigned to more than one proxy type.", type);
  internal static ProxyMatchException NotFound(Type type) => new ProxyMatchException($"No proxy type found for '{type.FullName}'.", type);
}