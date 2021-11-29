using Microsoft.CodeAnalysis;

namespace GeneratorKit;

public abstract class RuntimeSourceGenerator : ISourceGenerator
{
  public void Execute(GeneratorExecutionContext context)
  {
    ConcreteGeneratorRuntime runtime = new ConcreteGeneratorRuntime(context.Compilation, context.CancellationToken);
    Execute(context, runtime);
  }

  public abstract void Initialize(GeneratorInitializationContext context);

  protected abstract void Execute(GeneratorExecutionContext context, IGeneratorRuntime runtime);
}