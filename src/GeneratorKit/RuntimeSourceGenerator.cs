using GeneratorKit.Exceptions;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit;

public abstract class RuntimeSourceGenerator : ISourceGenerator
{
  protected virtual IExceptionHandler? ExceptionHandler => null;

  public void Execute(GeneratorExecutionContext context)
  {
    string assemblyName = context.Compilation.AssemblyName is string name
      ? $"{name}.GeneratorProxies"
      : "GeneratorProxies";

    ProxyTypeFactory typeFactory = new ProxyTypeFactory(assemblyName);
    ConcreteGeneratorRuntime runtime = new ConcreteGeneratorRuntime(context.Compilation, typeFactory, context.CancellationToken);
    
    try
    {
      Execute(context, runtime);
    }
    catch (TypeCreationException ex) when (ExceptionHandler is { } exceptionHandler)
    {
      exceptionHandler.HandleTypeCreationException(ex);
    }
    catch (OperationCanceledException ex) when (ExceptionHandler is { } exceptionHandler)
    {
      exceptionHandler.HandleOperationCanceledException(ex);
    }
  }

  public abstract void Execute(GeneratorExecutionContext context, IGeneratorRuntime runtime);

  public abstract void Initialize(GeneratorInitializationContext context);
}