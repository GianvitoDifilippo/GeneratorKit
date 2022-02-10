using GeneratorKit.Comparers;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Reflection;

namespace GeneratorKit.Reflection.Binders;

internal abstract partial class DelegatorBinder : Binder
{
  private readonly Type[] _parameterTypes;

  protected DelegatorBinder(Type[] parameterTypes)
  {
    _parameterTypes = parameterTypes;
  }

  public override MethodBase? SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
  {
    throw new NotSupportedException("Wrong binder provided.");
  }

  public override PropertyInfo? SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
  {
    throw new NotSupportedException("Wrong binder provided.");
  }

  protected bool ParametersMatch(ParameterInfo[] parameters)
  {
    if (_parameterTypes.Length != parameters.Length)
      return false;

    for (int i = 0; i < _parameterTypes.Length; i++)
    {
      if (!TypeEqualityComparer.Default.Equals(_parameterTypes[i], parameters[i].ParameterType))
        return false;
    }

    return true;
  }

  public static PropertyInfo ResolveProperty(Type type, SymbolPropertyInfo property)
  {
    BindingFlags bindingAttr = GetBindingAttr(property.OriginalSymbol);

    PropertyInfo? result;
    if (property.IsIndexer)
    {
      Type[] parameterTypes = property.ParameterTypes;
      IndexerBinder binder = new IndexerBinder(parameterTypes);

      result = type.GetProperty(property.Name, bindingAttr, binder, property.PropertyType, parameterTypes, null);
    }
    else
    {
      result = type.GetProperty(property.Name, bindingAttr);
    }

    return result ?? throw new InvalidOperationException($"Cannot resolve property {property} in type {type}.");
  }

  public static MethodInfo ResolveMethod(Type type, SymbolMethodInfo method)
  {
    BindingFlags bindingAttr = GetBindingAttr(method.OriginalSymbol);
    Type[] parameterTypes = method.ParameterTypes;

    MethodBinder binder = new MethodBinder(parameterTypes, method.GetGenericArguments());
    return type.GetMethod(method.Name, bindingAttr, binder, method.CallingConvention, parameterTypes, null)
      ?? throw new InvalidOperationException($"Cannot resolve method {method} in type {type}.");
  }

  public static ConstructorInfo ResolveConstructor(Type type, SymbolConstructorInfo constructor)
  {
    BindingFlags bindingAttr = GetBindingAttr(constructor.OriginalSymbol);

    ConstructorBinder binder = new ConstructorBinder(constructor.ParameterTypes);
    return type.GetConstructor(bindingAttr, binder, constructor.CallingConvention, constructor.ParameterTypes, null)
      ?? throw new InvalidOperationException($"Cannot resolve constructor {constructor} in type {type}.");
  }

  public static FieldInfo ResolveField(Type type, SymbolFieldInfo field)
  {
    BindingFlags bindingAttr = GetBindingAttr(field.OriginalSymbol);

    return type.GetField(field.Name, bindingAttr)
      ?? throw new InvalidOperationException($"Cannot resolve field {field} in type {type}.");
  }

  private static BindingFlags GetBindingAttr(ISymbol symbol)
  {
    BindingFlags result = default;
    result |= symbol.DeclaredAccessibility is Accessibility.Public ? BindingFlags.Public : BindingFlags.NonPublic;
    result |= symbol.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
    return result;
  }

  #region Not implemented
  public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture) => throw new NotImplementedException();
  public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state) => throw new NotImplementedException();
  public override object ChangeType(object value, Type type, CultureInfo culture) => throw new NotImplementedException();
  public override void ReorderArgumentArray(ref object[] args, object state) => throw new NotImplementedException();
  #endregion
}