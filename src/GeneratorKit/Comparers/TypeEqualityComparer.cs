using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratorKit.Comparers;

internal abstract class TypeEqualityComparer : IEqualityComparer<Type?>
{
  public static readonly TypeEqualityComparer Default = new DefaultComparer();
  public static readonly TypeEqualityComparer Shallow = new ShallowComparer();

  public abstract bool Equals(Type? x, Type? y);
  public abstract int GetHashCode(Type? obj);

  private class DefaultComparer : TypeEqualityComparer
  {
    public override bool Equals(Type? x, Type? y)
    {
      if (ReferenceEquals(x, y)) return true;

      if (x is null) return y is null;
      if (y is null) return false;

      if (x.IsArray)
      {
        if (!y.IsArray) return false;

        if (x.GetArrayRank() != y.GetArrayRank()) return false;

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

      // TODO: Here there should be a sort of assembly equivalence.
      // if (!AssemblyEqualityComparer.Default.Equals(x.Assembly, y.Assembly)) return false;

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

    public override int GetHashCode(Type? obj)
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

  private class ShallowComparer : TypeEqualityComparer
  {
    public override bool Equals(Type? x, Type? y)
    {
      if (ReferenceEquals(x, y))
        return true;

      if (x is null)
        return y is null;
      if (y is null)
        return false;

      if (x.Name != y.Name) return true;

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

    public override int GetHashCode(Type? obj)
    {
      if (obj is null) return 0;

      unchecked
      {
        int hashCode = 391 + (obj.Name?.GetHashCode() ?? 0);
        hashCode = hashCode * 23 + obj.IsGenericType.GetHashCode();
        hashCode = hashCode * 23 + obj.IsGenericParameter.GetHashCode();

        return hashCode;
      }
    }
  }
}