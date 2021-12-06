using GeneratorKit.Reflection;
using System;

namespace GeneratorKit;

internal interface IProxyTypeFactory
{
  Type? CreateProxyType(GeneratorRuntime runtime, SymbolType type);
}