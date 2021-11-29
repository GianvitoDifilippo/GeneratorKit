using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System.Reflection.Emit;
using System;
using System.Linq;

namespace GeneratorKit.Proxy;

internal class ProxyTypeBuilder
{
  private readonly GeneratorRuntime _runtime;
  private readonly SemanticModel _semanticModel;
  private readonly TypeBuilder _typeBuilder;
  private readonly SymbolType _type;

  private ProxyTypeBuilder(GeneratorRuntime runtime, SemanticModel semanticModel, TypeBuilder typeBuilder, SymbolType type)
  {
    _runtime = runtime;
    _semanticModel = semanticModel;
    _typeBuilder = typeBuilder;
    _type = type;
  }

  public static Type? BuildType(GeneratorRuntime runtime, SemanticModel semanticModel, ModuleBuilder moduleBuilder, SymbolType type)
  {
    if (type.BaseType is null)
    {
      throw new InvalidOperationException("Cannot build System.Object.");
    }

    string name = type.Name;
    Type parent = GetRealType(type.BaseType);
    Type[] interfaces = type.GetInterfaces().Select(GetRealType).ToArray();
    TypeBuilder typeBuilder = moduleBuilder.DefineType(name, type.Attributes, parent, interfaces);

    ProxyTypeBuilder builder = new ProxyTypeBuilder(runtime, semanticModel, typeBuilder, type);
    // builder.BuildFields();
    // builder.BuildMethods();
    // builder.BuildConstructors();

    return typeBuilder.CreateTypeInfo();
  }

  private static Type GetRealType(Type type)
  {
    Type underlyingType = type.UnderlyingSystemType;
    return underlyingType is not SymbolType
      ? underlyingType
      : throw new InvalidOperationException($"Cannot resolve the real type of {type.AssemblyQualifiedName}");
  }
}