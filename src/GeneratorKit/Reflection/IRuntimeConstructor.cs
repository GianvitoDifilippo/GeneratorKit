using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal interface IRuntimeConstructor
{
  IMethodSymbol Symbol { get; }
  IRuntimeType DeclaringType { get; }
  CallingConventions CallingConvention { get; }
  Type[] ParameterTypes { get; }
  ConstructorInfo UnderlyingSystemConstructor { get; }
}
