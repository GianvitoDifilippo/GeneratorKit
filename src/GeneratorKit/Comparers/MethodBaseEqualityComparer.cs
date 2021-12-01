using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Comparers;

internal class MethodBaseEqualityComparer : IEqualityComparer<MethodBase?>
{
  public static readonly MethodBaseEqualityComparer Default = new MethodBaseEqualityComparer();

  private MethodBaseEqualityComparer() { }

  public bool Equals(MethodBase? x, MethodBase? y)
  {
    if (ReferenceEquals(x, y)) return true;

    if (x is null) return y is null;
    if (y is null) return false;

    if (x is MethodInfo method1 && y is MethodInfo method2)
      return MethodInfoEqualityComparer.Default.Equals(method1, method2);

    if (x is ConstructorInfo constructor1 && y is ConstructorInfo constructor2)
      return ConstructorInfoEqualityComparer.Default.Equals(constructor1, constructor2);

    return false;
  }

  public int GetHashCode(MethodBase? obj)
  {
    if (obj is null) return 0;

    return obj switch
    {
      MethodInfo method           => MethodInfoEqualityComparer.Default.GetHashCode(method),
      ConstructorInfo constructor => ConstructorInfoEqualityComparer.Default.GetHashCode(constructor),
      _                           => obj.GetHashCode()
    };
  }
}