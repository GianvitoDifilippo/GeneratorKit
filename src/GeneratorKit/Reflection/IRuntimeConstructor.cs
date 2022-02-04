using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal interface IRuntimeConstructor
{
  CallingConventions CallingConvention { get; }
  IMethodSymbol Symbol { get; }
  IRuntimeType DeclaringType { get; }
  Type[] ParameterTypes { get; }
  ConstructorInfo UnderlyingSystemConstructor { get; }
}
