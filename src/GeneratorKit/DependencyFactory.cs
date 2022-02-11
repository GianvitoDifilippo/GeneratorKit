using GeneratorKit.Interpret;
using GeneratorKit.Interpret.Frame;
using GeneratorKit.Proxy;
using GeneratorKit.Reflection.Context;
using Microsoft.CodeAnalysis;
using System.Threading;

namespace GeneratorKit;

internal class DependencyFactory : IDependencyFactory
{
  private readonly Compilation _compilation;
  private readonly IProxyManager _proxyManager;
  private readonly CancellationToken _cancellationToken;

  public DependencyFactory(Compilation compilation, IProxyManager proxyManager, CancellationToken cancellationToken)
  {
    _compilation = compilation;
    _proxyManager = proxyManager;
    _cancellationToken = cancellationToken;
  }

  public void CreateDependencies(IReflectionRuntime runtime, out IGeneratorContext context, out IActivator activator, out IProxyManager proxyManager, out IInterpreter interpreter)
  {
    FrameProvider frameProvider = new FrameProvider();
    OperationManager operationManager = new OperationManager(_compilation, _cancellationToken);

    GeneratorContext generatorContext = new DefaultGeneratorContext(runtime);
    context = generatorContext;
    interpreter = new Interpreter(runtime, generatorContext, operationManager, frameProvider);
    activator = new Activator(interpreter);
    proxyManager = _proxyManager;
  }
}
