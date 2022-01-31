using System;

namespace GeneratorKit.Proxy;

public interface IProxyTypeSetup
{
  void RegisterProxyType(Type proxyType);
}