using System;
using System.Collections.Immutable;

namespace GeneratorKit.Utils;

internal static class ArrayExtensions
{
  public static TResult[] Map<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector)
  {
    if (source.Length == 0)
      return Array.Empty<TResult>();

    TResult[] result = new TResult[source.Length];
    for (int i = 0; i < source.Length; i++)
    {
      result[i] = selector(source[i]);
    }
    return result;
  }

  public static TResult[] Map<TSource, TResult>(this ImmutableArray<TSource> source, Func<TSource, TResult> selector)
  {
    if (source.Length == 0)
      return Array.Empty<TResult>();

    TResult[] result = new TResult[source.Length];
    for (int i = 0; i < source.Length; i++)
    {
      result[i] = selector(source[i]);
    }
    return result;
  }

  public static Array GetSubArray(this Array array, Range range, Type? elementType = null)
  {
    if (array == null)
      throw new ArgumentNullException(nameof(array));

    Type arrayType = array.GetType();
    elementType ??= arrayType.GetElementType();

    (int offset, int length) = range.GetOffsetAndLength(array.Length);

    Array dest = Array.CreateInstance(elementType, length);
    Array.Copy(array, offset, dest, 0, length);
    return dest;
  }
}