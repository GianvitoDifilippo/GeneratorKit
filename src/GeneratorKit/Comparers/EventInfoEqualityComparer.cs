using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Comparers;

internal abstract class EventInfoEqualityComparer : IEqualityComparer<EventInfo?>
{
  public static readonly EventInfoEqualityComparer Default = new DefaultComparer();
  public static readonly EventInfoEqualityComparer Shallow = new ShallowComparer();

  public abstract bool Equals(EventInfo? x, EventInfo? y);
  public abstract int GetHashCode(EventInfo? obj);

  private class DefaultComparer : EventInfoEqualityComparer
  {
    public override bool Equals(EventInfo? x, EventInfo? y)
    {
      if (ReferenceEquals(x, y)) return true;

      if (x is null) return y is null;
      if (y is null) return false;

      if (x.Name != y.Name) return false;

      if (!TypeEqualityComparer.Default.Equals(x.ReflectedType, y.ReflectedType)) return false;

      return true;
    }

    public override int GetHashCode(EventInfo? obj)
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

  private class ShallowComparer : EventInfoEqualityComparer
  {
    public override bool Equals(EventInfo? x, EventInfo? y)
    {
      if (ReferenceEquals(x, y)) return true;

      if (x is null) return y is null;
      if (y is null) return false;

      if (x.Name != y.Name) return false;

      if (x.Attributes != y.Attributes) return false;

      return true;
    }

    public override int GetHashCode(EventInfo? obj)
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