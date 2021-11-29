using System;
using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Comparers;

public class MemberInfoEqualityComparer : IEqualityComparer<MemberInfo?>
{
  public static readonly MemberInfoEqualityComparer Default = new MemberInfoEqualityComparer();

  private MemberInfoEqualityComparer() { }

  public bool Equals(MemberInfo? x, MemberInfo? y)
  {
    if (ReferenceEquals(x, y)) return true;

    if (x is null) return y is null;
    if (y is null) return false;

    if (x is FieldInfo field1 && y is FieldInfo field2)
      return FieldInfoEqualityComparer.Default.Equals(field1, field2);

    if (x is PropertyInfo property1 && y is PropertyInfo property2)
      return PropertyInfoEqualityComparer.Default.Equals(property1, property2);

    if (x is MethodInfo method1 && y is MethodInfo method2)
      return MethodInfoEqualityComparer.Default.Equals(method1, method2);

    if (x is ConstructorInfo constructor1 && y is ConstructorInfo constructor2)
      return ConstructorInfoEqualityComparer.Default.Equals(constructor1, constructor2);

    if (x is EventInfo event1 && y is EventInfo event2)
      return EventInfoEqualityComparer.Default.Equals(event1, event2);

    if (x is Type type1 && y is Type type2)
      return TypeEqualityComparer.Default.Equals(type1, type2);

    return false;
  }

  public int GetHashCode(MemberInfo? obj)
  {
    if (obj is null) return 0;

    return obj switch
    {
      FieldInfo field             => FieldInfoEqualityComparer.Default.GetHashCode(field),
      PropertyInfo property       => PropertyInfoEqualityComparer.Default.GetHashCode(property),
      MethodInfo method           => MethodInfoEqualityComparer.Default.GetHashCode(method),
      EventInfo @event            => EventInfoEqualityComparer.Default.GetHashCode(@event),
      ConstructorInfo constructor => ConstructorInfoEqualityComparer.Default.GetHashCode(constructor),
      Type type                   => TypeEqualityComparer.Default.GetHashCode(type),
      _                           => obj.GetHashCode()
    };
  }
}