using FluentAssertions;
using GeneratorKit.Comparers;
using GeneratorKit.Reflection.Binders;
using GeneratorKit.TestHelpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace GeneratorKit.Reflection;

public class SymbolFieldInfoTests : IClassFixture<SymbolFieldInfoFixture>
{
  private readonly SymbolFieldInfoFixture _fixture;

  public SymbolFieldInfoTests(SymbolFieldInfoFixture fixture)
  {
    _fixture = fixture;
  }

  [Theory]
  [FieldsData]
  public void Sut_ShouldBeEquivalentToReference(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    // Act

    // Assert
    sut.Should().Equal(reference, FieldInfoEqualityComparer.Default);
  }

  [Theory]
  [FieldsData]
  public void Attributes_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);
    
    FieldAttributes expected = reference.Attributes;

    // Act
    FieldAttributes actual = sut.Attributes;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void CustomAttributes_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);
    
    IEnumerable<CustomAttributeData> expected = reference.CustomAttributes;

    // Act
    IEnumerable<CustomAttributeData> actual = sut.CustomAttributes;

    // Assert
    actual.Should().BeEquivalentTo(expected, CustomAttributeDataEqualityComparer.Default);
  }

  [Theory]
  [FieldsData]
  public void DeclaringType_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    Type? expected = reference.DeclaringType;

    // Act
    Type actual = sut.DeclaringType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [FieldsData]
  public void FieldType_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    Type expected = reference.FieldType;

    // Act
    Type actual = sut.FieldType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [FieldsData]
  public void GetCustomAttributesData_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    IList<CustomAttributeData> expected = reference.GetCustomAttributesData();

    // Act
    IList<CustomAttributeData> actual = sut.GetCustomAttributesData();

    // Assert
    actual.Should().BeEquivalentTo(expected, CustomAttributeDataEqualityComparer.Default);
  }

  [Theory]
  [FieldsData]
  public void IsAssembly_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsAssembly;

    // Act
    bool actual = sut.IsAssembly;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void IsFamily_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsFamily;

    // Act
    bool actual = sut.IsFamily;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void IsFamilyAndAssembly_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsFamilyAndAssembly;

    // Act
    bool actual = sut.IsFamilyAndAssembly;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void IsFamilyOrAssembly_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsFamilyOrAssembly;

    // Act
    bool actual = sut.IsFamilyOrAssembly;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void IsInitOnly_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsInitOnly;

    // Act
    bool actual = sut.IsInitOnly;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void IsLiteral_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsLiteral;

    // Act
    bool actual = sut.IsLiteral;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void IsNotSerialized_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsNotSerialized;

    // Act
    bool actual = sut.IsNotSerialized;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void IsPinvokeImpl_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsPinvokeImpl;

    // Act
    bool actual = sut.IsPinvokeImpl;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void IsPrivate_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsPrivate;

    // Act
    bool actual = sut.IsPrivate;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void IsPublic_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsPublic;

    // Act
    bool actual = sut.IsPublic;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void IsSecurityCritical_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsSecurityCritical;

    // Act
    bool actual = sut.IsSecurityCritical;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void IsSecuritySafeCritical_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsSecuritySafeCritical;

    // Act
    bool actual = sut.IsSecuritySafeCritical;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void IsSecurityTransparent_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsSecurityTransparent;

    // Act
    bool actual = sut.IsSecurityTransparent;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void IsSpecialName_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsSpecialName;

    // Act
    bool actual = sut.IsSpecialName;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void IsStatic_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsStatic;

    // Act
    bool actual = sut.IsStatic;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void MemberType_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    MemberTypes expected = reference.MemberType;

    // Act
    MemberTypes actual = sut.MemberType;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void Module_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    Module expected = reference.Module;

    // Act
    Module actual = sut.Module;

    // Assert
    actual.Should().Equal(expected, ModuleEqualityComparer.Default);
  }

  [Theory]
  [FieldsData]
  public void Name_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    string expected = reference.Name;

    // Act
    string actual = sut.Name;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [FieldsData]
  public void ReflectedType_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    Type? expected = reference.ReflectedType;

    // Act
    Type? actual = sut.ReflectedType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [FieldsData]
  public void UnderlyingSystemField_ShouldBeCorrect(FieldCategory category)
  {
    // Arrange
    FieldInfo reference = _fixture.GetReference(category);
    SymbolFieldInfo sut = _fixture.GetDelegator(category);

    FieldInfo expected = reference;

    // Act
    FieldInfo actual = DelegatorBinder.ResolveField(sut.ReflectedType.UnderlyingSystemType, sut);

    // Assert
    actual.Should().Equal(expected, FieldInfoEqualityComparer.Default);
  }
}