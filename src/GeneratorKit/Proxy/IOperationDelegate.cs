using System;

namespace GeneratorKit.Proxy;

public interface IOperationDelegate
{
  void Invoke(int operationId, Type[] typeArguments, object?[] arguments);
  T Invoke<T>(int operationId, Type[] typeArguments, object?[] arguments);
}