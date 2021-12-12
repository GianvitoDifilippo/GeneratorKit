#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Exceptions;
using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace GeneratorKit.Emit;

internal class BuildMethodsStage : BuildOperationsStage
{
  private readonly IReadOnlyDictionary<IPropertySymbol, FieldBuilder> _backingFields;
  private readonly IReadOnlyDictionary<ITypeSymbol, Type> _interfaceTypes;
  private readonly Dictionary<IPropertySymbol, MethodBuilder> _getters;
  private readonly Dictionary<IPropertySymbol, MethodBuilder> _setters;

  public BuildMethodsStage(IBuildContext context, IReadOnlyDictionary<IPropertySymbol, FieldBuilder> backingFields, IReadOnlyDictionary<ITypeSymbol, Type> interfaceTypes)
    : base(context)
  {
    _context = context;
    _backingFields = backingFields;
    _interfaceTypes = interfaceTypes;
    _getters = new Dictionary<IPropertySymbol, MethodBuilder>(SymbolEqualityComparer.Default);
    _setters = new Dictionary<IPropertySymbol, MethodBuilder>(SymbolEqualityComparer.Default);
  }

  public IReadOnlyDictionary<IPropertySymbol, MethodBuilder> Getters => _getters;

  public IReadOnlyDictionary<IPropertySymbol, MethodBuilder> Setters => _setters;

  public void BuildMethod(SymbolMethodInfo method)
  {
    IMethodSymbol methodSymbol = method.Symbol;

    MethodBuilder methodBuilder = _context.TypeBuilder.DefineMethod(method.Name, method.Attributes, method.CallingConvention);

    IReadOnlyDictionary<string, Type>? genericParameters = CreateGenericParameterDictionary(methodBuilder, method);

    Type returnType = _context.ResolveType(method.ReturnType, genericParameters);
    Type[] parameterTypes = method.GetParameters().Map(x => _context.ResolveType(x.ParameterType, genericParameters));
    methodBuilder.SetReturnType(returnType);
    methodBuilder.SetParameters(parameterTypes);

    foreach (IMethodSymbol explicitMethodSymbol in method.Symbol.ExplicitInterfaceImplementations)
    {
      SymbolMethodInfo symbolMethodDefinition = _context.Runtime.CreateMethodInfoDelegator(explicitMethodSymbol.OriginalDefinition);
      Type interfaceType = _interfaceTypes[explicitMethodSymbol.ContainingType];
      
      MethodInfo methodDefinition = MemberResolver.ResolveMethod(interfaceType.GetGenericTypeDefinition(), symbolMethodDefinition);
      MethodInfo explicitMethod = TypeBuilder.GetMethod(interfaceType, methodDefinition);

      _context.TypeBuilder.DefineMethodOverride(methodBuilder, explicitMethod);
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
        BuildOrdinaryMethod(methodBuilder, methodSymbol, returnType, parameterTypes);
        break;
    }
  }

  private void BuildOrdinaryMethod(MethodBuilder methodBuilder, IMethodSymbol methodSymbol, Type returnType, Type[] parameterTypes)
  {
    ILGenerator il = methodBuilder.GetILGenerator();

    IOperation operation = _context.GetOperation(methodSymbol) ?? throw new OperationResolutionException(methodSymbol);
    BuildMethodBody(il, returnType, parameterTypes, operation, methodSymbol);

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

  private static IReadOnlyDictionary<string, Type>? CreateGenericParameterDictionary(MethodBuilder methodBuilder, SymbolMethodInfo method)
  {
    if (!method.IsGenericMethod)
      return null;

    Dictionary<string, Type> genericParameters = new Dictionary<string, Type>();

    SymbolType[] genericArguments = method.GetGenericArguments();
    GenericTypeParameterBuilder[] genericTypeParameterBuilders = methodBuilder.DefineGenericParameters(genericArguments.Map(x => x.Name));

    for (int i = 0; i < genericArguments.Length; i++)
    {
      GenericTypeParameterBuilder genericTypeParameterBuilder = genericTypeParameterBuilders[i];
      SymbolType genericArgument = genericArguments[i];
      genericParameters.Add(genericArgument.Name, genericTypeParameterBuilder);

      ProxyGenericTypeParameterBuilder.BuildGenericTypeParameter(genericTypeParameterBuilder, genericArgument);
    }

    return genericParameters;
  }
}