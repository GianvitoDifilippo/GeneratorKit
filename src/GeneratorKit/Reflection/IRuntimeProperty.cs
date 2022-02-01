using Microsoft.CodeAnalysis;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal interface IRuntimeProperty
{
  IRuntimeMethod? Getter { get; }
  IRuntimeMethod? Setter { get; }
  IPropertySymbol Symbol { get; }
  bool IsSource { get; }
  bool IsStatic { get; }
  IRuntimeType DeclaringType { get; }
  PropertyInfo UnderlyingSystemProperty { get; }
}
