using FluentAssertions;
using GeneratorKit.Comparers;
using System.Reflection;
using System;
using Xunit;
using System.Collections.Generic;

namespace GeneratorKit.Reflection;

public class SymbolPropertyInfoTests : IClassFixture<SymbolPropertyInfoFixture>
{
  private readonly SymbolPropertyInfoFixture _fixture;

  public SymbolPropertyInfoTests(SymbolPropertyInfoFixture fixture)
  {
    _fixture = fixture;
  }

  [Theory]
  [PropertiesData]
  public void Sut_ShouldBeEquivalentToReference(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    // Act

    // Assert
    sut.Should().Equal(reference, PropertyInfoEqualityComparer.Default);
  }

  [Theory]
  [PropertiesData]
  public void Attributes_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    PropertyAttributes expected = reference.Attributes;

    // Act
    PropertyAttributes actual = sut.Attributes;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [PropertiesData]
  public void CanRead_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.CanRead;

    // Act
    bool actual = sut.CanRead;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [PropertiesData]
  public void CanWrite_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.CanWrite;

    // Act
    bool actual = sut.CanWrite;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [PropertiesData]
  public void CustomAttributes_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    IEnumerable<CustomAttributeData> expected = reference.CustomAttributes;

    // Act
    IEnumerable<CustomAttributeData> actual = sut.CustomAttributes;

    // Assert
    actual.Should().BeEquivalentTo(expected, CustomAttributeDataEqualityComparer.Default);
  }

  [Theory]
  [PropertiesData]
  public void DeclaringType_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    Type? expected = reference.DeclaringType;

    // Act
    Type actual = sut.DeclaringType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [PropertiesData]
  public void GetAccessors_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    MethodInfo[] expected = reference.GetAccessors();

    // Act
    MethodInfo[] actual = sut.GetAccessors();

    // Assert
    actual.Should().BeEquivalentTo(expected, MethodInfoEqualityComparer.Default);
  }

  [Theory]
  [PropertiesData]
  public void GetCustomAttributesData_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);
    
    IList<CustomAttributeData> expected = reference.GetCustomAttributesData();

    // Act
    IList<CustomAttributeData> actual = sut.GetCustomAttributesData();

    // Assert
    actual.Should().BeEquivalentTo(expected, CustomAttributeDataEqualityComparer.Default);
  }

  [Theory]
  [PropertiesData]
  public void GetMethod_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    MethodInfo? expected = reference.GetMethod;

    // Act
    MethodInfo? actual = sut.GetMethod;

    // Assert
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }

  [Theory]
  [PropertiesData]
  public void GetGetMethod_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    MethodInfo? expected = reference.GetGetMethod();

    // Act
    MethodInfo? actual = sut.GetGetMethod();

    // Assert
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }

  [Theory]
  [PropertiesData]
  public void GetIndexParameters_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    ParameterInfo[] expected = reference.GetIndexParameters();

    // Act
    ParameterInfo[] actual = sut.GetIndexParameters();

    // Assert
    actual.Should().BeEquivalentTo(expected, ParameterInfoEqualityComparer.Default);
  }

  [Theory]
  [PropertiesData]
  public void GetSetMethod_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    MethodInfo? expected = reference.GetSetMethod();

    // Act
    MethodInfo? actual = sut.GetSetMethod();

    // Assert
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }

  [Theory]
  [PropertiesData]
  public void IsSpecialName_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);
    
    bool expected = reference.IsSpecialName;

    // Act
    bool actual = sut.IsSpecialName;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [PropertiesData]
  public void MemberType_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    MemberTypes expected = reference.MemberType;

    // Act
    MemberTypes actual = sut.MemberType;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [PropertiesData]
  public void Module_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    Module expected = reference.Module;

    // Act
    Module actual = sut.Module;

    // Assert
    actual.Should().Equal(expected, ModuleEqualityComparer.Default);
  }

  [Theory]
  [PropertiesData]
  public void Name_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    string expected = reference.Name;

    // Act
    string actual = sut.Name;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [PropertiesData]
  public void PropertyType_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    Type expected = reference.PropertyType;

    // Act
    Type actual = sut.PropertyType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [PropertiesData]
  public void ReflectedType_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    Type? expected = reference.ReflectedType;

    // Act
    Type actual = sut.ReflectedType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [PropertiesData]
  public void RuntimeProperty_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    PropertyInfo expected = reference;

    // Act
    PropertyInfo actual = sut.RuntimeProperty;

    // Assert
    actual.Should().Equal(expected, PropertyInfoEqualityComparer.Default);
  }

  [Theory]
  [PropertiesData]
  public void SetMethod_ShouldBeCorrect(PropertyCategory category)
  {
    // Arrange
    PropertyInfo reference = _fixture.GetReference(category);
    SymbolPropertyInfo sut = _fixture.GetDelegator(category);

    MethodInfo? expected = reference.SetMethod;

    // Act
    MethodInfo? actual = sut.SetMethod;

    // Assert
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }
}