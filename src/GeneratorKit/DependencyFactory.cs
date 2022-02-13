using GeneratorKit.Interpret;
using GeneratorKit.Interpret.Frame;
using GeneratorKit.Proxy;
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

  public void CreateDependencies(GeneratorContext context, out IActivator activator, out IProxyManager proxyManager, out IInterpreter interpreter)
  {
    FrameProvider frameProvider = new FrameProvider();
    OperationManager operationManager = new OperationManager(_compilation, _cancellationToken);

    interpreter = new Interpreter(context, operationManager, frameProvider);
    activator = new Activator(interpreter);
    proxyManager = _proxyManager;
  }
}
