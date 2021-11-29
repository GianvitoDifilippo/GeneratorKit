using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Comparers;

public class FieldInfoEqualityComparer : IEqualityComparer<FieldInfo?>
{
  public static readonly FieldInfoEqualityComparer Default = new FieldInfoEqualityComparer();

  private FieldInfoEqualityComparer() { }

  public bool Equals(FieldInfo? x, FieldInfo? y)
  {
    if (ReferenceEquals(x, y)) return true;

    if (x is null) return y is null;
    if (y is null) return false;

    if (x.Name != y.Name) return false;

    if (!TypeEqualityComparer.Default.Equals(x.ReflectedType, y.ReflectedType)) return false;

    return true;
  }

  public int GetHashCode(FieldInfo? obj)
  {
    if (obj is null) return 0;

    unchecked
    {
      int hashCode = 391 + TypeEqualityComparer.Default.GetHashCode(obj.DeclaringType);
      hashCode = hashCode * 23 + obj.Name.GetHashCode();

      return hashCode;
    }
  }
}