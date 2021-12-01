using GeneratorKit.Reflection;
using System;

namespace GeneratorKit.Proxy;

internal interface IProxyTypeFactory
{
  Type? CreateProxyType(GeneratorRuntime runtime, SymbolType type);
}