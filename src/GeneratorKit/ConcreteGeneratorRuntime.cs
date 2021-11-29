#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GeneratorKit;

internal class ConcreteGeneratorRuntime : GeneratorRuntime
{
  private readonly IProxyTypeFactory _proxyTypeFactory;
  private readonly SymbolAssembly _compilationAssembly;
  private readonly Dictionary<ITypeSymbol, Type> _typeMap;

  public ConcreteGeneratorRuntime(Compilation compilation, IProxyTypeFactory proxyTypeFactory, CancellationToken cancellationToken)
    : base(compilation)
  {
    _proxyTypeFactory = proxyTypeFactory;
    CancellationToken = cancellationToken;

    _compilationAssembly = new SymbolAssembly(this, compilation.Assembly, compilation.GetEntryPoint(cancellationToken));
    _typeMap = new Dictionary<ITypeSymbol, Type>(SymbolEqualityComparer.Default);
  }

  public override SymbolAssembly CompilationAssembly => _compilationAssembly;

  public override CancellationToken CancellationToken { get; }

  public override Type? GetRuntimeType(SymbolType type)
  {
    if (type.HasElementType || type.IsGenericType)
    {
      throw new NotSupportedException("To be supported.");
    }

    if (!_typeMap.TryGetValue(type.Symbol, out Type? runtimeType))
    {
      runtimeType = ResolveRuntimeType(type);
      if (runtimeType is not null)
      {
        _typeMap.Add(type.Symbol, runtimeType);
      }
    }
    return runtimeType;
  }

  public override ITypeSymbol? GetTypeSymbol(Type type)
  {
    if (type.HasElementType || type.IsGenericType)
    {
      throw new NotSupportedException("To be supported.");
    }

    return Compilation.GetTypeByMetadataName(type.FullName);
  }

  private Type? ResolveRuntimeType(SymbolType type)
  {
    return null;
  }
}