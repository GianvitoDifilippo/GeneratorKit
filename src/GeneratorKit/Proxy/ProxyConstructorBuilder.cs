using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using GeneratorKit.Reflection;
using System.Linq;
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GeneratorKit.Proxy;

internal class ProxyConstructorBuilder
{
  private const MethodAttributes s_defaultConstructorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
  private const BindingFlags s_allDeclared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

  private readonly IReadOnlyCollection<ConstructorData> _constructors;
  private readonly ConstructorBuilder? _defaultCtorBuilder;

  private ProxyConstructorBuilder(IReadOnlyCollection<ConstructorData> constructors)
  {
    _constructors = constructors;
  }

  private ProxyConstructorBuilder(ConstructorBuilder? defaultCtorBuilder)
  {
    _constructors = Array.Empty<ConstructorData>();
    _defaultCtorBuilder = defaultCtorBuilder;
  }

  public void Build(IReadOnlyDictionary<FieldBuilder, ExpressionSyntax> initializers)
  {
    foreach ((ConstructorBuilder builder, IMethodSymbol symbol) in _constructors)
    {
      BuildConstructor(builder, symbol, initializers);
    }

    if (_defaultCtorBuilder is not null)
    {
      BuildDefaultConstructor(_defaultCtorBuilder, initializers);
    }
  }

  private void BuildConstructor(ConstructorBuilder builder, IMethodSymbol symbol, IReadOnlyDictionary<FieldBuilder, ExpressionSyntax> initializers)
  {
    ILGenerator il = builder.GetILGenerator();

    il.Emit(OpCodes.Ret);
  }

  private void BuildDefaultConstructor(ConstructorBuilder builder, IReadOnlyDictionary<FieldBuilder, ExpressionSyntax> initializers)
  {
    ILGenerator il = builder.GetILGenerator();

    il.Emit(OpCodes.Ret);
  }

  public static ProxyConstructorBuilder Create(TypeBuilder typeBuilder, SymbolType type)
  {
    SymbolConstructorInfo[] constructors = type.GetConstructors(s_allDeclared);

    if (constructors.Length == 0)
    {
      return new ProxyConstructorBuilder(typeBuilder.DefineDefaultConstructor(s_defaultConstructorAttributes));
    }

    List<ConstructorData> constructorList = new List<ConstructorData>(constructors.Length);

    foreach (SymbolConstructorInfo constructor in constructors)
    {
      IMethodSymbol constructorSymbol = constructor.Symbol;

      ConstructorBuilder constructorBuilder = constructorSymbol.MethodKind is MethodKind.StaticConstructor
        ? typeBuilder.DefineTypeInitializer()
        : typeBuilder.DefineConstructor(
            attributes: constructor.Attributes,
            callingConvention: constructor.CallingConvention,
            parameterTypes: constructor.GetParameters().Select(x => x.ParameterType.RuntimeType).ToArray());

      constructorList.Add(new ConstructorData(constructorBuilder, constructorSymbol));
    }

    return new ProxyConstructorBuilder(constructorList);
  }

  private readonly record struct ConstructorData(ConstructorBuilder Builder, IMethodSymbol Symbol);
}