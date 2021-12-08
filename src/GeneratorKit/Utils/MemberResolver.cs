using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Utils;

internal static class MemberResolver
{
  public static FieldInfo ResolveField(Type type, SymbolFieldInfo field)
  {
    BindingFlags bindingAttr = GetBindingAttr(field.Symbol);
    return type.GetField(field.Name, bindingAttr) ?? throw new InvalidOperationException($"Cannot resolve field {field.Name} on type {type.Name}.");
  }

  public static PropertyInfo ResolveProperty(Type type, SymbolPropertyInfo property)
  {
    BindingFlags bindingAttr = GetBindingAttr(property.Symbol);
    
    if (property.Name is "Item")
    {
      SymbolType[] parameters = property.GetIndexParameters().Map(x => x.ParameterType);
      return type.GetProperty("Item", bindingAttr, new DelegatorBinder(0), property.PropertyType, parameters, null);
    }
    return type.GetProperty(property.Name, bindingAttr) ?? throw new InvalidOperationException($"Cannot resolve property {property.Name} on type {type.Name}.");
  }

  public static MethodInfo ResolveMethod(Type type, SymbolMethodInfo method)
  {
    BindingFlags bindingAttr = GetBindingAttr(method.Symbol);
    int genericParameterCount = method.GetGenericArguments().Length;
    SymbolType[] parameters = method.GetParameters().Map(x => x.ParameterType);

    return type.GetMethod(method.Name, bindingAttr, new DelegatorBinder(genericParameterCount), parameters, null) ?? throw new InvalidOperationException($"Cannot resolve method {method.Name} on type {type.Name}.");
  }

  public static ConstructorInfo ResolveConstructor(Type type, SymbolConstructorInfo constructor)
  {
    BindingFlags bindingAttr = GetBindingAttr(constructor.Symbol);
    SymbolType[] parameters = constructor.GetParameters().Map(x => x.ParameterType);

    return type.GetConstructor(bindingAttr, new DelegatorBinder(0), parameters, null) ?? throw new InvalidOperationException($"Cannot resolve constructor on type {type.Name}.");
  }

  private static BindingFlags GetBindingAttr(ISymbol symbol)
  {
    BindingFlags result = default;
    result |= symbol.DeclaredAccessibility is Accessibility.Public ? BindingFlags.Public : BindingFlags.NonPublic;
    result |= symbol.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
    return result;
  }
}