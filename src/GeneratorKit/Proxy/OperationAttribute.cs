using System;

namespace GeneratorKit.Proxy;

internal class OperationAttribute : Attribute
{
  public OperationAttribute(int operationId)
  {
    OperationId = operationId;
  }

  public int OperationId { get; }
}
