using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal interface IRuntimeMethod
{
  SymbolMethodInfo Definition { get; }
  Type[] TypeArguments { get; }
  IRuntimeType DeclaringType { get; }
  bool IsStatic { get; }
  bool IsGenericMethod { get; }
  bool IsGenericMethodDefinition { get; }
  bool IsOpenGeneric { get; }
  bool IsSource { get; }
  MethodInfo UnderlyingSystemMethod { get; }
}
