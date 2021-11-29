using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using System;

namespace GeneratorKit.TestHelpers;

internal class FakeProxyTypeFactory : IProxyTypeFactory
{
  public Type? BuildProxyType(GeneratorRuntime runtime, SymbolType type)
  {
    return null;
  }
}