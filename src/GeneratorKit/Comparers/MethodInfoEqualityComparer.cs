using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Comparers;

public class MethodInfoEqualityComparer : IEqualityComparer<MethodInfo?>
{
  public static readonly MethodInfoEqualityComparer Default = new MethodInfoEqualityComparer(TypeEqualityComparer.Default);
  public static readonly MethodInfoEqualityComparer Shallow = new MethodInfoEqualityComparer(TypeEqualityComparer.Shallow);

  private readonly TypeEqualityComparer _typeComparer;

  private MethodInfoEqualityComparer(TypeEqualityComparer typeComparer)
  {
    _typeComparer = typeComparer;
  }

  public bool Equals(MethodInfo? x, MethodInfo? y)
  {
    if (ReferenceEquals(x, y)) return true;

    if (x is null) return y is null;
    if (y is null) return false;

    if (x.Name != y.Name) return false;

    if (!_typeComparer.Equals(x.ReflectedType, y.ReflectedType)) return false;

    ParameterInfo[] parameters1 = x.GetParameters();
    ParameterInfo[] parameters2 = y.GetParameters();

    if (parameters1.Length != parameters2.Length) return false;

    for (int i = 0; i < parameters1.Length; i++)
    {
      ParameterInfo param1 = parameters1[i];
      ParameterInfo param2 = parameters2[i];

      if (!_typeComparer.Equals(param1.ParameterType, param2.ParameterType)) return false;
    }

    return true;
  }

  public int GetHashCode(MethodInfo? obj)
  {
    if (obj is null) return 0;

    unchecked
    {
      int hashCode = 17;
      hashCode = hashCode * 23 + _typeComparer.GetHashCode(obj.ReflectedType);
      hashCode = hashCode * 23 + obj.Name.GetHashCode();

      foreach (ParameterInfo parameter in obj.GetParameters())
      {
        hashCode = hashCode * 23 + _typeComparer.GetHashCode(parameter.ParameterType);
      }

      return hashCode;
    }
  }
}