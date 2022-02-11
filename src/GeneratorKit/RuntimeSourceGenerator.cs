using GeneratorKit.Exceptions;
using GeneratorKit.Proxy;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GeneratorKit;

public abstract class RuntimeSourceGenerator : ISourceGenerator
{
  protected virtual IExceptionHandler? ExceptionHandler => null;

  protected virtual IEnumerable<Assembly> ProxiesAssemblies
  {
    get
    {
      yield return GetType().Assembly;
    }
  }

  public void Execute(GeneratorExecutionContext context)
  {
    ProxyManager proxyManager = new ProxyManager();
    DependencyFactory dependencyFactory = new DependencyFactory(context.Compilation, proxyManager, context.CancellationToken);
    GeneratorRuntime generatorRuntime = new GeneratorRuntime(context.Compilation, dependencyFactory, context.CancellationToken);

    try
    {
      RegisterProxies(proxyManager);
      Execute(context, generatorRuntime);
    }
    catch (OperationCanceledException ex) when (ExceptionHandler is { } exceptionHandler)
    {
      exceptionHandler.HandleOperationCanceledException(ex);
    }
  }

  protected virtual void RegisterProxies(IProxyTypeSetup setup)
  {
    IEnumerable<Type> proxyTypes = ProxiesAssemblies
      .SelectMany(a => a.GetTypes())
      .Where(t => t.IsDefined(typeof(ProxyClassAttribute), false));

    foreach (Type proxyType in proxyTypes)
    {
      setup.RegisterProxyType(proxyType);
    }
  }

  public abstract void Execute(GeneratorExecutionContext context, IGeneratorRuntime runtime);

  public abstract void Initialize(GeneratorInitializationContext context);
}