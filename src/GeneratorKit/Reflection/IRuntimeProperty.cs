using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal interface IRuntimeProperty
{
  IPropertySymbol Symbol { get; }
  IRuntimeMethod? Getter { get; }
  IRuntimeMethod? Setter { get; }
  IRuntimeType DeclaringType { get; }
  string Name { get; }
  bool IsStatic { get; }
  Type[] ParameterTypes { get; }
  Type PropertyType { get; }
  PropertyInfo UnderlyingSystemProperty { get; }
}
