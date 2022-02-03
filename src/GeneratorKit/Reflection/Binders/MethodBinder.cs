using System;
using System.Reflection;

namespace GeneratorKit.Reflection.Binders;

internal partial class DelegatorBinder
{
  internal class MethodBinder : DelegatorBinder
  {
    private readonly Type[] _genericArguments;

    public MethodBinder(Type[] parameterTypes, Type[] genericArguments)
      : base(parameterTypes)
    {
      _genericArguments = genericArguments;
    }

    private bool IsGenericMethod => _genericArguments.Length != 0;

    public override MethodBase? SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
    {
      MethodInfo? result = null;

      foreach (MethodInfo method in match)
      {
        if (IsGenericMethod != method.IsGenericMethod || !ParametersMatch(method.GetParameters()))
          continue;

        result = result is null ? method : throw new AmbiguousMatchException();
      }

      if (result is not null && IsGenericMethod)
      {
        Type[] genericArguments = new Type[_genericArguments.Length];
        Type[]? genericParameters = null;
        for (int i = 0; i < genericArguments.Length; i++)
        {
          Type genericArgument = _genericArguments[i];
          if (genericArgument.IsGenericParameter)
          {
            genericParameters ??= result.GetGenericArguments();
            genericArguments[i] = genericParameters[i];
          }
          else
          {
            genericArguments[i] = genericArgument.UnderlyingSystemType;
          }
        }
        result = result.MakeGenericMethod(genericArguments);
      }

      return result;
    }
  }
}