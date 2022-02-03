using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal interface IRuntimeMethod
{
  SymbolMethodInfo Definition { get; }
  Type[] TypeArguments { get; }
  IRuntimeType DeclaringType { get; }
  string Name { get; }
  CallingConventions CallingConvention { get; }
  bool IsStatic { get; }
  bool IsGenericMethod { get; }
  bool IsGenericMethodDefinition { get; }
  bool IsOpenGeneric { get; }
  MethodInfo UnderlyingSystemMethod { get; }
}
