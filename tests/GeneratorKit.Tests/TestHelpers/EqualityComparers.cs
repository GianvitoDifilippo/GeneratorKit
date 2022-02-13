using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GeneratorKit.TestHelpers;

internal class ArrayArrayEqualityComparer<T> : IEqualityComparer<T[][]>
{
  public bool Equals(T[][]? x, T[][]? y)
  {
    if (x is null)
      return y is null;

    if (y is null)
      return false;

    if (x.Length != y.Length)
      return false;

    for (int i = 0; i < x.Length; i++)
    {
      if (!x[i].SequenceEqual(y[i]))
        return false;
    }

    return true;
  }

  public int GetHashCode(T[][] obj)
  {
    return obj.GetHashCode();
  }
}

internal class Array3EqualityComparer<T> : IEqualityComparer<T[,,]>
{
  public bool Equals(T[,,]? x, T[,,]? y)
  {
    if (x is null)
      return y is null;

    if (y is null)
      return false;

    for (int i = 0; i < 3; i++)
    {
      if (x.GetLength(i) != y.GetLength(i))
        return false;
    }

    return x.Cast<T>().SequenceEqual(y.Cast<T>());
  }

  public int GetHashCode(T[,,] obj)
  {
    throw new NotImplementedException();
  }
}