using System;

namespace GeneratorKit.Exceptions;

public class ExceptionHandler : IExceptionHandler
{
  public virtual void HandleOperationCanceledException(OperationCanceledException exception)
  {
    DefaultHandle(exception);
  }

  public virtual void HandleTypeCreationException(TypeCreationException exception)
  {
    DefaultHandle(exception);
  }

  protected virtual void DefaultHandle(Exception exception)
  {
  }
}
