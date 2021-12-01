#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GeneratorKit.Proxy;

internal class ProxyMethodBuilder
{
  private const BindingFlags s_allDeclared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

  private readonly IReadOnlyCollection<MethodData> _methods;

  private ProxyMethodBuilder(IReadOnlyCollection<MethodData> methods, IReadOnlyDictionary<IPropertySymbol, MethodBuilder> getters, IReadOnlyDictionary<IPropertySymbol, MethodBuilder> setters)
  {
    _methods = methods;
    Getters = getters;
    Setters = setters;
  }

  public IReadOnlyDictionary<IPropertySymbol, MethodBuilder> Getters { get; }

  public IReadOnlyDictionary<IPropertySymbol, MethodBuilder> Setters { get; }

  public void Build(IReadOnlyDictionary<IPropertySymbol, FieldBuilder> backingFields)
  {
    foreach ((MethodBuilder builder, IMethodSymbol symbol) in _methods)
    {
      switch (symbol.MethodKind)
      {
        case MethodKind.PropertyGet when backingFields.TryGetValue((IPropertySymbol)symbol.AssociatedSymbol!, out FieldBuilder backingField):
          BuildAutoGetter(builder, backingField);
          break;
        case MethodKind.PropertySet when backingFields.TryGetValue((IPropertySymbol)symbol.AssociatedSymbol!, out FieldBuilder backingField):
          BuildAutoSetter(builder, backingField);
          break;
        default:
          BuildMethod(builder, symbol);
          break;
      }
    }
  }

  private void BuildMethod(MethodBuilder builder, IMethodSymbol methodSymbol)
  {
    ILGenerator il = builder.GetILGenerator();

    il.Emit(OpCodes.Ret);
  }

  private void BuildAutoGetter(MethodBuilder builder, FieldBuilder backingField)
  {
    ILGenerator il = builder.GetILGenerator();

    il.Emit(OpCodes.Ldarg_0);
    il.Emit(OpCodes.Ldfld, backingField);
    il.Emit(OpCodes.Ret);
  }

  private void BuildAutoSetter(MethodBuilder builder, FieldBuilder backingField)
  {
    ILGenerator il = builder.GetILGenerator();

    il.Emit(OpCodes.Ldarg_0);
    il.Emit(OpCodes.Ldarg_1);
    il.Emit(OpCodes.Stfld, backingField);
    il.Emit(OpCodes.Ret);
  }

  public static ProxyMethodBuilder Create(TypeBuilder typeBuilder, SymbolType type)
  {
    SymbolMethodInfo[] methods = type.GetMethods(s_allDeclared);
    List<MethodData> methodList = new List<MethodData>(methods.Length);
    Dictionary<IPropertySymbol, MethodBuilder> getters = new Dictionary<IPropertySymbol, MethodBuilder>(SymbolEqualityComparer.Default);
    Dictionary<IPropertySymbol, MethodBuilder> setters = new Dictionary<IPropertySymbol, MethodBuilder>(SymbolEqualityComparer.Default);

    foreach (SymbolMethodInfo method in methods)
    {
      IMethodSymbol methodSymbol = method.Symbol;

      MethodBuilder methodBuilder = typeBuilder.DefineMethod(
        name: method.Name,
        attributes: method.Attributes,
        callingConvention: method.CallingConvention,
        returnType: method.ReturnType.RuntimeType,
        parameterTypes: method.GetParameters().Select(x => x.ParameterType.RuntimeType).ToArray());

      switch (methodSymbol.MethodKind)
      {
        case MethodKind.PropertyGet:
          getters.Add((IPropertySymbol)methodSymbol.AssociatedSymbol!, methodBuilder);
          break;
        case MethodKind.PropertySet:
          setters.Add((IPropertySymbol)methodSymbol.AssociatedSymbol!, methodBuilder);
          break;
      }

      methodList.Add(new MethodData(methodBuilder, methodSymbol));
    }

    return new ProxyMethodBuilder(methodList, getters, setters);
  }

  private record struct MethodData(MethodBuilder Builder, IMethodSymbol Symbol);
}