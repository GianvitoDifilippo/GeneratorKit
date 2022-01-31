using System;
using System.Reflection;

namespace GeneratorKit.Reflection.Binders;

internal abstract partial class DelegatorBinder
{
  private class ConstructorBinder : DelegatorBinder
  {
    public ConstructorBinder(ParameterInfo[] parameters)
      : base(parameters)
    {
    }

    public override MethodBase? SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
    {
      ConstructorInfo? result = null;

      foreach (ConstructorInfo constructor in match)
      {
        if (!ParametersMatch(constructor.GetParameters()))
          continue;

        result = result is null ? constructor : throw new AmbiguousMatchException();
      }

      return result;
    }
  }
}