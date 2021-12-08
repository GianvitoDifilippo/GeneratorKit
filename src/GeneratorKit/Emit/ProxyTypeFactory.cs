#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Exceptions;
using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace GeneratorKit.Emit;

internal class ProxyTypeFactory : IProxyTypeFactory
{
  private const BindingFlags s_allDeclared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

  private readonly ModuleBuilder _moduleBuilder;

  public ProxyTypeFactory(string assemblyName)
  {
    AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndCollect);
    _moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);
  }

  public Type CreateProxyType(GeneratorRuntime runtime, SymbolType type)
  {
    SemanticModel[] semanticModels = type.Symbol.DeclaringSyntaxReferences.Map(x => runtime.Compilation.GetSemanticModel(x.SyntaxTree));
    IReadOnlyCollection<Diagnostic> errors = GetErrors(semanticModels, runtime.CancellationToken);

    TypeBuilder typeBuilder = _moduleBuilder.DefineType(type.Name, type.Attributes);
    IReadOnlyDictionary<string, Type>? genericParameters = CreateGenericParameterDictionary(typeBuilder, type);

    BuildContext context = new BuildContext(runtime, typeBuilder, semanticModels, genericParameters);

    BuildTypeStage typeStage = new BuildTypeStage(context, type);

    BuildFieldsStage fieldsStage = new BuildFieldsStage(context);
    BuildFields(fieldsStage, type);

    BuildMethodsStage methodsStage = new BuildMethodsStage(context, fieldsStage.BackingFields, typeStage.InterfaceTypes);
    BuildMethods(methodsStage, type);

    BuildPropertiesStage propertiesStage = new BuildPropertiesStage(context, methodsStage.Getters, methodsStage.Setters);
    BuildProperties(propertiesStage, type);

    BuildConstructorsStage constructorsStage = new BuildConstructorsStage(context, typeStage.BaseType, fieldsStage.InstanceInitializers, fieldsStage.StaticInitializers);
    BuildConstructors(constructorsStage, type);

    return typeBuilder.CreateTypeInfo() ?? throw new NullEmitException(typeBuilder);
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

    Dictionary<string, Type> genericParameters = new Dictionary<string, Type>();

    SymbolType[] genericArguments = type.GetGenericArguments();
    GenericTypeParameterBuilder[] genericTypeParameterBuilders = typeBuilder.DefineGenericParameters(genericArguments.Map(x => x.Name));

    for (int i = 0; i < genericArguments.Length; i++)
    {
      GenericTypeParameterBuilder genericTypeParameterBuilder = genericTypeParameterBuilders[i];
      SymbolType genericArgument = genericArguments[i];
      genericParameters.Add(genericArgument.Name, genericTypeParameterBuilder);

      ProxyGenericTypeParameterBuilder.BuildGenericTypeParameter(genericTypeParameterBuilder, genericArgument);
    }

    return genericParameters;
  }

  private static IReadOnlyCollection<Diagnostic> GetErrors(IEnumerable<SemanticModel> semanticModels, CancellationToken cancellationToken)
  {
    List<Diagnostic> diagnostics = new List<Diagnostic>();
    foreach (SemanticModel semanticModel in semanticModels)
    {
      foreach (Diagnostic diagnostic in semanticModel.GetDiagnostics(cancellationToken: cancellationToken))
      {
        diagnostics.Add(diagnostic);
      }
    }
    return diagnostics;
  }
}