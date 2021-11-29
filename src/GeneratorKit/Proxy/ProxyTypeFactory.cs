using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GeneratorKit.Proxy;

internal class ProxyTypeFactory : IProxyTypeFactory
{
  private readonly ModuleBuilder _moduleBuilder;

  public ProxyTypeFactory(string assemblyName)
  {
    AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndCollect);
    _moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);
  }

  public Type? BuildProxyType(GeneratorRuntime runtime, SymbolType type)
  {
    SemanticModel semanticModel = runtime.Compilation.GetSemanticModel(type.Symbol.DeclaringSyntaxReferences[0].SyntaxTree);

    if (semanticModel.GetDiagnostics(cancellationToken: runtime.CancellationToken).Any(x => x.Severity == DiagnosticSeverity.Error))
    {
      return null;
    }

    return ProxyTypeBuilder.BuildType(runtime, semanticModel, _moduleBuilder, type);
  }
}