using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Comparers;

public class EventInfoEqualityComparer : IEqualityComparer<EventInfo?>
{
  public static readonly EventInfoEqualityComparer Default = new EventInfoEqualityComparer(TypeEqualityComparer.Default);
  public static readonly EventInfoEqualityComparer Shallow = new EventInfoEqualityComparer(TypeEqualityComparer.Shallow);

  private readonly TypeEqualityComparer _typeComparer;

  private EventInfoEqualityComparer(TypeEqualityComparer typeComparer)
  {
    _typeComparer = typeComparer;
  }

  public bool Equals(EventInfo? x, EventInfo? y)
  {
    if (ReferenceEquals(x, y)) return true;

    if (x is null) return y is null;
    if (y is null) return false;

    if (x.Name != y.Name) return false;

    if (!_typeComparer.Equals(x.ReflectedType, y.ReflectedType)) return false;

    return true;
  }

  public int GetHashCode(EventInfo? obj)
  {
    if (obj is null) return 0;

    unchecked
    {
      int hashCode = 391 + _typeComparer.GetHashCode(obj.DeclaringType);
      hashCode = hashCode * 23 + obj.Name.GetHashCode();

      return hashCode;
    }
  }
}