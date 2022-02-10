using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Utils;

internal class DelegateHelper
{
  public static Type GetDelegateType(IGeneratorContext context, IMethodSymbol method)
  {
    return method.ReturnsVoid
      ? GetActionType(context, method)
      : GetFuncType(context, method);
  }

  private static Type GetActionType(IGeneratorContext context, IMethodSymbol method)
  {
    Type[] types = method.Parameters.Map(p => context.GetContextType(p.Type).UnderlyingSystemType);
    return types.Length switch
    {
      0 => typeof(Action),
      1 => typeof(Action<>).MakeGenericType(types),
      2 => typeof(Action<,>).MakeGenericType(types),
      3 => typeof(Action<,,>).MakeGenericType(types),
      4 => typeof(Action<,,,>).MakeGenericType(types),
      5 => typeof(Action<,,,,>).MakeGenericType(types),
      6 => typeof(Action<,,,,,>).MakeGenericType(types),
      7 => typeof(Action<,,,,,,>).MakeGenericType(types),
      8 => typeof(Action<,,,,,,,>).MakeGenericType(types),
      9 => typeof(Action<,,,,,,,,>).MakeGenericType(types),
      10 => typeof(Action<,,,,,,,,,>).MakeGenericType(types),
      11 => typeof(Action<,,,,,,,,,,>).MakeGenericType(types),
      12 => typeof(Action<,,,,,,,,,,,>).MakeGenericType(types),
      13 => typeof(Action<,,,,,,,,,,,,>).MakeGenericType(types),
      14 => typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(types),
      15 => typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(types),
      16 => typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(types),
      _ => throw new NotSupportedException("Delegates with more than 16 parameters are not supported.")
    };
  }

  private static Type GetFuncType(IGeneratorContext context, IMethodSymbol method)
  {
    Type[] types = new Type[method.Parameters.Length + 1];
    types[0] = context.GetContextType(method.ReturnType).UnderlyingSystemType;
    for (int i = 0; i < method.Parameters.Length; i++)
    {
      types[i + 1] = context.GetContextType(method.Parameters[i].Type).UnderlyingSystemType;
    }
    return types.Length switch
    {
      1 => typeof(Func<>).MakeGenericType(types),
      2 => typeof(Func<,>).MakeGenericType(types),
      3 => typeof(Func<,,>).MakeGenericType(types),
      4 => typeof(Func<,,,>).MakeGenericType(types),
      5 => typeof(Func<,,,,>).MakeGenericType(types),
      6 => typeof(Func<,,,,,>).MakeGenericType(types),
      7 => typeof(Func<,,,,,,>).MakeGenericType(types),
      8 => typeof(Func<,,,,,,,>).MakeGenericType(types),
      9 => typeof(Func<,,,,,,,,>).MakeGenericType(types),
      10 => typeof(Func<,,,,,,,,,>).MakeGenericType(types),
      11 => typeof(Func<,,,,,,,,,,>).MakeGenericType(types),
      12 => typeof(Func<,,,,,,,,,,,>).MakeGenericType(types),
      13 => typeof(Func<,,,,,,,,,,,,>).MakeGenericType(types),
      14 => typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(types),
      15 => typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(types),
      16 => typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(types),
      17 => typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(types),
      _ => throw new NotSupportedException("Delegates with more than 16 parameters are not supported.")
    };
  }
}