using GeneratorKit.Emit;
using GeneratorKit.Exceptions;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit;

public abstract class RuntimeSourceGenerator : ISourceGenerator
{
  protected virtual IExceptionHandler? ExceptionHandler => this as IExceptionHandler;

  public void Execute(GeneratorExecutionContext context)
  {
    ProxyTypeFactory typeFactory = new ProxyTypeFactory(Constants.ProxiesAssemblyName);
    ConcreteGeneratorRuntime runtime = new ConcreteGeneratorRuntime(context.Compilation, typeFactory, context.CancellationToken);
    
    try
    {
      Execute(context, runtime);
    }
    catch (Exception ex)
    {
      if (ExceptionHandler is not IExceptionHandler exceptionHandler || !Catch(ex, exceptionHandler))
        throw;
    }
  }

  public abstract void Execute(GeneratorExecutionContext context, IGeneratorRuntime runtime);

  public abstract void Initialize(GeneratorInitializationContext context);

  private static bool Catch(Exception ex, IExceptionHandler exceptionHandler)
  {
    switch (ex)
    {
      case TargetInvocationException targetInvocationException:
        Catch(targetInvocationException.InnerException, exceptionHandler);
        break;
      case TypeCreationException typeCreationException:
        exceptionHandler.HandleTypeCreationException(typeCreationException);
        break;
      case OperationCanceledException operationCanceledException:
        exceptionHandler.HandleOperationCanceledException(operationCanceledException);
        break;
      default:
        return false;
    }
    return true;
  }
}