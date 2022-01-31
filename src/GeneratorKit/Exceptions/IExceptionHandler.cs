using System;

namespace GeneratorKit.Exceptions;

public interface IExceptionHandler
{
  void HandleOperationCanceledException(OperationCanceledException exception);
}