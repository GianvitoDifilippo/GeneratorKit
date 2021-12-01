using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Comparers;

internal abstract class MethodInfoEqualityComparer : IEqualityComparer<MethodInfo?>
{
  public static readonly MethodInfoEqualityComparer Default = new DefaultComparer();
  public static readonly MethodInfoEqualityComparer Shallow = new ShallowComparer();

  public abstract bool Equals(MethodInfo? x, MethodInfo? y);
  public abstract int GetHashCode(MethodInfo? obj);

  private class DefaultComparer : MethodInfoEqualityComparer
  {
    public override bool Equals(MethodInfo? x, MethodInfo? y)
    {
      if (ReferenceEquals(x, y)) return true;

      if (x is null) return y is null;
      if (y is null) return false;

      if (x.Name != y.Name) return false;

      if (!TypeEqualityComparer.Default.Equals(x.ReflectedType, y.ReflectedType)) return false;

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

    public override int GetHashCode(MethodInfo? obj)
    {
      if (obj is null) return 0;

      unchecked
      {
        int hashCode = 17;
        hashCode = hashCode * 23 + TypeEqualityComparer.Default.GetHashCode(obj.ReflectedType);
        hashCode = hashCode * 23 + obj.Name.GetHashCode();

        foreach (ParameterInfo parameter in obj.GetParameters())
        {
          hashCode = hashCode * 23 + TypeEqualityComparer.Default.GetHashCode(parameter.ParameterType);
        }

        return hashCode;
      }
    }
  }

  private class ShallowComparer : MethodInfoEqualityComparer
  {
    public override bool Equals(MethodInfo? x, MethodInfo? y)
    {
      if (ReferenceEquals(x, y)) return true;

      if (x is null) return y is null;
      if (y is null) return false;

      if (x.Name != y.Name) return false;

      if (x.Attributes != y.Attributes) return false;

      ParameterInfo[] parameters1 = x.GetParameters();
      ParameterInfo[] parameters2 = y.GetParameters();

      if (parameters1.Length != parameters2.Length) return false;

      for (int i = 0; i < parameters1.Length; i++)
      {
        ParameterInfo param1 = parameters1[i];
        ParameterInfo param2 = parameters2[i];

        if (!TypeEqualityComparer.Shallow.Equals(param1.ParameterType, param2.ParameterType)) return false;
      }

      return true;
    }

    public override int GetHashCode(MethodInfo? obj)
    {
      if (obj is null) return 0;

      unchecked
      {
        int hashCode = 17;
        hashCode = hashCode * 23 + obj.Name.GetHashCode();
        hashCode = hashCode * 23 + obj.Attributes.GetHashCode();

        foreach (ParameterInfo parameter in obj.GetParameters())
        {
          hashCode = hashCode * 23 + TypeEqualityComparer.Shallow.GetHashCode(parameter.ParameterType);
        }

        return hashCode;
      }
    }
  }
}