using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;

namespace GeneratorKit;

public interface IGeneratorRuntime
{
  SymbolAssembly CreateAssemblyDelegator(IAssemblySymbol symbol);
  SymbolConstructorInfo CreateConstructorInfoDelegator(IMethodSymbol symbol);
  SymbolEventInfo CreateEventInfoDelegator(IEventSymbol symbol);
  SymbolFieldInfo CreateFieldInfoDelegator(IFieldSymbol symbol);
  SymbolMethodInfo CreateMethodInfoDelegator(IMethodSymbol symbol);
  SymbolModule CreateModuleDelegator(IModuleSymbol symbol);
  SymbolParameterInfo CreateParameterInfoDelegator(IParameterSymbol symbol);
  SymbolPropertyInfo CreatePropertyInfoDelegator(IPropertySymbol symbol);
  SymbolType CreateTypeDelegator(ITypeSymbol symbol);
}