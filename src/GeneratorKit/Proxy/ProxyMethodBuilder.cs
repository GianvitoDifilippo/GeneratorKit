#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GeneratorKit.Proxy;


internal class ProxyMethodBuilder
{
  private readonly IBuilderContext _context;
  private readonly TypeBuilder _typeBuilder;
  private readonly IReadOnlyDictionary<IPropertySymbol, FieldBuilder> _backingFields;
  private readonly Dictionary<IPropertySymbol, MethodBuilder> _getters;
  private readonly Dictionary<IPropertySymbol, MethodBuilder> _setters;

  public ProxyMethodBuilder(IBuilderContext context, IReadOnlyDictionary<IPropertySymbol, FieldBuilder> backingFields)
  {
    _context = context;
    _typeBuilder = context.TypeBuilder;
    _backingFields = backingFields;
    _getters = new Dictionary<IPropertySymbol, MethodBuilder>(SymbolEqualityComparer.Default);
    _setters = new Dictionary<IPropertySymbol, MethodBuilder>(SymbolEqualityComparer.Default);
  }

  public IReadOnlyDictionary<IPropertySymbol, MethodBuilder> Getters => _getters;

  public IReadOnlyDictionary<IPropertySymbol, MethodBuilder> Setters => _setters;

  public void BuildMethod(SymbolMethodInfo method)
  {
    IMethodSymbol methodSymbol = method.Symbol;

    MethodBuilder methodBuilder = _typeBuilder.DefineMethod(method.Name, method.Attributes, method.CallingConvention);

    IReadOnlyDictionary<string, Type> genericTypes = CreateGenericTypeDictionary(methodBuilder, method);

    methodBuilder.SetReturnType(_context.ResolveType(method.ReturnType, genericTypes));
    methodBuilder.SetParameters(method.GetParameters().Select(x => _context.ResolveType(x.ParameterType, genericTypes)).ToArray());

    if (method.Symbol.ExplicitInterfaceImplementations.Length != 0)
    {
      throw new NotSupportedException("Explicit implementation of interfaces is not supported.");
    }

    switch (methodSymbol.MethodKind)
    {
      case MethodKind.PropertyGet:
        _getters.Add((IPropertySymbol)methodSymbol.AssociatedSymbol!, methodBuilder);
        break;
      case MethodKind.PropertySet:
        _setters.Add((IPropertySymbol)methodSymbol.AssociatedSymbol!, methodBuilder);
        break;
      default:
        break;
    }

    switch (methodSymbol.MethodKind)
    {
      case MethodKind.PropertyGet when _backingFields.TryGetValue((IPropertySymbol)methodSymbol.AssociatedSymbol!, out FieldBuilder backingField):
        BuildAutoGetter(methodBuilder, backingField);
        break;
      case MethodKind.PropertySet when _backingFields.TryGetValue((IPropertySymbol)methodSymbol.AssociatedSymbol!, out FieldBuilder backingField):
        BuildAutoSetter(methodBuilder, backingField);
        break;
      default:
        BuildOrdinaryMethod(methodBuilder, methodSymbol);
        break;
    }
  }

  private void BuildOrdinaryMethod(MethodBuilder methodBuilder, IMethodSymbol methodSymbol)
  {
    ILGenerator il = methodBuilder.GetILGenerator();

    il.Emit(OpCodes.Ret);
  }

  private void BuildAutoGetter(MethodBuilder methodBuilder, FieldInfo backingField)
  {
    ILGenerator il = methodBuilder.GetILGenerator();

    il.Emit(OpCodes.Ldarg_0);
    il.Emit(OpCodes.Ldfld, backingField);
    il.Emit(OpCodes.Ret);
  }

  private void BuildAutoSetter(MethodBuilder methodBuilder, FieldInfo backingField)
  {
    ILGenerator il = methodBuilder.GetILGenerator();

    il.Emit(OpCodes.Ldarg_0);
    il.Emit(OpCodes.Ldarg_1);
    il.Emit(OpCodes.Stfld, backingField);
    il.Emit(OpCodes.Ret);
  }

  private static IReadOnlyDictionary<string, Type> CreateGenericTypeDictionary(MethodBuilder methodBuilder, SymbolMethodInfo method)
  {
    Dictionary<string, Type> genericTypes = new Dictionary<string, Type>();
    if (!method.IsGenericMethod)
      return genericTypes;

    SymbolType[] genericArguments = method.GetGenericArguments();
    GenericTypeParameterBuilder[] genericTypeParameterBuilders = methodBuilder.DefineGenericParameters(genericArguments.Select(x => x.Name).ToArray());

    for (int i = 0; i < genericArguments.Length; i++)
    {
      GenericTypeParameterBuilder genericTypeParameterBuilder = genericTypeParameterBuilders[i];
      SymbolType genericArgument = genericArguments[i];
      genericTypes.Add(genericArgument.Name, genericTypeParameterBuilder);

      ProxyGenericTypeParameterBuilder.BuildGenericTypeParameter(genericTypeParameterBuilder, genericArgument);
    }

    return genericTypes;
  }
}