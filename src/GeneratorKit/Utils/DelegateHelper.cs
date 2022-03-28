using System;

namespace GeneratorKit.Utils;

internal class DelegateHelper
{
  public static Type GetActionType(Type[] typeArguments)
  {
    return typeArguments.Length switch
    {
      0  => typeof(Action),
      1  => typeof(Action<>).MakeGenericType(typeArguments),
      2  => typeof(Action<,>).MakeGenericType(typeArguments),
      3  => typeof(Action<,,>).MakeGenericType(typeArguments),
      4  => typeof(Action<,,,>).MakeGenericType(typeArguments),
      5  => typeof(Action<,,,,>).MakeGenericType(typeArguments),
      6  => typeof(Action<,,,,,>).MakeGenericType(typeArguments),
      7  => typeof(Action<,,,,,,>).MakeGenericType(typeArguments),
      8  => typeof(Action<,,,,,,,>).MakeGenericType(typeArguments),
      9  => typeof(Action<,,,,,,,,>).MakeGenericType(typeArguments),
      10 => typeof(Action<,,,,,,,,,>).MakeGenericType(typeArguments),
      11 => typeof(Action<,,,,,,,,,,>).MakeGenericType(typeArguments),
      12 => typeof(Action<,,,,,,,,,,,>).MakeGenericType(typeArguments),
      13 => typeof(Action<,,,,,,,,,,,,>).MakeGenericType(typeArguments),
      14 => typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(typeArguments),
      15 => typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(typeArguments),
      16 => typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(typeArguments),
      _  => throw new NotSupportedException("Delegates with more than 16 parameters are not supported.")
    };
  }

  public static Type GetFuncType(Type[] typeArguments)
  {
    return typeArguments.Length switch
    {
      1  => typeof(Func<>).MakeGenericType(typeArguments),
      2  => typeof(Func<,>).MakeGenericType(typeArguments),
      3  => typeof(Func<,,>).MakeGenericType(typeArguments),
      4  => typeof(Func<,,,>).MakeGenericType(typeArguments),
      5  => typeof(Func<,,,,>).MakeGenericType(typeArguments),
      6  => typeof(Func<,,,,,>).MakeGenericType(typeArguments),
      7  => typeof(Func<,,,,,,>).MakeGenericType(typeArguments),
      8  => typeof(Func<,,,,,,,>).MakeGenericType(typeArguments),
      9  => typeof(Func<,,,,,,,,>).MakeGenericType(typeArguments),
      10 => typeof(Func<,,,,,,,,,>).MakeGenericType(typeArguments),
      11 => typeof(Func<,,,,,,,,,,>).MakeGenericType(typeArguments),
      12 => typeof(Func<,,,,,,,,,,,>).MakeGenericType(typeArguments),
      13 => typeof(Func<,,,,,,,,,,,,>).MakeGenericType(typeArguments),
      14 => typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(typeArguments),
      15 => typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(typeArguments),
      16 => typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(typeArguments),
      17 => typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(typeArguments),
      _  => throw new NotSupportedException($"Delegates with more than 16 parameters are not supported.")
    };
  }
}
