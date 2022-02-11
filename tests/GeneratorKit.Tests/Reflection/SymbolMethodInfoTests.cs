using FluentAssertions;
using GeneratorKit.Comparers;
using GeneratorKit.Reflection.Binders;
using GeneratorKit.TestHelpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace GeneratorKit.Reflection;

public class SymbolMethodInfoTests : IClassFixture<SymbolMethodInfoFixture>
{
  private readonly SymbolMethodInfoFixture _fixture;

  public SymbolMethodInfoTests(SymbolMethodInfoFixture fixture)
  {
    _fixture = fixture;
  }

  [Theory]
  [MethodsData]
  public void Sut_ShouldBeEquivalentToReference(MethodCategory category)
  {
    // Arrange
    SymbolMethodInfo sut = _fixture.GetDelegator(category);
    MethodInfo reference = _fixture.GetReference(category);

    // Act

    // Assert
    sut.Should().Equal(reference, MethodInfoEqualityComparer.Default);
  }

  [Theory]
  [MethodsData]
  public void Attributes_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);
    
    MethodAttributes expected = reference.Attributes;

    // Act
    MethodAttributes actual = sut.Attributes;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void CallingConventions_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    CallingConventions expected = reference.CallingConvention;

    // Act
    CallingConventions actual = sut.CallingConvention;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void ContainsGenericParameters_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.ContainsGenericParameters;

    // Act
    bool actual = sut.ContainsGenericParameters;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void CustomAttributes_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    IEnumerable<CustomAttributeData> expected = reference.CustomAttributes;
    
    // Act
    IEnumerable<CustomAttributeData> actual = sut.CustomAttributes;

    // Assert
    actual.Should().BeEquivalentTo(expected, CustomAttributeDataEqualityComparer.Default);
  }

  [Theory]
  [MethodsData]
  public void DeclaringType_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    Type? expected = reference.DeclaringType;

    // Act
    Type? actual = sut.DeclaringType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [MethodsData]
  public void GetBaseDefinition_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);
    
    MethodInfo expected = reference.GetBaseDefinition();

    // Act
    MethodInfo actual = sut.GetBaseDefinition();

    // Assert
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }

  [Theory]
  [MethodsData]
  public void GetCustomAttributesData_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    IList<CustomAttributeData> expected = reference.GetCustomAttributesData();

    // Act
    IList<CustomAttributeData> actual = sut.GetCustomAttributesData();

    // Assert
    actual.Should().BeEquivalentTo(expected, CustomAttributeDataEqualityComparer.Default);
  }

  [Theory]
  [MethodsData]
  public void GetGenericArguments_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    Type[] expected = reference.GetGenericArguments();

    // Act
    Type[] actual = sut.GetGenericArguments();

    // Assert
    actual.Should().BeEquivalentTo(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [InlineData(MethodCategory.GenericMethod)]
  [InlineData(MethodCategory.ConstructedGenericMethod)]
  [InlineData(MethodCategory.GenericMethodNotOverridden)]
  [InlineData(MethodCategory.ConstructedGenericMethodNotOverridden)]
  [InlineData(MethodCategory.GenericMethodNotOverriddenReflectedFromDerived)]
  [InlineData(MethodCategory.ConstructedGenericMethodNotOverriddenReflectedFromDerived)]
  [InlineData(MethodCategory.GenericMethodOverridden)]
  [InlineData(MethodCategory.GenericMethodOverriddenReflectedFromDerived)]
  [InlineData(MethodCategory.ConstructedGenericMethodOverridden)]
  [InlineData(MethodCategory.ConstructedGenericMethodOverriddenReflectedFromDerived)]
  [InlineData(MethodCategory.ConstructedGenericOverload1)]
  [InlineData(MethodCategory.ConstructedGenericOverload2)]
  [InlineData(MethodCategory.GenericMethodDefinitionInGenericClass)]
  [InlineData(MethodCategory.ConstructedGenericMethodInGenericClass)]
  [InlineData(MethodCategory.GenericMethodDefinitionInGenericClassDefinition)]
  [InlineData(MethodCategory.ConstructedGenericMethodInGenericClassDefinition)]
  [InlineData(MethodCategory.GenericOverriddenMethodDefinitionInGenericClassDefinition)]
  [InlineData(MethodCategory.GenericConstructedOverriddenMethodInGenericClassDefinition)]
  [InlineData(MethodCategory.GenericOverriddenMethodDefinitionInGenericConstructedClass)]
  [InlineData(MethodCategory.GenericConstructedOverriddenMethodInGenericConstructedClass)]
  public void GetGenericMethodDefinition_ShouldBeCorrect_WhenMethodIsGeneric(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    MethodInfo expected = reference.GetGenericMethodDefinition();

    // Act
    MethodInfo actual = sut.GetGenericMethodDefinition();
    
    // Assert
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }

  [Theory]
  [MethodsDataExcept(
    MethodCategory.GenericMethod,
    MethodCategory.ConstructedGenericMethod,
    MethodCategory.GenericMethodNotOverridden,
    MethodCategory.ConstructedGenericMethodNotOverridden,
    MethodCategory.GenericMethodNotOverriddenReflectedFromDerived,
    MethodCategory.ConstructedGenericMethodNotOverriddenReflectedFromDerived,
    MethodCategory.GenericMethodOverridden,
    MethodCategory.GenericMethodOverriddenReflectedFromDerived,
    MethodCategory.ConstructedGenericMethodOverridden,
    MethodCategory.ConstructedGenericMethodOverriddenReflectedFromDerived,
    MethodCategory.ConstructedGenericOverload1,
    MethodCategory.ConstructedGenericOverload2,
    MethodCategory.GenericMethodDefinitionInGenericClass,
    MethodCategory.ConstructedGenericMethodInGenericClass,
    MethodCategory.GenericMethodDefinitionInGenericClassDefinition,
    MethodCategory.ConstructedGenericMethodInGenericClassDefinition,
    MethodCategory.GenericOverriddenMethodDefinitionInGenericClassDefinition,
    MethodCategory.GenericConstructedOverriddenMethodInGenericClassDefinition,
    MethodCategory.GenericOverriddenMethodDefinitionInGenericConstructedClass,
    MethodCategory.GenericConstructedOverriddenMethodInGenericConstructedClass)]
  public void GetGenericMethodDefinition_ShouldThrow_WhenMethodIsNotGeneric(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    Func<MethodInfo> expected = () => reference.GetGenericMethodDefinition();

    // Act
    Func<MethodInfo> actual = () => sut.GetGenericMethodDefinition();

    // Assert
    actual.Should().ThrowSameExceptionsAs(expected);
  }

  [Theory]
  [MethodsData]
  public void IsAbstract_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsAbstract;

    // Act
    bool actual = sut.IsAbstract;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsAssembly_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsAssembly;

    // Act
    bool actual = sut.IsAssembly;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsConstructor_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsConstructor;

    // Act
    bool actual = sut.IsConstructor;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsFamily_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsFamily;

    // Act
    bool actual = sut.IsFamily;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsFamilyAndAssembly_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsFamilyAndAssembly;

    // Act
    bool actual = sut.IsFamilyAndAssembly;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsFamilyOrAssembly_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsFamilyOrAssembly;

    // Act
    bool actual = sut.IsFamilyOrAssembly;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsFinal_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsFinal;

    // Act
    bool actual = sut.IsFinal;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsHideBySig_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsHideBySig;

    // Act
    bool actual = sut.IsHideBySig;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsGenericMethod_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsGenericMethod;

    // Act
    bool actual = sut.IsGenericMethod;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsGenericMethodDefinition_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsGenericMethodDefinition;

    // Act
    bool actual = sut.IsGenericMethodDefinition;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsPrivate_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsPrivate;

    // Act
    bool actual = sut.IsPrivate;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsPublic_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsPublic;

    // Act
    bool actual = sut.IsPublic;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsSecurityCritical_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsSecurityCritical;

    // Act
    bool actual = sut.IsSecurityCritical;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsSecuritySafeCritical_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsSecuritySafeCritical;

    // Act
    bool actual = sut.IsSecuritySafeCritical;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsSecurityTransparent_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsSecurityTransparent;

    // Act
    bool actual = sut.IsSecurityTransparent;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsSpecialName_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsSpecialName;

    // Act
    bool actual = sut.IsSpecialName;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsStatic_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    bool expected = reference.IsStatic;

    // Act
    bool actual = sut.IsStatic;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void IsVirtual_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);
    
    bool expected = reference.IsVirtual;

    // Act
    bool actual = sut.IsVirtual;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [InlineData(MethodCategory.GenericMethod)]
  [InlineData(MethodCategory.GenericMethodNotOverridden)]
  [InlineData(MethodCategory.GenericMethodNotOverriddenReflectedFromDerived)]
  [InlineData(MethodCategory.GenericMethodOverridden)]
  [InlineData(MethodCategory.GenericMethodOverriddenReflectedFromDerived)]
  public void MakeGenericMethod_ShouldBeCorrect_WhenMethodIsGenericDefinition_TwoArguments(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    MethodInfo expected = reference.MakeGenericMethod(typeof(int), typeof(string));

    // Act
    MethodInfo actual = sut.MakeGenericMethod(_fixture.IntSymbolType, _fixture.StringSymbolType);

    // Assert
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }

  [Theory]
  [InlineData(MethodCategory.GenericMethodDefinitionInGenericClass)]
  [InlineData(MethodCategory.GenericMethodDefinitionInGenericClassDefinition)]
  [InlineData(MethodCategory.GenericOverriddenMethodDefinitionInGenericClassDefinition)]
  [InlineData(MethodCategory.GenericOverriddenMethodDefinitionInGenericConstructedClass)]
  public void MakeGenericMethod_ShouldBeCorrect_WhenMethodIsGenericDefinition_OneArgument(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    MethodInfo expected = reference.MakeGenericMethod(typeof(int));

    // Act
    MethodInfo actual = sut.MakeGenericMethod(_fixture.IntSymbolType);

    // Assert
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }

  [Theory]
  [MethodsDataExcept(
    MethodCategory.GenericMethod,
    MethodCategory.GenericMethodNotOverridden,
    MethodCategory.GenericMethodNotOverriddenReflectedFromDerived,
    MethodCategory.GenericMethodOverridden,
    MethodCategory.GenericMethodOverriddenReflectedFromDerived,
    MethodCategory.GenericMethodDefinitionInGenericClass,
    MethodCategory.GenericMethodDefinitionInGenericClassDefinition,
    MethodCategory.GenericOverriddenMethodDefinitionInGenericClassDefinition,
    MethodCategory.GenericOverriddenMethodDefinitionInGenericConstructedClass)]
  public void MakeGenericMethod_ShouldThrow_WhenMethodIsNotGenericDefinition(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    Func<MethodInfo> expected = () => reference.MakeGenericMethod(typeof(int), typeof(string));

    // Act
    Func<MethodInfo> actual = () => sut.MakeGenericMethod(typeof(int), typeof(string));

    // Assert
    actual.Should().ThrowSameExceptionsAs(expected);
  }

  [Theory]
  [MethodsData]
  public void MemberType_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    MemberTypes expected = reference.MemberType;

    // Act
    MemberTypes actual = sut.MemberType;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void MethodImplementationFlags_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    MethodImplAttributes expected = reference.MethodImplementationFlags;

    // Act
    MethodImplAttributes actual = sut.MethodImplementationFlags;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void Module_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);
    
    Module expected = reference.Module;

    // Act
    Module actual = sut.Module;

    // Assert
    actual.Should().Equal(expected, ModuleEqualityComparer.Default);
  }

  [Theory]
  [MethodsData]
  public void Name_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    string expected = reference.Name;

    // Act
    string actual = sut.Name;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [MethodsData]
  public void ReflectedType_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    Type? expected = reference.ReflectedType;

    // Act
    Type? actual = sut.ReflectedType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [MethodsData]
  public void ReturnType_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);
    
    Type? expected = reference.ReturnType;

    // Act
    Type? actual = sut.ReturnType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [MethodsData]
  public void GetParameters_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    MethodInfo reference = _fixture.GetReference(category);
    SymbolMethodInfo sut = _fixture.GetDelegator(category);

    ParameterInfo[] expected = reference.GetParameters();

    // Act
    ParameterInfo[] actual = sut.GetParameters();

    // Assert
    actual.Should().BeEquivalentTo(expected, ParameterInfoEqualityComparer.Default);
  }

  [Theory]
  [MethodsData]
  public void UnderlyingSystemMethod_ShouldBeCorrect(MethodCategory category)
  {
    // Arrange
    SymbolMethodInfo sut = _fixture.GetDelegator(category);
    MethodInfo reference = _fixture.GetReference(category);

    MethodInfo expected = reference;

    // Act
    MethodInfo actual = DelegatorBinder.ResolveMethod(sut.ReflectedType.UnderlyingSystemType, sut);

    // Assert
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }
}