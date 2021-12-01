using GeneratorKit.TestHelpers;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Threading;

namespace GeneratorKit.Reflection;

public class SymbolAssemblyFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName + ".SymbolAssembly";

  private const string s_source =
@"

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(""OtherAssembly"")]

namespace " + AssemblyName + @"
{
  public class OtherClass { }
}

namespace " + Namespace + @"
{
  public class BaseClass { }

  public class DerivedClass : BaseClass { }

  public interface IInterface { }

  internal class InternalClass { }

  public class Program
  {
    public static void Main(string[] args)
    {
    }
  }
}

";

  private readonly SymbolAssembly _delegator;
  private readonly Assembly _reference;

  public SymbolAssemblyFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName, OutputKind.ConsoleApplication);
    if (!output.IsValid)
    {
      throw new Exception($"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");
    }

    FakeGeneratorRuntime runtime = new FakeGeneratorRuntime(output.Compilation);
    Symbol = output.Compilation.Assembly;
    EntryPoint = output.Compilation.GetEntryPoint(CancellationToken.None)!;
    _delegator = new SymbolAssembly(runtime, output.Compilation.Assembly, EntryPoint);
    _reference = output.Assembly!;

    INamedTypeSymbol attributeType = output.Compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.InternalsVisibleToAttribute")!;
    CustomAttributeTypedArgument attributeArgument = new CustomAttributeTypedArgument("OtherAssembly");
    CustomAttributeData = CompilationCustomAttributeData.FromSymbol(
      runtime,
      attributeType.Constructors[0],
      new[] { attributeArgument },
      Array.Empty<CustomAttributeNamedArgument>());
  }

  public IAssemblySymbol Symbol { get; }
  public IMethodSymbol EntryPoint { get; }
  public CustomAttributeData CustomAttributeData { get; }

  internal SymbolAssembly GetDelegator()
  {
    return _delegator;
  }

  public Assembly GetReference()
  {
    return _reference;
  }
}
