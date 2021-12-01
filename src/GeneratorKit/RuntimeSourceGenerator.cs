using GeneratorKit.Proxy;
using Microsoft.CodeAnalysis;

namespace GeneratorKit;

public abstract class RuntimeSourceGenerator : ISourceGenerator
{
  public void Execute(GeneratorExecutionContext context)
  {
    string assemblyName = context.Compilation.AssemblyName is string name
      ? $"{name}.GeneratorProxies"
      : "GeneratorProxies";

    ProxyTypeFactory typeFactory = new ProxyTypeFactory(assemblyName);
    ConcreteGeneratorRuntime runtime = new ConcreteGeneratorRuntime(context.Compilation, typeFactory, context.CancellationToken);
    
    Execute(context, runtime);
  }

  public abstract void Execute(GeneratorExecutionContext context, IGeneratorRuntime runtime);

  public abstract void Initialize(GeneratorInitializationContext context);
}