using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GeneratorKit.Proxy;

internal class ProxyTypeBuilder : IBuilderContext
{
  private const BindingFlags s_allDeclared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
  
  private readonly GeneratorRuntime _runtime;
  private readonly TypeBuilder _typeBuilder;
  private readonly SymbolType _type;
  private readonly IReadOnlyDictionary<string, Type> _genericTypes;
  private Type? _baseType;
  private IReadOnlyDictionary<IPropertySymbol, FieldBuilder>? _backingFields;
  private IReadOnlyCollection<InitializerData>? _initializers;
  private IReadOnlyDictionary<IPropertySymbol, MethodBuilder>? _getters;
  private IReadOnlyDictionary<IPropertySymbol, MethodBuilder>? _setters;

  private ProxyTypeBuilder(GeneratorRuntime runtime, TypeBuilder typeBuilder, SymbolType type, IReadOnlyDictionary<string, Type> genericTypes)
  {
    _runtime = runtime;
    _typeBuilder = typeBuilder;
    _type = type;
    _genericTypes = genericTypes;
  }

  private void SetParent()
  {
    if (_type.BaseType is null)
      throw new InvalidOperationException("Cannot build System.Object.");

    _baseType = ResolveType(_type.BaseType);
    _typeBuilder.SetParent(_baseType);
  }

  private void SetInterfaces()
  {
    Type[] interfaceTypes = _type.GetInterfaces().Select(x => ResolveType(x)).ToArray();
    foreach (Type interfaceType in interfaceTypes!)
    {
      _typeBuilder.AddInterfaceImplementation(interfaceType);
    }
  }

  private void BuildFields()
  {
    ProxyFieldBuilder builder = new ProxyFieldBuilder(this);

    foreach (SymbolFieldInfo field in _type.GetFields(s_allDeclared))
    {
      builder.BuildField(field);
    }

    _backingFields = builder.BackingFields;
    _initializers = builder.Initializers;
  }

  private void BuildMethods()
  {
    if (_backingFields is null)
      throw new InvalidOperationException("Backing fields were not initialized.");

    ProxyMethodBuilder builder = new ProxyMethodBuilder(this, _backingFields);

    foreach (SymbolMethodInfo method in _type.GetMethods(s_allDeclared))
    {
      builder.BuildMethod(method);
    }

    _getters = builder.Getters;
    _setters = builder.Setters;
  }

  private void BuildProperties()
  {
    if (_getters is null || _setters is null)
      throw new InvalidOperationException("Getters and setters were not initialized.");

    ProxyPropertyBuilder builder = new ProxyPropertyBuilder(this, _getters, _setters);

    foreach (SymbolPropertyInfo property in _type.GetProperties(s_allDeclared))
    {
      builder.BuildProperty(property);
    }
  }

  private void BuildConstructors()
  {
    if (_initializers is null)
      throw new InvalidOperationException("Initializers were not initialized");
    if (_baseType is null)
      throw new InvalidOperationException("Base type was not initialized");

    ProxyConstructorBuilder builder = new ProxyConstructorBuilder(this, _initializers, _baseType);

    foreach (SymbolConstructorInfo constructor in _type.GetConstructors(s_allDeclared))
    {
      builder.BuildConstructor(constructor);
    }
  }

  public Type ResolveType(SymbolType type, IReadOnlyDictionary<string, Type>? genericTypes = null)
  {
    if (type.IsGenericType && !type.IsGenericTypeDefinition)
    {
      return ResolveGenericType(type, genericTypes);
    }
    if (type.IsGenericParameter)
    {
      return genericTypes is not null && genericTypes.TryGetValue(type.Name, out Type genericType)
        ? genericType
        : _genericTypes[type.Name];
    }

    return type.RuntimeType;
  }

  private Type ResolveGenericType(SymbolType type, IReadOnlyDictionary<string, Type>? genericTypes)
  {
    Type[] typeArguments = type.GenericTypeArguments.Select(x => ResolveType(x, genericTypes)).ToArray();
    Type typeDefinition = ResolveType(type.GetGenericTypeDefinition(), genericTypes);
    return typeDefinition.MakeGenericType(typeArguments);
  }

  public static Type? BuildType(GeneratorRuntime runtime, ModuleBuilder moduleBuilder, SymbolType type)
  {
    TypeBuilder typeBuilder = moduleBuilder.DefineType(type.Name, type.Attributes);
    IReadOnlyDictionary<string, Type> genericTypes = CreateGenericTypeDictionary(typeBuilder, type);

    ProxyTypeBuilder builder = new ProxyTypeBuilder(runtime, typeBuilder, type, genericTypes);
    builder.SetParent();
    builder.SetInterfaces();
    builder.BuildFields();
    builder.BuildMethods();
    builder.BuildProperties();
    builder.BuildConstructors();

    return typeBuilder.CreateTypeInfo();
  }

  private static IReadOnlyDictionary<string, Type> CreateGenericTypeDictionary(TypeBuilder typeBuilder, SymbolType type)
  {
    Dictionary<string, Type> genericTypes = new Dictionary<string, Type>();
    if (!type.IsGenericType)
      return genericTypes;

    SymbolType[] genericArguments = type.GetGenericArguments();
    GenericTypeParameterBuilder[] genericTypeParameterBuilders = typeBuilder.DefineGenericParameters(genericArguments.Select(x => x.Name).ToArray());

    for (int i = 0; i < genericArguments.Length; i++)
    {
      GenericTypeParameterBuilder genericTypeParameterBuilder = genericTypeParameterBuilders[i];
      SymbolType genericArgument = genericArguments[i];
      genericTypes.Add(genericArgument.Name, genericTypeParameterBuilder);

      ProxyGenericTypeParameterBuilder.BuildGenericTypeParameter(genericTypeParameterBuilder, genericArgument);
    }

    return genericTypes;
  }

  TypeBuilder IBuilderContext.TypeBuilder => _typeBuilder;

  SymbolType IBuilderContext.Type => _type;

  GeneratorRuntime IBuilderContext.Runtime => _runtime;
}