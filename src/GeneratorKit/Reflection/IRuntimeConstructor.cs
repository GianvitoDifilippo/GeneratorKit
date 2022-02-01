using Microsoft.CodeAnalysis;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal interface IRuntimeConstructor
{
  IMethodSymbol Symbol { get; }
  IRuntimeType DeclaringType { get; }
  bool IsSource { get; }
  ConstructorInfo UnderlyingSystemConstructor { get; }
}
