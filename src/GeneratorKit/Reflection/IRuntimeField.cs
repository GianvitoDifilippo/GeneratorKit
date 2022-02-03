using Microsoft.CodeAnalysis;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal interface IRuntimeField
{
  string Name { get; }
  IFieldSymbol Symbol { get; }
  IRuntimeType DeclaringType { get; }
  bool IsStatic { get; }
  FieldInfo UnderlyingSystemField { get; }
}
