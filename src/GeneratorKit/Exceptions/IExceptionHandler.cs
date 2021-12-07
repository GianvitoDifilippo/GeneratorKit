using System;

namespace GeneratorKit.Exceptions;

public interface IExceptionHandler
{
  void HandleTypeCreationException(TypeCreationException exception);
  void HandleOperationCanceledException(OperationCanceledException exception);
}