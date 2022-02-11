using GeneratorKit.Interpret;
using GeneratorKit.Proxy;
using GeneratorKit.Reflection.Context;
using Microsoft.CodeAnalysis;
using System;
using System.Threading;

namespace GeneratorKit.TestHelpers;

internal class FakeDependencyFactory : IDependencyFactory
{
  private readonly FakeFrameProvider _frameProvider;
  private readonly Compilation _compilation;
  private Interpreter? _interpreter;

  public FakeDependencyFactory(FakeFrameProvider frameProvider, Compilation compilation)
  {
    _frameProvider = frameProvider;
    _compilation = compilation;
  }

  public void CreateDependencies(IReflectionRuntime runtime, out IGeneratorContext context, out IActivator activator, out IProxyManager proxyManager, out IInterpreter interpreter)
  {
    OperationManager operationManager = new OperationManager(_compilation, CancellationToken.None);
    GeneratorContext generatorContext = new DefaultGeneratorContext(runtime);
    context = generatorContext;
    _interpreter = new Interpreter(runtime, generatorContext, operationManager, _frameProvider);
    interpreter = _interpreter;
    activator = new Activator(_interpreter);
    proxyManager = new ProxyManager();
  }

  public Interpreter Interpreter => _interpreter ?? throw new InvalidOperationException("GetDependencies was not called");
}
