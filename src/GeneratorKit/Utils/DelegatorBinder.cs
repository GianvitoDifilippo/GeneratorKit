using GeneratorKit.Comparers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Utils;

internal class DelegatorBinder : Binder
{
  private readonly int _genericParameterCount;

  public DelegatorBinder(int genericParameterCount)
  {
    _genericParameterCount = genericParameterCount;
  }

  public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture)
  {
    throw new NotImplementedException();
  }

  public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state)
  {
    throw new NotImplementedException();
  }

  public override object ChangeType(object value, Type type, CultureInfo culture)
  {
    throw new NotImplementedException();
  }

  public override void ReorderArgumentArray(ref object[] args, object state)
  {
    throw new NotImplementedException();
  }

  public override MethodBase? SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
  {
    // TODO: This can't resolve method hidden by the 'new' keyword
    foreach (MethodBase method in match)
    {
      if (!method.IsConstructor && method.GetGenericArguments().Length != _genericParameterCount) continue;

      IEnumerable<Type> parameterTypes = method.GetParameters().Select(x => x.ParameterType);
      if (!parameterTypes.SequenceEqual(types, TypeEqualityComparer.Default)) continue;

      return method;
    }

    return null;
  }

  public override PropertyInfo? SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
  {
    // TODO: This can't resolve properties hidden by the 'new' keyword
    foreach (PropertyInfo property in match)
    {
      IEnumerable<Type> indexTypes = property.GetIndexParameters().Select(x => x.ParameterType);
      if (!indexTypes.SequenceEqual(indexes, TypeEqualityComparer.Default)) continue;

      return property;
    }

    return null;
  }
}