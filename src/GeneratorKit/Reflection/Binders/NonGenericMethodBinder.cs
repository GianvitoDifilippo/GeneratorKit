using System;
using System.Reflection;

namespace GeneratorKit.Reflection.Binders;

internal partial class DelegatorBinder
{
  internal class NonGenericMethodBinder : DelegatorBinder
  {
    public NonGenericMethodBinder(ParameterInfo[] parameters)
      : base(parameters)
    {
    }

    public override MethodBase? SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
    {
      MethodInfo? result = null;

      foreach (MethodInfo method in match)
      {
        if (!ParametersMatch(method.GetParameters()))
          continue;

        result = result is null ? method : throw new AmbiguousMatchException();
      }

      return result;
    }
  }
}