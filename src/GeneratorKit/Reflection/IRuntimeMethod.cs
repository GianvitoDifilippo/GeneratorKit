using System;

namespace GeneratorKit.Reflection;

internal interface IRuntimeMethod
{
  SymbolMethodInfo Definition { get; }
  Type[] TypeArguments { get; }
  IRuntimeType DeclaringType { get; }
  bool IsStatic { get; }
  bool ContainsGenericParameters { get; }
  bool IsSource { get; }
}
