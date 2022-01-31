using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using Xunit;

namespace GeneratorKit.Proxy;

public class ProxyManagerFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName + ".Proxy";

  private const string s_source = @"

using static GeneratorKit.TestHelpers.ProxyTypes;

namespace " + Namespace + @"
{
  public class NonGenericClassSource : NonGenericClass { }

  public class NonGenericClassGenericBaseSource : GenericClass<int> { }

  public class GenericClassSource<T> : GenericClass<T> { }

  public class GenericClassSourceLessParameters<T> : GenericClass<string, T> { }

  public class GenericClassSourceMoreParameters1<T1, T2, T3> : GenericClass<T3, T1> { }

  public class GenericClassSourceMoreParameters2<T1, T2, T3> : GenericClass<string, T3> { }

  public class InterfaceSource : IInterface { }
}
";

  private readonly SymbolType _nonGenericClassSourceType;
  private readonly SymbolType _nonGenericClassGenericBaseSourceType;
  private readonly SymbolType _genericClassSourceType;
  private readonly SymbolType _genericClassSourceLessParametersType;
  private readonly SymbolType _genericClassSourceMoreParametersType1;
  private readonly SymbolType _genericClassSourceMoreParametersType2;
  private readonly SymbolType _interfaceSourceType;

  public ProxyManagerFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName, referencedAssemblies: new[] { typeof(ProxyTypes).Assembly });
    Assert.True(output.IsValid, $"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");

    Compilation compilation = output.Compilation;
    FakeGeneratorRuntime runtime = new FakeGeneratorRuntime(compilation);
    runtime.AddType(compilation.GetSpecialType(SpecialType.System_String), typeof(string));
    runtime.AddType(compilation.GetSpecialType(SpecialType.System_Int32), typeof(int));

    _nonGenericClassSourceType = new SymbolNamedType(runtime, compilation.GetTypeByMetadataName(Namespace + ".NonGenericClassSource")!);
    _nonGenericClassGenericBaseSourceType = new SymbolNamedType(runtime, compilation.GetTypeByMetadataName(Namespace + ".NonGenericClassGenericBaseSource")!);
    _genericClassSourceType = new SymbolNamedType(runtime, compilation.GetTypeByMetadataName(Namespace + ".GenericClassSource`1")!);
    _genericClassSourceLessParametersType = new SymbolNamedType(runtime, compilation.GetTypeByMetadataName(Namespace + ".GenericClassSourceLessParameters`1")!);
    _genericClassSourceMoreParametersType1 = new SymbolNamedType(runtime, compilation.GetTypeByMetadataName(Namespace + ".GenericClassSourceMoreParameters1`3")!);
    _genericClassSourceMoreParametersType2 = new SymbolNamedType(runtime, compilation.GetTypeByMetadataName(Namespace + ".GenericClassSourceMoreParameters2`3")!);
    _interfaceSourceType = new SymbolNamedType(runtime, compilation.GetTypeByMetadataName(Namespace + ".InterfaceSource")!);
  }

  internal SymbolType GetSourceType(SourceType sourceType)
  {
    return sourceType switch
    {
      SourceType.NonGenericClass             => _nonGenericClassSourceType,
      SourceType.NonGenericClassGenericBase  => _nonGenericClassGenericBaseSourceType,
      SourceType.GenericClass                => _genericClassSourceType,
      SourceType.GenericClassLessParameters  => _genericClassSourceLessParametersType,
      SourceType.GenericClassMoreParameters1 => _genericClassSourceMoreParametersType1,
      SourceType.GenericClassMoreParameters2 => _genericClassSourceMoreParametersType2,
      SourceType.Interface                   => _interfaceSourceType,
      _                                      => throw Errors.Unreacheable
    };
  }

  public enum SourceType
  {
    NonGenericClass,
    NonGenericClassGenericBase,
    GenericClass,
    GenericClassLessParameters,
    GenericClassMoreParameters1,
    GenericClassMoreParameters2,
    Interface
  }
}