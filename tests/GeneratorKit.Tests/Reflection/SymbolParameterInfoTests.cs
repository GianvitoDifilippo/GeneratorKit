using FluentAssertions;
using GeneratorKit.Comparers;
using GeneratorKit.TestHelpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace GeneratorKit.Reflection;

public class SymbolParameterInfoTests : IClassFixture<SymbolParameterInfoFixture>
{
  private readonly SymbolParameterInfoFixture _fixture;

  public SymbolParameterInfoTests(SymbolParameterInfoFixture fixture)
  {
    _fixture = fixture;
  }

  [Theory]
  [ParametersData]
  public void Sut_ShouldBeEquivalentToReference(ParameterCategory category)
  {
    // Arrange
    SymbolParameterInfo sut = _fixture.GetDelegator(category);
    ParameterInfo reference = _fixture.GetReference(category);

    // Act

    // Assert
    sut.Should().Equal(reference, ParameterInfoEqualityComparer.Default);
  }

  [Theory]
  [ParametersData]
  public void Attributes_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);

    ParameterAttributes expected = reference.Attributes;

    // Act
    ParameterAttributes actual = sut.Attributes;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ParametersData]
  public void CustomAttributes_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);

    IEnumerable<CustomAttributeData> expected = reference.CustomAttributes;

    // Act
    IEnumerable<CustomAttributeData> actual = sut.CustomAttributes;

    // Assert
    actual.Should().BeEquivalentTo(expected, CustomAttributeDataEqualityComparer.Default);
  }

  [Theory]
  [ParametersData]
  public void DefaultValue_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);
    
    object? expected = reference.DefaultValue;

    // Act
    object? actual = sut.DefaultValue;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ParametersData]
  public void GetCustomAttributesData_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);
    
    IList<CustomAttributeData> expected = reference.GetCustomAttributesData();

    // Act
    IList<CustomAttributeData> actual = sut.GetCustomAttributesData();

    // Assert
    actual.Should().BeEquivalentTo(expected, CustomAttributeDataEqualityComparer.Default);
  }

  [Theory]
  [ParametersData]
  public void HasDefaultValue_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.HasDefaultValue;

    // Act
    bool actual = sut.HasDefaultValue;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ParametersData]
  public void IsIn_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsIn;

    // Act
    bool actual = sut.IsIn;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ParametersData]
  public void IsLcid_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsLcid;

    // Act
    bool actual = sut.IsLcid;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ParametersData]
  public void IsOptional_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsOptional;

    // Act
    bool actual = sut.IsOptional;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ParametersData]
  public void IsOut_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsOut;

    // Act
    bool actual = sut.IsOut;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ParametersData]
  public void IsRetval_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsRetval;

    // Act
    bool actual = sut.IsRetval;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ParametersData]
  public void Member_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);

    MemberInfo expected = reference.Member;

    // Act
    MemberInfo actual = sut.Member;

    // Assert
    actual.Should().Equal(expected, MemberInfoEqualityComparer.Default);
  }

  [Theory]
  [ParametersData]
  public void Name_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);

    string? expected = reference.Name;

    // Act
    string? actual = sut.Name;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ParametersData]
  public void ParameterType_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);

    Type expected = reference.ParameterType;

    // Act
    Type actual = sut.ParameterType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [ParametersData]
  public void Position_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);
    
    int expected = reference.Position;

    // Act
    int actual = sut.Position;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ParametersData]
  public void RawDefaultValue_ShouldBeCorrect(ParameterCategory category)
  {
    // Arrange
    ParameterInfo reference = _fixture.GetReference(category);
    SymbolParameterInfo sut = _fixture.GetDelegator(category);

    object? expected = reference.RawDefaultValue;

    // Act
    object? actual = sut.RawDefaultValue;

    // Assert
    actual.Should().Be(expected);
  }
}