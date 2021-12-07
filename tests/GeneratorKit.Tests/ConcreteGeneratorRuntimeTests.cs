using FluentAssertions;
using GeneratorKit.Comparers;
using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using Microsoft.CodeAnalysis;
using Moq;
using System;
using System.Reflection;
using Xunit;

namespace GeneratorKit;

public class ConcreteGeneratorRuntimeTests : IClassFixture<ConcreteGeneratorRuntimeFixture>
{
  private readonly ConcreteGeneratorRuntimeFixture _fixture;
  private readonly Mock<IProxyTypeFactory> _factoryMock;

  public ConcreteGeneratorRuntimeTests(ConcreteGeneratorRuntimeFixture fixture)
  {
    _fixture = fixture;
    _factoryMock = new Mock<IProxyTypeFactory>(MockBehavior.Strict);
  }

  [Fact]
  public void CompilationAssembly_ShouldBeCorrect()
  {
    // Arrange
    ConcreteGeneratorRuntime sut = _fixture.GetGeneratorRuntime(_factoryMock.Object);

    Assembly expected = _fixture.Assembly;

    // Act
    SymbolAssembly actual = sut.CompilationAssembly;

    // Assert
    actual.Should().Equal(expected, AssemblyEqualityComparer.Default);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnType_WhenTypeIsInCompilation()
  {
    // Arrange
    ConcreteGeneratorRuntime sut = _fixture.GetGeneratorRuntime(_factoryMock.Object);

    const string typeName = "Class";
    Type expected = _fixture.GetType(typeName);
    SymbolType type = new SymbolNamedType(sut, (INamedTypeSymbol)_fixture.GetTypeSymbol(typeName));
    _factoryMock
      .Setup(x => x.CreateProxyType(sut, type))
      .Returns(expected);

    // Act
    Type? actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnTypeWithoutFactory_WhenTypeIsReferenced()
  {
    // Arrange
    ConcreteGeneratorRuntime sut = _fixture.GetGeneratorRuntime(_factoryMock.Object);

    Type expected = typeof(object);
    SymbolType type = new SymbolNamedType(sut, _fixture.Compilation.GetSpecialType(SpecialType.System_Object));

    // Act
    Type? actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnType_WhenTypeIsArray()
  {
    // Arrange
    ConcreteGeneratorRuntime sut = _fixture.GetGeneratorRuntime(_factoryMock.Object);

    const string typeName = "Class";
    Type elementType = _fixture.GetType(typeName);
    Type expected = elementType.MakeArrayType();
    SymbolType type = new SymbolNamedType(sut, (INamedTypeSymbol)_fixture.GetTypeSymbol(typeName));
    _factoryMock
      .Setup(x => x.CreateProxyType(sut, type))
      .Returns(elementType);

    // Act
    Type? actual = sut.GetRuntimeType(type.MakeArrayType());

    // Assert
    actual.Should().Be(expected);
  }
}