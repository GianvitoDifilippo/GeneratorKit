using System;
using System.Reflection.Emit;

namespace GeneratorKit.Exceptions;

public class NullEmitException : Exception
{
  public NullEmitException(TypeBuilder typeBuilder)
    : base($"TypeBuilder returned null.")
  {
    TypeBuilder = typeBuilder;
  }

  public TypeBuilder TypeBuilder { get; }
}