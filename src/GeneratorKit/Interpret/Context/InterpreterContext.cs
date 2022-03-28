using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Interpret.Context;

internal abstract class InterpreterContext : GeneratorContext, IInterpreterContext
{
  public Assembly GetAssembly(IAssemblySymbol symbol)
  {
    return CreateAssemblyDelegator(symbol);
  }

  public ConstructorInfo GetConstructorInfo(IMethodSymbol symbol)
  {
    return CreateConstructorInfoDelegator(symbol);
  }

  public FieldInfo GetFieldInfo(IFieldSymbol symbol)
  {
    return CreateFieldInfoDelegator(symbol);
  }

  public MethodInfo GetMethodInfo(IMethodSymbol symbol)
  {
    return CreateMethodInfoDelegator(symbol);
  }

  public Module GetModule(IModuleSymbol symbol)
  {
    return CreateModuleDelegator(symbol);
  }

  public PropertyInfo GetPropertyInfo(IPropertySymbol symbol)
  {
    return CreatePropertyInfoDelegator(symbol);
  }

  public Type GetType(ITypeSymbol symbol)
  {
    return GetContextType(symbol);
  }

  public INamedTypeSymbol? GetTypeSymbol(string fullyQualifiedMetadataName)
  {
    return Compilation.GetTypeByMetadataName(fullyQualifiedMetadataName);
  }
}
