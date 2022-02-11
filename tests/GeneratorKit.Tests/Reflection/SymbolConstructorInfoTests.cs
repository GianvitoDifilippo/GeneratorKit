using FluentAssertions;
using GeneratorKit.Comparers;
using GeneratorKit.Reflection.Binders;
using GeneratorKit.TestHelpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace GeneratorKit.Reflection;

public class SymbolConstructorInfoTests : IClassFixture<SymbolConstructorInfoFixture>
{
  private readonly SymbolConstructorInfoFixture _fixture;

  public SymbolConstructorInfoTests(SymbolConstructorInfoFixture fixture)
  {
    _fixture = fixture;
  }

  [Theory]
  [ConstructorsData]
  public void Sut_ShouldBeEquivalentToReference(ConstructorCategory category)
  {
    // Arrange
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);
    ConstructorInfo reference = _fixture.GetReference(category);

    // Act

    // Assert
    sut.Should().Equal(reference, ConstructorInfoEqualityComparer.Default);
  }

  [Theory]
  [ConstructorsData]
  public void Attributes_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);
    
    MethodAttributes expected = reference.Attributes;

    // Act
    MethodAttributes actual = sut.Attributes;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void CallingConventions_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    CallingConventions expected = reference.CallingConvention;

    // Act
    CallingConventions actual = sut.CallingConvention;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void ContainsGenericParameters_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.ContainsGenericParameters;

    // Act
    bool actual = sut.ContainsGenericParameters;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void CustomAttributes_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    IEnumerable<CustomAttributeData> expected = reference.CustomAttributes;

    // Act
    IEnumerable<CustomAttributeData> actual = sut.CustomAttributes;

    // Assert
    actual.Should().BeEquivalentTo(expected, CustomAttributeDataEqualityComparer.Default);
  }

  [Theory]
  [ConstructorsData]
  public void DeclaringType_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    Type? expected = reference.DeclaringType;

    // Act
    Type? actual = sut.DeclaringType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [ConstructorsData]
  public void GetCustomAttributesData_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    IList<CustomAttributeData> expected = reference.GetCustomAttributesData();

    // Act
    IList<CustomAttributeData> actual = sut.GetCustomAttributesData();

    // Assert
    actual.Should().BeEquivalentTo(expected, CustomAttributeDataEqualityComparer.Default);
  }

  [Theory]
  [ConstructorsData]
  public void GetGenericArguments_ShouldThrow(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    Func<Type[]> expected = () => reference.GetGenericArguments();

    // Act
    Func<Type[]> actual = () => sut.GetGenericArguments();

    // Assert
    actual.Should().ThrowSameExceptionsAs(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsAbstract_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsAbstract;

    // Act
    bool actual = sut.IsAbstract;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsAssembly_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsAssembly;

    // Act
    bool actual = sut.IsAssembly;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsConstructor_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsConstructor;

    // Act
    bool actual = sut.IsConstructor;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsFamily_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsFamily;

    // Act
    bool actual = sut.IsFamily;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsFamilyAndAssembly_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsFamilyAndAssembly;

    // Act
    bool actual = sut.IsFamilyAndAssembly;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsFamilyOrAssembly_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsFamilyOrAssembly;

    // Act
    bool actual = sut.IsFamilyOrAssembly;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsFinal_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsFinal;

    // Act
    bool actual = sut.IsFinal;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsGenericMethod_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsGenericMethod;

    // Act
    bool actual = sut.IsGenericMethod;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsGenericMethodDefinition_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsGenericMethodDefinition;

    // Act
    bool actual = sut.IsGenericMethodDefinition;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsHideBySig_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsHideBySig;

    // Act
    bool actual = sut.IsHideBySig;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsPrivate_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsPrivate;

    // Act
    bool actual = sut.IsPrivate;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsPublic_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsPublic;

    // Act
    bool actual = sut.IsPublic;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsSecurityCritical_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsSecurityCritical;

    // Act
    bool actual = sut.IsSecurityCritical;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsSecuritySafeCritical_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsSecuritySafeCritical;

    // Act
    bool actual = sut.IsSecuritySafeCritical;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsSecurityTransparent_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsSecurityTransparent;

    // Act
    bool actual = sut.IsSecurityTransparent;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsSpecialName_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsSpecialName;

    // Act
    bool actual = sut.IsSpecialName;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsStatic_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsStatic;

    // Act
    bool actual = sut.IsStatic;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void IsVirtual_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsVirtual;

    // Act
    bool actual = sut.IsVirtual;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void MemberType_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    MemberTypes expected = reference.MemberType;

    // Act
    MemberTypes actual = sut.MemberType;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void MethodImplementationFlags_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);
    
    MethodImplAttributes expected = reference.MethodImplementationFlags;

    // Act
    MethodImplAttributes actual = sut.MethodImplementationFlags;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void Module_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    Module expected = reference.Module;

    // Act
    Module actual = sut.Module;

    // Assert
    actual.Should().Equal(expected, ModuleEqualityComparer.Default);
  }

  [Theory]
  [ConstructorsData]
  public void Name_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    string expected = reference.Name;

    // Act
    string actual = sut.Name;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [ConstructorsData]
  public void ReflectedType_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    Type? expected = reference.ReflectedType;

    // Act
    Type? actual = sut.ReflectedType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [ConstructorsData]
  public void GetParameters_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    ConstructorInfo reference = _fixture.GetReference(category);
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);

    ParameterInfo[] expected = reference.GetParameters();

    // Act
    ParameterInfo[] actual = sut.GetParameters();

    // Assert
    actual.Should().BeEquivalentTo(expected, ParameterInfoEqualityComparer.Default);
  }

  [Theory]
  [ConstructorsData]
  public void UnderlyingSystemConstructor_ShouldBeCorrect(ConstructorCategory category)
  {
    // Arrange
    SymbolConstructorInfo sut = _fixture.GetDelegator(category);
    ConstructorInfo reference = _fixture.GetReference(category);
    
    ConstructorInfo expected = reference;

    // Act
    ConstructorInfo actual = DelegatorBinder.ResolveConstructor(sut.DeclaringType.UnderlyingSystemType, sut);
  
    // Assert
    actual.Should().Equal(expected, ConstructorInfoEqualityComparer.Default);
  }
}
