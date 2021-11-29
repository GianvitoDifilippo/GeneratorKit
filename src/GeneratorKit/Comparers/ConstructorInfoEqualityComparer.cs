using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Comparers;

public class ConstructorInfoEqualityComparer : IEqualityComparer<ConstructorInfo?>
{
  public static readonly ConstructorInfoEqualityComparer Default = new ConstructorInfoEqualityComparer();

  private ConstructorInfoEqualityComparer() { }

  public bool Equals(ConstructorInfo? x, ConstructorInfo? y)
  {
    if (ReferenceEquals(x, y)) return true;

    if (x is null) return y is null;
    if (y is null) return false;

    if (x.IsStatic != y.IsStatic) return false;

    if (!TypeEqualityComparer.Default.Equals(x.DeclaringType, y.DeclaringType)) return false;

    ParameterInfo[] parameters1 = x.GetParameters();
    ParameterInfo[] parameters2 = y.GetParameters();

    if (parameters1.Length != parameters2.Length) return false;

    for (int i = 0; i < parameters1.Length; i++)
    {
      ParameterInfo param1 = parameters1[i];
      ParameterInfo param2 = parameters2[i];

      if (!TypeEqualityComparer.Default.Equals(param1.ParameterType, param2.ParameterType)) return false;
    }

    return true;
  }

  public int GetHashCode(ConstructorInfo? obj)
  {
    if (obj is null) return 0;

    unchecked
    {
      int hashCode = 17;
      hashCode = hashCode * 23 + TypeEqualityComparer.Default.GetHashCode(obj.DeclaringType);

      foreach (ParameterInfo parameter in obj.GetParameters())
      {
        hashCode = hashCode * 23 + TypeEqualityComparer.Default.GetHashCode(parameter.ParameterType);
      }

      return hashCode;
    }
  }
}