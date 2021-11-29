using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit;

public interface IGeneratorRuntime
{
  Assembly CreateAssemblyDelegator(IAssemblySymbol symbol);
  ConstructorInfo CreateConstructorInfoDelegator(IMethodSymbol symbol);
  EventInfo CreateEventInfoDelegator(IEventSymbol symbol);
  FieldInfo CreateFieldInfoDelegator(IFieldSymbol symbol);
  MethodInfo CreateMethodInfoDelegator(IMethodSymbol symbol);
  Module CreateModuleDelegator(IModuleSymbol symbol);
  PropertyInfo CreatePropertyInfoDelegator(IPropertySymbol symbol);
  Type CreateTypeDelegator(ITypeSymbol symbol);
}