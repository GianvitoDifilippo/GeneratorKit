using GeneratorKit.TestHelpers;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Threading;

namespace GeneratorKit.Reflection;

public class SymbolModuleFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName + ".SymbolModule";

  private const string s_source =
@"

[module: System.CLSCompliant(true)]

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
}

";

  private readonly SymbolModule _delegator;
  private readonly Module _reference;

  public SymbolModuleFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName);
    if (!output.IsValid)
    {
      throw new Exception($"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");
    }

    FakeGeneratorRuntime runtime = new FakeGeneratorRuntime(output.Compilation);
    Symbol = output.Compilation.Assembly;
    EntryPoint = output.Compilation.GetEntryPoint(CancellationToken.None)!;
    _delegator = new SymbolModule(runtime, output.Compilation.SourceModule);
    _reference = output.Assembly!.ManifestModule;

    INamedTypeSymbol attributeType = output.Compilation.GetTypeByMetadataName("System.CLSCompliantAttribute")!;
    CustomAttributeTypedArgument attributeArgument = new CustomAttributeTypedArgument(true);
    CustomAttributeData = CompilationCustomAttributeData.FromSymbol(
      runtime,
      attributeType.Constructors[0],
      new[] { attributeArgument },
      Array.Empty<CustomAttributeNamedArgument>());
  }

  public IAssemblySymbol Symbol { get; }
  public IMethodSymbol EntryPoint { get; }
  public CustomAttributeData CustomAttributeData { get; }

  internal SymbolModule GetDelegator()
  {
    return _delegator;
  }

  public Module GetReference()
  {
    return _reference;
  }
}
