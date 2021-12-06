using GeneratorKit.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GeneratorKit.Emit;

internal class ProxyTypeBuilder
{
  private const BindingFlags s_allDeclared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

  public static Type? BuildType(GeneratorRuntime runtime, ModuleBuilder moduleBuilder, SymbolType type)
  {
    TypeBuilder typeBuilder = moduleBuilder.DefineType(type.Name, type.Attributes);
    IReadOnlyDictionary<string, Type>? genericParameters = CreateGenericParameterDictionary(typeBuilder, type);

    BuildContext context = new BuildContext(runtime, typeBuilder, genericParameters);

    BuildTypeStage typeStage = new BuildTypeStage(context, type);
    
    BuildFieldsStage fieldsStage = new BuildFieldsStage(context);
    BuildFields(fieldsStage, type);

    BuildMethodsStage methodsStage = new BuildMethodsStage(context, fieldsStage.BackingFields, typeStage.InterfaceTypes);
    BuildMethods(methodsStage, type);

    BuildPropertiesStage propertiesStage = new BuildPropertiesStage(context, methodsStage.Getters, methodsStage.Setters);
    BuildProperties(propertiesStage, type);

    BuildConstructorsStage constructorsStage = new BuildConstructorsStage(context, typeStage.BaseType, fieldsStage.InstanceInitializers, fieldsStage.StaticInitializers);
    BuildConstructors(constructorsStage, type);

    return typeBuilder.CreateTypeInfo();
  }

  private static void BuildFields(BuildFieldsStage stage, SymbolType type)
  {
    foreach (SymbolFieldInfo field in type.GetFields(s_allDeclared))
    {
      stage.BuildField(field);
    }
  }

  private static void BuildMethods(BuildMethodsStage stage, SymbolType type)
  {
    foreach (SymbolMethodInfo method in type.GetMethods(s_allDeclared))
    {
      stage.BuildMethod(method);
    }
  }

  private static void BuildProperties(BuildPropertiesStage stage, SymbolType type)
  {
    foreach (SymbolPropertyInfo property in type.GetProperties(s_allDeclared))
    {
      stage.BuildProperty(property);
    }
  }

  private static void BuildConstructors(BuildConstructorsStage stage, SymbolType type)
  {
    foreach (SymbolConstructorInfo constructor in type.GetConstructors(s_allDeclared))
    {
      stage.BuildConstructor(constructor);
    }
  }

  private static IReadOnlyDictionary<string, Type>? CreateGenericParameterDictionary(TypeBuilder typeBuilder, SymbolType type)
  {
    if (!type.IsGenericType)
      return null;

    Dictionary<string, Type> genericTypes = new Dictionary<string, Type>();

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
}