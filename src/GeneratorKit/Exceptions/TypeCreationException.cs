using System;

namespace GeneratorKit.Exceptions;

public class TypeCreationException : Exception
{
  internal TypeCreationException(Type type)
    : base(FormatMessage(type))
  {
    Type = type;
  }

  internal TypeCreationException(Type type, Exception innerException)
    : base(FormatMessage(type), innerException)
  {
    Type = type;
  }

  public Type Type { get; }

  private static string FormatMessage(Type type) => $"Could not create proxy of type {type.Namespace}.{type.Name}.";
}
