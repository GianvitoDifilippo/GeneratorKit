using GeneratorKit.Interpret;
using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using GeneratorKit.Reflection.Context;
using GeneratorKit.TestHelpers;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace GeneratorKit;

public class GeneratorRuntimeFixture
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
  private readonly Mock<IActivator> _activatorMock;
  private readonly Mock<IInterpreter> _interpreterMock;

  public GeneratorRuntimeFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName, referencedAssemblies: new[] { typeof(ProxyTypes).Assembly });
    Assert.True(output.IsValid, $"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");

    _compilation = output.Compilation;

    _activatorMock = new Mock<IActivator>(MockBehavior.Strict);
    _interpreterMock = new Mock<IInterpreter>(MockBehavior.Strict);
  }

  internal SymbolType GetSpecialType(GeneratorRuntime runtime, SpecialType specialType)
  {
    return new SymbolNamedType(runtime, _compilation.GetSpecialType(specialType));
  }

  public void MockActivator<T>()
  {
    _activatorMock
      .Setup(x => x.CreateInstance(It.IsAny<SymbolNamedType>(), Array.Empty<object?>()))
      .Returns<SymbolType, object?[]>((t, args) => (T)System.Activator.CreateInstance(t.UnderlyingSystemType)!);
  }

  internal GeneratorRuntime CreateSut(IProxyManager proxyManager)
  {
    return new GeneratorRuntime(_compilation, new MockDependencyFactory(_activatorMock.Object, proxyManager, _interpreterMock.Object), CancellationToken.None);
  }

  internal SymbolType GetSourceType(SourceType sourceType, GeneratorRuntime runtime)
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

  private class MockDependencyFactory : IDependencyFactory
  {
    private readonly IActivator _activator;
    private readonly IProxyManager _proxyManager;
    private readonly IInterpreter _interpreter;

    public MockDependencyFactory(IActivator activator, IProxyManager proxyManager, IInterpreter interpreter)
    {
      _activator = activator;
      _proxyManager = proxyManager;
      _interpreter = interpreter;
    }

    public void CreateDependencies(GeneratorContext context, out IActivator activator, out IProxyManager proxyManager, out IInterpreter interpreter)
    {
      activator = _activator;
      proxyManager = _proxyManager;
      interpreter = _interpreter;
    }
  }
}