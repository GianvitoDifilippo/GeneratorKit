using GeneratorKit.Comparers;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Reflection;

namespace GeneratorKit.Reflection.Binders;

internal abstract partial class DelegatorBinder : Binder
{
  protected DelegatorBinder(ParameterInfo[] parameters)
  {
    Parameters = parameters;
  }

  protected ParameterInfo[] Parameters { get; }

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
    if (Parameters.Length != parameters.Length)
      return false;

    for (int i = 0; i < Parameters.Length; i++)
    {
      if (!TypeEqualityComparer.Default.Equals(Parameters[i].ParameterType, parameters[i].ParameterType))
        return false;
    }

    return true;
  }

  public static PropertyInfo ResolveProperty(Type type, SymbolPropertyInfo property)
  {
    BindingFlags bindingAttr = GetBindingAttr(property.Symbol);
    ParameterInfo[] parameters = property.GetIndexParameters();
    
    if (parameters.Length != 0)
    {
      Type[] parameterTypes = parameters.Map(p => p.ParameterType);
      IndexerBinder binder = new IndexerBinder(parameters);

      return type.GetProperty(property.Name, bindingAttr, binder, property.PropertyType, parameterTypes, null);
    }

    return type.GetProperty(property.Name, bindingAttr);
  }

  public static MethodInfo ResolveMethod(Type type, SymbolMethodInfo method)
  {
    return method.IsGenericMethod
      ? method.IsGenericMethodDefinition
        ? ResolveGenericMethodDefinition(type, method)
        : ResolveConstructedGenericMethod(type, method.GetGenericMethodDefinition(), method.GetGenericArguments())
      : ResolveNonGenericMethod(type, method);
  }

  public static MethodInfo ResolveMethod(Type type, HybridGenericMethod method)
  {
    return ResolveConstructedGenericMethod(type, method.GetGenericMethodDefinition(), method.GetGenericArguments());
  }

  public static ConstructorInfo ResolveConstructor(Type type, SymbolConstructorInfo constructor)
  {
    BindingFlags bindingAttr = GetBindingAttr(constructor.Symbol);
    ParameterInfo[] parameters = constructor.GetParameters();
    Type[] parameterTypes = parameters.Map(p => p.ParameterType);

    ConstructorBinder binder = new ConstructorBinder(parameters);
    return type.GetConstructor(bindingAttr, binder, constructor.CallingConvention, parameterTypes, null);
  }

  public static FieldInfo ResolveField(Type type, SymbolFieldInfo field)
  {
    BindingFlags bindingAttr = GetBindingAttr(field.Symbol);

    return type.GetField(field.Name, bindingAttr);
  }

  private static MethodInfo ResolveNonGenericMethod(Type type, SymbolMethodInfo method)
  {
    BindingFlags bindingAttr = GetBindingAttr(method.Symbol);
    ParameterInfo[] parameters = method.GetParameters();
    Type[] parameterTypes = parameters.Map(p => p.ParameterType);

    NonGenericMethodBinder binder = new NonGenericMethodBinder(parameters);
    return type.GetMethod(method.Name, bindingAttr, binder, method.CallingConvention, parameterTypes, null);
  }

  private static MethodInfo ResolveGenericMethodDefinition(Type type, SymbolMethodInfo method)
  {
    BindingFlags bindingAttr = GetBindingAttr(method.Symbol);
    ParameterInfo[] parameters = method.GetParameters();
    Type[] parameterTypes = parameters.Map(p => p.ParameterType);

    GenericMethodDefinitionBinder binder = new GenericMethodDefinitionBinder(parameters);
    return type.GetMethod(method.Name, bindingAttr, binder, method.CallingConvention, parameterTypes, null);
  }

  private static MethodInfo ResolveConstructedGenericMethod(Type type, SymbolMethodInfo methodDefinition, Type[] genericArguments)
  {
    BindingFlags bindingAttr = GetBindingAttr(methodDefinition.Symbol);
    ParameterInfo[] parameters = methodDefinition.GetParameters();
    Type[] parameterTypes = parameters.Map(p => p.ParameterType);

    ConstructedGenericMethodBinder binder = new ConstructedGenericMethodBinder(parameters, genericArguments);
    return type.GetMethod(methodDefinition.Name, bindingAttr, binder, methodDefinition.CallingConvention, parameterTypes, null);
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