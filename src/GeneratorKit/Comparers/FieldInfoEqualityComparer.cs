using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Comparers;

internal abstract class FieldInfoEqualityComparer : IEqualityComparer<FieldInfo?>
{
  public static readonly FieldInfoEqualityComparer Default = new DefaultComparer();
  public static readonly FieldInfoEqualityComparer Shallow = new ShallowComparer();

  public abstract bool Equals(FieldInfo? x, FieldInfo? y);
  public abstract int GetHashCode(FieldInfo? obj);
  
  private class DefaultComparer : FieldInfoEqualityComparer
  {
    public override bool Equals(FieldInfo? x, FieldInfo? y)
    {
      if (ReferenceEquals(x, y)) return true;

      if (x is null) return y is null;
      if (y is null) return false;

      if (x.Name != y.Name) return false;

      if (!TypeEqualityComparer.Default.Equals(x.ReflectedType, y.ReflectedType)) return false;

      return true;
    }

    public override int GetHashCode(FieldInfo? obj)
    {
      if (obj is null) return 0;

      unchecked
      {
        int hashCode = 391 + TypeEqualityComparer.Default.GetHashCode(obj.ReflectedType);
        hashCode = hashCode * 23 + obj.Name.GetHashCode();

        return hashCode;
      }
    }
  }

  private class ShallowComparer : FieldInfoEqualityComparer
  {
    public override bool Equals(FieldInfo? x, FieldInfo? y)
    {
      if (ReferenceEquals(x, y)) return true;

      if (x is null) return y is null;
      if (y is null) return false;

      if (x.Name != y.Name) return false;

      if (x.Attributes != y.Attributes) return false;

      return true;
    }

    public override int GetHashCode(FieldInfo? obj)
    {
      if (obj is null) return 0;

      unchecked
      {
        int hashCode = 391 + obj.Name.GetHashCode();
        hashCode = hashCode * 23 + obj.Attributes.GetHashCode();

        return hashCode;
      }
    }
  }
}