using FluentAssertions;
using GeneratorKit.Comparers;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace GeneratorKit.Reflection;

public class SymbolModuleTests : IClassFixture<SymbolModuleFixture>
{
  private const string s_namespace = SymbolModuleFixture.Namespace;

  private readonly SymbolModuleFixture _fixture;

  public SymbolModuleTests(SymbolModuleFixture fixture)
  {
    _fixture = fixture;
  }

  [Fact]
  public void Assembly_ShouldBeCorrect()
  {
    // Arrange
    SymbolModule sut = _fixture.GetDelegator();
    Module reference = _fixture.GetReference();
    Assembly expected = reference.Assembly;

    // Act
    Assembly actual = sut.Assembly;

    // Assert
    actual.Should().Equal(expected, AssemblyEqualityComparer.Default);
  }

  [Fact]
  public void CustomAttributes_ShouldBeCorrect()
  {
    // Arrange
    SymbolModule sut = _fixture.GetDelegator();
    Module reference = _fixture.GetReference();
    IEnumerable<CustomAttributeData> expected = reference.CustomAttributes;

    // Act
    IEnumerable<CustomAttributeData> actual = sut.CustomAttributes;

    // Assert
    expected.Should().Contain(x => CustomAttributeDataEqualityComparer.Default.Equals(x, _fixture.CustomAttributeData));
    actual.Should().Contain(x => CustomAttributeDataEqualityComparer.Default.Equals(x, _fixture.CustomAttributeData));
  }

  [Fact]
  public void FullyQualifiedName_ShouldBeCorrect()
  {
    // Arrange
    SymbolModule sut = _fixture.GetDelegator();
    Module reference = _fixture.GetReference();
    string expected = reference.FullyQualifiedName;

    // Act
    string actual = sut.FullyQualifiedName;

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void GetCustomAttributesData_ShouldBeCorrect()
  {
    // Arrange
    SymbolModule sut = _fixture.GetDelegator();
    Module reference = _fixture.GetReference();
    IList<CustomAttributeData> expected = reference.GetCustomAttributesData();

    // Act
    IList<CustomAttributeData> actual = sut.GetCustomAttributesData();

    // Assert
    expected.Should().Contain(x => CustomAttributeDataEqualityComparer.Default.Equals(x, _fixture.CustomAttributeData));
    actual.Should().Contain(x => CustomAttributeDataEqualityComparer.Default.Equals(x, _fixture.CustomAttributeData));
  }

  [Fact]
  public void GetTypes_ShouldBeCorrect()
  {
    // Arrange
    SymbolModule sut = _fixture.GetDelegator();
    Module reference = _fixture.GetReference();
    IEnumerable<Type> expected = reference.GetTypes();

    // Act
    IEnumerable<Type> actual = sut.GetTypes();

    // Assert
    actual.Should().BeEquivalentTo(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [InlineData(s_namespace + ".BaseClass")]
  [InlineData(s_namespace + ".GenericClass`2")]
  public void GetType_ShouldReturnType_WhenCorrectNameIsProviderAndIsCaseSensitive(string name)
  {
    // Arrange
    SymbolModule sut = _fixture.GetDelegator();
    Module reference = _fixture.GetReference();
    Type? expected = reference.GetType(name, false, false);

    // Act
    Type? actual = sut.GetType(name, false, false);

    // Assert
    expected.Should().Equal(actual, TypeEqualityComparer.Default);
  }

  [Fact]
  public void GetType_ShouldReturnNull_WhenIncorrectNameIsProvidedAndIsCaseSensitive()
  {
    // Arrange
    SymbolModule sut = _fixture.GetDelegator();
    Module reference = _fixture.GetReference();
    Type? expected = reference.GetType("x", false, false);

    // Act
    Type? actual = sut.GetType("x", false, false);

    // Assert
    expected.Should().BeNull();
    actual.Should().BeNull();
  }

  [Fact]
  public void GetType_ShouldReturnType_WhenCorrectNameIsProviderAndIsNotCaseSensitive()
  {
    // Arrange
    const string name = s_namespace + ".baseClass";
    SymbolModule sut = _fixture.GetDelegator();
    Module reference = _fixture.GetReference();
    Type? expected = reference.GetType(name, false, true);

    // Act
    Type? actual = sut.GetType(name, false, true);

    // Assert
    expected.Should().Equal(actual, TypeEqualityComparer.Default);
  }

  [Fact]
  public void GetType_ShouldThrowTypeLoadException_WhenThrowOnErrorIsTrue()
  {
    // Arrange
    SymbolModule sut = _fixture.GetDelegator();

    // Act
    Func<Type?> getType = () => sut.GetType("", true, false);

    // Assert
    getType.Should().ThrowExactly<TypeLoadException>();
  }

  [Fact]
  public void Name_ShouldBeCorrect()
  {
    // Arrange
    SymbolModule sut = _fixture.GetDelegator();
    Module reference = _fixture.GetReference();
    string expected = reference.Name;

    // Act
    string actual = sut.Name;

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void ScopeName_ShouldBeCorrect()
  {
    // Arrange
    SymbolModule sut = _fixture.GetDelegator();
    Module reference = _fixture.GetReference();
    string expected = reference.ScopeName;

    // Act
    string actual = sut.ScopeName;

    // Assert
    actual.Should().Be(expected);
  }
}