using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratorKit.Comparers;

public class TypeEqualityComparer : IEqualityComparer<Type>
{
  public static readonly TypeEqualityComparer Default = new TypeEqualityComparer();

  private TypeEqualityComparer() { }

  public bool Equals(Type? x, Type? y)
  {
    if (ReferenceEquals(x, y)) return true;

    if (x is null) return y is null;
    if (y is null) return false;

    if (x.IsArray)
    {
      if (!y.IsArray) return false;

      return Equals(x.GetElementType(), y.GetElementType());
    }
    else if (y.IsArray) return false;

    if (x.IsByRef)
    {
      if (!y.IsByRef) return false;

      return Equals(x.GetElementType(), y.GetElementType());
    }
    else if (y.IsByRef) return false;

    if (x.Name != y.Name) return false;
    if (x.Namespace != y.Namespace) return false;
    if (!AssemblyEqualityComparer.Default.Equals(x.Assembly, y.Assembly)) return false;

    if (x.IsGenericType)
    {
      if (!y.IsGenericType) return false;
      if (x.IsGenericTypeDefinition != y.IsGenericTypeDefinition) return false;
      if (!x.GetGenericArguments().SequenceEqual(y.GetGenericArguments(), this)) return false;
    }
    else if (y.IsGenericType) return false;

    if (x.IsGenericParameter)
    {
      if (!y.IsGenericParameter) return false;
      if (x.GenericParameterPosition != y.GenericParameterPosition) return false;
    }
    else if (y.IsGenericParameter) return false;


    return true;
  }

  public int GetHashCode(Type? obj)
  {
    if (obj is null) return 0;

    unchecked
    {
      int hashCode = 391 + (obj.Name?.GetHashCode() ?? 0);
      hashCode = hashCode * 23 + (obj.Namespace?.GetHashCode() ?? 0);
      hashCode = hashCode * 23 + AssemblyEqualityComparer.Default.GetHashCode(obj.Assembly);
      hashCode = hashCode * 23 + obj.IsGenericType.GetHashCode();
      hashCode = hashCode * 23 + obj.IsGenericParameter.GetHashCode();

      return hashCode;
    }
  }
}