#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Emit;
using GeneratorKit.Exceptions;
using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GeneratorKit;

internal class ProxyTypeFactory : IProxyTypeFactory
{
  private readonly ModuleBuilder _moduleBuilder;
  private readonly Dictionary<ITypeSymbol, Type> _typeMap;

  public ProxyTypeFactory(string assemblyName)
  {
    AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndCollect);
    _moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);
    
    _typeMap = new Dictionary<ITypeSymbol, Type>(SymbolEqualityComparer.Default);
  }

  public Type CreateProxyType(GeneratorRuntime runtime, SymbolType type)
  {
    if (_typeMap.TryGetValue(type.Symbol, out Type? result))
      return result;

    SemanticModel semanticModel = runtime.Compilation.GetSemanticModel(type.Symbol.DeclaringSyntaxReferences[0].SyntaxTree);
    ImmutableArray<Diagnostic> diagnostics = semanticModel.GetDiagnostics(cancellationToken: runtime.CancellationToken);

    IEnumerable<Diagnostic> errors = diagnostics.Where(x => x.Severity is DiagnosticSeverity.Error);
    if (errors.Any())
      throw new InvalidCodeException(errors.ToArray());

    result = ProxyTypeBuilder.BuildType(runtime, _moduleBuilder, type);
    _typeMap.Add(type.Symbol, result);

    return result;
  }
}