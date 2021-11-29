using FluentAssertions;
using GeneratorKit.Comparers;
using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using Xunit;

namespace GeneratorKit;

public class ConcreteGeneratorRuntimeTests : IClassFixture<ConcreteGeneratorRuntimeFixture>
{
  private readonly ConcreteGeneratorRuntimeFixture _fixture;

  public ConcreteGeneratorRuntimeTests(ConcreteGeneratorRuntimeFixture fixture)
  {
    _fixture = fixture;
  }

  [Fact]
  public void CompilationAssembly_ShouldBeCorrect()
  {
    // Arrange
    ConcreteGeneratorRuntime sut = _fixture.GetGeneratorRuntime();

    Assembly expected = _fixture.Assembly;

    // Act
    SymbolAssembly actual = sut.CompilationAssembly;

    // Assert
    actual.Should().Equal(expected, AssemblyEqualityComparer.Default);
  }

  [Fact]
  public void GetTypeSymbol_ShouldReturnCorrectSymbol_WhenTypeIsReferenced()
  {
    // Arrange
    ConcreteGeneratorRuntime sut = _fixture.GetGeneratorRuntime();

    ITypeSymbol expected = _fixture.Compilation.GetSpecialType(SpecialType.System_Object);

    // Act
    ITypeSymbol? actual = sut.GetTypeSymbol(typeof(object));

    // Assert
    actual.Should().Equal(expected, SymbolEqualityComparer.Default);
  }

  [Fact]
  public void GetTypeSymbol_ShouldReturnCorrectSymbol_WhenTypeIsInCompilation()
  {
    // Arrange
    ConcreteGeneratorRuntime sut = _fixture.GetGeneratorRuntime();

    const string typeName = "Class";
    Type type = _fixture.GetType(typeName);
    ITypeSymbol expected = _fixture.GetTypeSymbol(typeName);

    // Act
    ITypeSymbol? actual = sut.GetTypeSymbol(type);

    // Assert
    actual.Should().Equal(expected, SymbolEqualityComparer.Default);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnActivatableObject_WhenTypeIsReferenced()
  {
    // Arrange
    ConcreteGeneratorRuntime sut = _fixture.GetGeneratorRuntime();

    Type expected = typeof(object);
    SymbolType type = new SymbolNamedType(sut, _fixture.Compilation.GetSpecialType(SpecialType.System_Object));

    // Act
    Type? actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnActivatableObject_WhenTypeIsInCompilation()
  {
    // Arrange
    ConcreteGeneratorRuntime sut = _fixture.GetGeneratorRuntime();

    const string typeName = "Class";
    Type expected = _fixture.GetType(typeName);
    SymbolType type = new SymbolNamedType(sut, (INamedTypeSymbol)_fixture.GetTypeSymbol(typeName));

    // Act
    Type? actual = sut.GetRuntimeType(type);

    // Assert
    actual!.Should().NotBeNull();
    Func<object?> activation = () => Activator.CreateInstance(actual!);
    activation.Should().NotThrow();
  }
}