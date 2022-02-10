using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit;

public interface IGeneratorRuntime
{
  Assembly CompilationAssembly { get; }

  T CreateInstance<T>(Type type, object?[] arguments);

  Assembly CreateAssemblyDelegator(IAssemblySymbol symbol);
  ConstructorInfo CreateConstructorInfoDelegator(IMethodSymbol symbol);
  FieldInfo CreateFieldInfoDelegator(IFieldSymbol symbol);
  MethodInfo CreateMethodInfoDelegator(IMethodSymbol symbol);
  Module CreateModuleDelegator(IModuleSymbol symbol);
  PropertyInfo CreatePropertyInfoDelegator(IPropertySymbol symbol);
  Type CreateTypeDelegator(ITypeSymbol symbol);
}