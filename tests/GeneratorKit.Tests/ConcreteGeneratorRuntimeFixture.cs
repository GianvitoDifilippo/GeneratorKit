using GeneratorKit.Proxy;
using GeneratorKit.TestHelpers;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Threading;

namespace GeneratorKit;

public class ConcreteGeneratorRuntimeFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName + ".ConcreteGeneratorRuntime";

  private const string s_source =
@"

namespace " + Namespace + @"
{
  public class Class { }

  public class Base<T1, T2> { }

  public class Derived<T> : Base<int, T> { }
}

";

  public ConcreteGeneratorRuntimeFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName);
    if (!output.IsValid)
    {
      throw new Exception($"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");
    }

    Compilation = output.Compilation;
    Assembly = output.Assembly!;
  }

  public Compilation Compilation { get; }
  public Assembly Assembly { get; }

  public ITypeSymbol GetTypeSymbol(string name)
  {
    name = $"{Namespace}.{name}";
    return Compilation.GetTypeByMetadataName(name) ?? throw new Exception($"Could not find type {name} in compilation.");
  }

  public Type GetType(string name)
  {
    name = $"{Namespace}.{name}";
    return Assembly.GetType(name) ?? throw new Exception($"Could not find type {name} in assembly.");
  }

  internal ConcreteGeneratorRuntime GetGeneratorRuntime(IProxyTypeFactory factory)
  {
    return new ConcreteGeneratorRuntime(Compilation, factory, CancellationToken.None);
  }
}