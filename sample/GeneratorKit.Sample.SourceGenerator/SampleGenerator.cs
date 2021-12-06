using GeneratorKit;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Linq;

namespace SourceGenerator
{
  [Generator]
  public class SampleGenerator : RuntimeSourceGenerator
  {
    public override void Execute(GeneratorExecutionContext context, IGeneratorRuntime runtime)
    {
      Type configurationType = runtime.CompilationAssembly
        .GetTypes()
        .FirstOrDefault(x => x.BaseType.Equals(typeof(GeneratorConfiguration)));

      if (configurationType == null)
      {
        return;
      }

      GeneratorConfiguration configuration = (GeneratorConfiguration)Activator.CreateInstance(configurationType);

      // 'configuration.ClassNames' will be empty because nothing is executed... for now ;)
      foreach (string className in configuration.ClassNames)
      {
        context.AddSource(className, $"namespace {configurationType.Namespace} {{ public class {className} {{  }} }}");
      }
    }

    public override void Initialize(GeneratorInitializationContext context)
    {
      if (!Debugger.IsAttached)
      {
        Debugger.Launch();
      }
    }
  }
}