using GeneratorKit.Interpret;
using GeneratorKit.Proxy;

namespace GeneratorKit;

internal interface IDependencyFactory
{
  void CreateDependencies(GeneratorContext context, out IActivator activator, out IProxyManager proxyManager, out IInterpreter interpreter);
}