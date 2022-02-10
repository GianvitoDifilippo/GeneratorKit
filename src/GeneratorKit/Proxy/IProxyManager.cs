using GeneratorKit.Reflection;
using System;

namespace GeneratorKit.Proxy;

internal interface IProxyManager
{
  Type GetProxyType(SymbolType type);
}
