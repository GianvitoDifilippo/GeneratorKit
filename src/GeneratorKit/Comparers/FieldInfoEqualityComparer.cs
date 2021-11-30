using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Comparers;

public class FieldInfoEqualityComparer : IEqualityComparer<FieldInfo?>
{
  public static readonly FieldInfoEqualityComparer Default = new FieldInfoEqualityComparer(TypeEqualityComparer.Default);
  public static readonly FieldInfoEqualityComparer Shallow = new FieldInfoEqualityComparer(TypeEqualityComparer.Shallow);

  private readonly TypeEqualityComparer _typeComparer;

  private FieldInfoEqualityComparer(TypeEqualityComparer typeComparer)
  {
    _typeComparer = typeComparer;
  }

  public bool Equals(FieldInfo? x, FieldInfo? y)
  {
    if (ReferenceEquals(x, y)) return true;

    if (x is null) return y is null;
    if (y is null) return false;

    if (x.Name != y.Name) return false;

    if (!_typeComparer.Equals(x.ReflectedType, y.ReflectedType)) return false;

    return true;
  }

  public int GetHashCode(FieldInfo? obj)
  {
    if (obj is null) return 0;

    unchecked
    {
      int hashCode = 391 + _typeComparer.GetHashCode(obj.ReflectedType);
      hashCode = hashCode * 23 + obj.Name.GetHashCode();

      return hashCode;
    }
  }
}