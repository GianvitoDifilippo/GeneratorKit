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
    foreach (MethodBase method in match)
    {
      if (method.GetGenericArguments().Length != _genericParameterCount) continue;

      IEnumerable<Type> methodTypes = method.GetParameters().Select(x => x.ParameterType);
      if (!methodTypes.SequenceEqual(types, TypeEqualityComparer.Default)) continue;

      return method;
    }

    return null;
  }

  public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
  {
    throw new NotImplementedException();
  }
}