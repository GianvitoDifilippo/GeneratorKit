using System;

namespace GeneratorKit.Reflection;

internal interface IRuntimeType
{
  SymbolNamedType Definition { get; }
  Type[] TypeArguments { get; }
  IRuntimeType? BaseType { get; }
  bool IsSource { get; }
  bool ContainsGenericParameters { get; }
  bool IsInterface { get; }
  bool IsAbstract { get; }
  string? FullName { get; }
  Type UnderlyingSystemType { get; }
}
