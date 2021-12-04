using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Threading;

namespace GeneratorKit;

internal class ConcreteGeneratorRuntime : GeneratorRuntime
{
  private readonly IProxyTypeFactory _typeFactory;
  private readonly SymbolAssembly _compilationAssembly;

  public ConcreteGeneratorRuntime(Compilation compilation, IProxyTypeFactory typeFactory, CancellationToken cancellationToken)
    : base(compilation)
  {
    _typeFactory = typeFactory;
    CancellationToken = cancellationToken;

    _compilationAssembly = new SymbolAssembly(this, compilation.Assembly, compilation.GetEntryPoint(cancellationToken));
  }

  public override SymbolAssembly CompilationAssembly => _compilationAssembly;

  public override CancellationToken CancellationToken { get; }

  public override Type? GetRuntimeType(SymbolType type)
  {
    if (type.Symbol.ContainingAssembly is not ISourceAssemblySymbol)
      return Type.GetType(type.AssemblyQualifiedName);

    if (type.HasElementType || type.IsConstructedGenericType)
    {
      throw new NotSupportedException("To be supported.");
    }

    return _typeFactory.CreateProxyType(this, type);
  }

  public override ITypeSymbol? GetTypeSymbol(Type type)
  {
    if (type is SymbolType symbolType)
      return symbolType.Symbol;

    if (type.HasElementType || type.IsGenericType)
    {
      throw new NotSupportedException("To be supported.");
    }

    return Compilation.GetTypeByMetadataName(type.FullName);
  }
}