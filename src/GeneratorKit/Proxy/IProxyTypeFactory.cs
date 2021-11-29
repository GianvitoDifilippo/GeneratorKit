using GeneratorKit.Reflection;
using System;

namespace GeneratorKit.Proxy;

internal interface IProxyTypeFactory
{
  Type? BuildProxyType(GeneratorRuntime runtime, SymbolType type);
}