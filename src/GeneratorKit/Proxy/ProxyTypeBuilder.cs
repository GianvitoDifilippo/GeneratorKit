using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System.Reflection.Emit;
using System;

namespace GeneratorKit.Proxy;

internal class ProxyTypeBuilder
{
  private readonly GeneratorRuntime _runtime;
  private readonly SemanticModel _semanticModel;
  private readonly TypeBuilder _typeBuilder;
  private readonly SymbolType _type;

  private ProxyFieldBuilder? _fieldBuilder;
  private ProxyMethodBuilder? _methodBuilder;
  private ProxyPropertyBuilder? _propertyBuilder;
  private ProxyConstructorBuilder? _constructorBuilder;

  private ProxyTypeBuilder(GeneratorRuntime runtime, SemanticModel semanticModel, TypeBuilder typeBuilder, SymbolType type)
  {
    _runtime = runtime;
    _semanticModel = semanticModel;
    _typeBuilder = typeBuilder;
    _type = type;
  }

  public void BuildFields()
  {
    _fieldBuilder = ProxyFieldBuilder.Create(_typeBuilder, _type);
  }

  public void BuildMethods()
  {
    if (_fieldBuilder is null)
    {
      throw new InvalidOperationException("Fields must be initialized before methods.");
    }
    _methodBuilder = ProxyMethodBuilder.Create(_typeBuilder, _type);
    _methodBuilder.Build(_fieldBuilder.BackingFields);
  }

  public void BuildProperties()
  {
    if ( _methodBuilder is null)
    {
      throw new InvalidOperationException("Methods must be initialized before properties.");
    }
    _propertyBuilder = ProxyPropertyBuilder.Create(_typeBuilder, _type);
    _propertyBuilder.Build(_methodBuilder.Getters, _methodBuilder.Setters);
  }

  public void BuildConstructors()
  {
    if (_fieldBuilder is null)
    {
      throw new InvalidOperationException("Fields must be initialized before constructors.");
    }
    _constructorBuilder = ProxyConstructorBuilder.Create(_typeBuilder, _type);
    _constructorBuilder.Build(_fieldBuilder.Initializers);
  }

  public static Type? BuildType(GeneratorRuntime runtime, SemanticModel semanticModel, ModuleBuilder moduleBuilder, SymbolType type)
  {
    if (type.BaseType is null)
    {
      throw new InvalidOperationException("Cannot build System.Object.");
    }

    string name = type.Name;
    if (!TryGetRealType(type.BaseType, out Type? parent))
    {
      return null;
    }
    if (!TryGetRealTypes(type.GetInterfaces(), out Type[]? interfaces))
    {
      return null;
    }
    TypeBuilder typeBuilder = moduleBuilder.DefineType(name, type.Attributes, parent, interfaces);

    ProxyTypeBuilder builder = new ProxyTypeBuilder(runtime, semanticModel, typeBuilder, type);
    builder.BuildFields();
    builder.BuildMethods();
    builder.BuildProperties();
    builder.BuildConstructors();

    return typeBuilder.CreateTypeInfo();
  }

  private static bool TryGetRealType(Type type, out Type? realType)
  {
    Type underlyingType = type.UnderlyingSystemType;
    if (underlyingType is not SymbolType)
    {
      realType = underlyingType;
      return true;
    }
    realType = null;
    return false;
  }

  private static bool TryGetRealTypes(Type[] types, out Type[]? realTypes)
  {
    Type[] result = new Type[types.Length];
    for (int i = 0; i < types.Length; i++)
    {
      if (!TryGetRealType(types[i], out Type? realType))
      {
        realTypes = null;
        return false;
      }
      result[i] = realType!;
    }

    realTypes = result;
    return true;
  }
}