using System;
using System.Reflection;

namespace GeneratorKit.Reflection.Binders;

internal partial class DelegatorBinder
{
  private class IndexerBinder : DelegatorBinder
  {
    public IndexerBinder(Type[] parameterTypes)
      : base(parameterTypes)
    {
    }

    public override PropertyInfo? SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
    {
      PropertyInfo? result = null;

      foreach (PropertyInfo property in match)
      {
        if (!ParametersMatch(property.GetIndexParameters()))
          continue;

        result = result is null ? property : throw new AmbiguousMatchException();
      }

      return result;
    }
  }
}