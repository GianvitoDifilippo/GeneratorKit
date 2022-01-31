using System;
using System.Reflection;

namespace GeneratorKit.Reflection.Binders;

internal partial class DelegatorBinder
{
  internal class ConstructedGenericMethodBinder : DelegatorBinder
  {
    private readonly Type[] _genericArguments;

    public ConstructedGenericMethodBinder(ParameterInfo[] parameters, Type[] genericArguments)
      : base(parameters)
    {
      _genericArguments = genericArguments;
    }

    public override MethodBase? SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
    {
      MethodInfo? result = null;

      foreach (MethodInfo method in match)
      {
        if (!method.IsGenericMethod || !ParametersMatch(method.GetParameters()))
          continue;

        result = result is null ? method : throw new AmbiguousMatchException();
      }

      if (result is not null)
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