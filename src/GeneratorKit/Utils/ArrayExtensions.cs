using System;
using System.Collections.Immutable;

namespace GeneratorKit.Utils;

internal static class ArrayExtensions
{
  public static TResult[] Map<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector)
  {
    TResult[] result = new TResult[source.Length];
    for (int i = 0; i < source.Length; i++)
    {
      result[i] = selector(source[i]);
    }
    return result;
  }

  public static TResult[] Map<TSource, TResult>(this ImmutableArray<TSource> source, Func<TSource, TResult> selector)
  {
    TResult[] result = new TResult[source.Length];
    for (int i = 0; i < source.Length; i++)
    {
      result[i] = selector(source[i]);
    }
    return result;
  }
}