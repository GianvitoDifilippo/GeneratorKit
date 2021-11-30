using System;
using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.TestHelpers;

internal class ByNameTypeEqualityComparer : IEqualityComparer<Type>
{
  public static readonly ByNameTypeEqualityComparer Default = new ByNameTypeEqualityComparer();

  private ByNameTypeEqualityComparer() { }

  public bool Equals(Type? x, Type? y)
  {
    if (ReferenceEquals(x, y))
      return true;

    if (x is null)
      return y is null;
    if (y is null)
      return false;

    return x.Name == y.Name;
  }

  public int GetHashCode(Type obj)
  {
    if (obj is null)
      return 0;

    return obj.Name.GetHashCode();
  }
}

internal class ByNameFieldInfoEqualityComparer : IEqualityComparer<FieldInfo>
{
  public static readonly ByNameFieldInfoEqualityComparer Default = new ByNameFieldInfoEqualityComparer();

  private ByNameFieldInfoEqualityComparer() { }

  public bool Equals(FieldInfo? x, FieldInfo? y)
  {
    if (ReferenceEquals(x, y))
      return true;

    if (x is null)
      return y is null;
    if (y is null)
      return false;

    if (x.Name != y.Name)
      return false;

    return ByNameTypeEqualityComparer.Default.Equals(x.FieldType, y.FieldType);
  }

  public int GetHashCode(FieldInfo obj)
  {
    if (obj is null)
      return 0;

    unchecked
    {
      int hashCode = 391 + ByNameTypeEqualityComparer.Default.GetHashCode(obj.ReflectedType!);
      hashCode = hashCode * 23 + obj.Name.GetHashCode();

      return hashCode;
    }
  }
}

internal class ByNamePropertyInfoEqualityComparer : IEqualityComparer<PropertyInfo>
{
  public static readonly ByNamePropertyInfoEqualityComparer Default = new ByNamePropertyInfoEqualityComparer();

  private ByNamePropertyInfoEqualityComparer() { }

  public bool Equals(PropertyInfo? x, PropertyInfo? y)
  {
    if (ReferenceEquals(x, y))
      return true;

    if (x is null)
      return y is null;
    if (y is null)
      return false;

    if (x.Name != y.Name)
      return false;

    if (!ByNameTypeEqualityComparer.Default.Equals(x.ReflectedType, y.ReflectedType))
      return false;

    ParameterInfo[] parameters1 = x.GetIndexParameters();
    ParameterInfo[] parameters2 = y.GetIndexParameters();

    if (parameters1.Length != parameters2.Length)
      return false;

    for (int i = 0; i < parameters1.Length; i++)
    {
      ParameterInfo param1 = parameters1[i];
      ParameterInfo param2 = parameters2[i];

      if (!ByNameTypeEqualityComparer.Default.Equals(param1.ParameterType, param2.ParameterType))
        return false;
    }

    return true;
  }

  public int GetHashCode(PropertyInfo obj)
  {
    if (obj is null)
      return 0;

    unchecked
    {
      int hashCode = 17;
      hashCode = hashCode * 23 + ByNameTypeEqualityComparer.Default.GetHashCode(obj.ReflectedType!);
      hashCode = hashCode * 23 + obj.Name.GetHashCode();

      foreach (ParameterInfo parameter in obj.GetIndexParameters())
      {
        hashCode = hashCode * 23 + ByNameTypeEqualityComparer.Default.GetHashCode(parameter.ParameterType);
      }

      return hashCode;
    }
  }
}

public class ByNameMethodInfoEqualityComparer : IEqualityComparer<MethodInfo?>
{
  public static readonly ByNameMethodInfoEqualityComparer Default = new ByNameMethodInfoEqualityComparer();

  private ByNameMethodInfoEqualityComparer() { }

  public bool Equals(MethodInfo? x, MethodInfo? y)
  {
    if (ReferenceEquals(x, y))
      return true;

    if (x is null)
      return y is null;
    if (y is null)
      return false;

    if (x.Name != y.Name)
      return false;

    if (!ByNameTypeEqualityComparer.Default.Equals(x.ReflectedType, y.ReflectedType))
      return false;

    ParameterInfo[] parameters1 = x.GetParameters();
    ParameterInfo[] parameters2 = y.GetParameters();

    if (parameters1.Length != parameters2.Length)
      return false;

    for (int i = 0; i < parameters1.Length; i++)
    {
      ParameterInfo param1 = parameters1[i];
      ParameterInfo param2 = parameters2[i];

      if (!ByNameTypeEqualityComparer.Default.Equals(param1.ParameterType, param2.ParameterType))
        return false;
    }

    return true;
  }

  public int GetHashCode(MethodInfo obj)
  {
    if (obj is null)
      return 0;

    unchecked
    {
      int hashCode = 17;
      hashCode = hashCode * 23 + ByNameTypeEqualityComparer.Default.GetHashCode(obj.ReflectedType!);
      hashCode = hashCode * 23 + obj.Name.GetHashCode();

      foreach (ParameterInfo parameter in obj.GetParameters())
      {
        hashCode = hashCode * 23 + ByNameTypeEqualityComparer.Default.GetHashCode(parameter.ParameterType);
      }

      return hashCode;
    }
  }
}