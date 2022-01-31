using GeneratorKit.Interpret;
using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace GeneratorKit;

public class ConcreteGeneratorRuntimeFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName;

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

  private readonly Compilation _compilation;
  private readonly Mock<IDependencyFactory> _dependencyFactoryMock;
  private readonly Mock<IActivator> _activatorMock;
  private readonly Mock<IInterpreter> _interpreterMock;
  private readonly Mock<IFrameProvider> _frameProviderMock;

  public ConcreteGeneratorRuntimeFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName, referencedAssemblies: new[] { typeof(ProxyTypes).Assembly });
    Assert.True(output.IsValid, $"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");

    _compilation = output.Compilation;

    _activatorMock = new Mock<IActivator>(MockBehavior.Strict);
    _interpreterMock = new Mock<IInterpreter>(MockBehavior.Strict);
    _frameProviderMock = new Mock<IFrameProvider>(MockBehavior.Strict);
    _dependencyFactoryMock = new Mock<IDependencyFactory>(MockBehavior.Strict);

    IActivator activator = _activatorMock.Object;
    IInterpreter interpreter = _interpreterMock.Object;
    IFrameProvider frameProvider = _frameProviderMock.Object;
    _dependencyFactoryMock
      .Setup(x => x.GetDependencies(It.IsAny<GeneratorRuntime>(), out activator, out interpreter, out frameProvider));
  }

  internal SymbolType GetSpecialType(ConcreteGeneratorRuntime sut, SpecialType specialType) => new SymbolNamedType(sut, _compilation.GetSpecialType(specialType));

  public void MockActivator<T>()
  {
    _activatorMock
      .Setup(x => x.CreateInstance<T>(It.IsAny<IRuntimeType>(), Array.Empty<object?>()))
      .Returns<IRuntimeType, object?[]>((t, args) => (T)System.Activator.CreateInstance(t.UnderlyingSystemType)!);
  }

  internal ConcreteGeneratorRuntime CreateSut(IProxyManager proxyManager)
  {
    return new ConcreteGeneratorRuntime(_compilation, proxyManager, _dependencyFactoryMock.Object, CancellationToken.None);
  }

  internal SymbolType GetSourceType(SourceType sourceType, ConcreteGeneratorRuntime runtime)
  {
    INamedTypeSymbol symbol = sourceType switch
    {
      SourceType.NonGenericClass             => GetTypeSymbolFromCompilation("NonGenericClassSource"),
      SourceType.NonGenericClassGenericBase  => GetTypeSymbolFromCompilation("NonGenericClassGenericBaseSource"),
      SourceType.GenericClass                => GetTypeSymbolFromCompilation("GenericClassSource`1"),
      SourceType.GenericClassLessParameters  => GetTypeSymbolFromCompilation("GenericClassSourceLessParameters`1"),
      SourceType.GenericClassMoreParameters1 => GetTypeSymbolFromCompilation("GenericClassSourceMoreParameters1`3"),
      SourceType.GenericClassMoreParameters2 => GetTypeSymbolFromCompilation("GenericClassSourceMoreParameters2`3"),
      SourceType.Interface                   => GetTypeSymbolFromCompilation("InterfaceSource"),
      _                                      => throw Errors.Unreacheable
    };

    return new SymbolNamedType(runtime, symbol);

    INamedTypeSymbol GetTypeSymbolFromCompilation(string name)
    {
      INamedTypeSymbol? result = _compilation.GetTypeByMetadataName($"{Namespace}.{name}");
      Assert.NotNull(result);
      return result!;
    }
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