using FluentAssertions;
using GeneratorKit.Comparers;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace GeneratorKit.Reflection;

public class SymbolTypeTests : IClassFixture<SymbolTypeFixture>
{
  private readonly SymbolTypeFixture _fixture;

  public SymbolTypeTests(SymbolTypeFixture fixture)
  {
    _fixture = fixture;
  }

  [Theory, SymbolTypeTestData]
  public void Assembly_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    Assembly expected = reference.Assembly;

    // Act
    Assembly actual = sut.Assembly;

    // Assert
    actual.Should().Equal(expected, AssemblyEqualityComparer.Default);
  }

  [Theory, SymbolTypeTestData]
  public void AssemblyQualifiedName_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    string? expected = reference.AssemblyQualifiedName;

    // Act
    string? actual = sut.AssemblyQualifiedName;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void Attributes_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    TypeAttributes expected = reference.Attributes;

    // Act
    TypeAttributes actual = sut.Attributes;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void BaseType_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    Type? expected = reference.BaseType;

    // Act
    Type? actual = sut.BaseType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory, SymbolTypeTestData]
  public void ContainsGenericParameters_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.ContainsGenericParameters;

    // Act
    bool actual = sut.ContainsGenericParameters;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void CustomAttributes_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    IEnumerable<CustomAttributeData> expected = reference.CustomAttributes;

    // Act
    IEnumerable<CustomAttributeData> actual = sut.CustomAttributes;

    // Assert
    actual.Should().BeEquivalentTo(expected, CustomAttributeDataEqualityComparer.Default);
  }

  [Theory, SymbolTypeTestData]
  public void DeclaringMethod_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    Func<MethodBase?> expected = () => reference.DeclaringMethod;

    // Act
    Func<MethodBase?> actual = () => sut.DeclaringMethod;

    // Assert
    expected.Should().ThrowExactly<InvalidOperationException>();
    actual.Should().ThrowExactly<InvalidOperationException>();
  }

  [Theory, SymbolTypeTestData]
  public void DeclaringType_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    Type? expected = reference.DeclaringType;

    // Act
    Type? actual = sut.DeclaringType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory, SymbolTypeTestData]
  public void FullName_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    string? expected = reference.FullName;

    // Act
    string? actual = sut.FullName;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void GenericTypeArguments_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    Type[] expected = reference.GenericTypeArguments;

    // Act
    Type[] actual = sut.GenericTypeArguments;

    // Assert
    actual.Should().BeEquivalentTo(expected, TypeEqualityComparer.Default);
  }

  [Theory, AllBindingFlagsData]
  public void GetConstructors_ShouldBeCorrect(BindingFlags bindingAttr)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);
    ConstructorInfo[] expected = reference.GetConstructors(bindingAttr);

    // Act
    ConstructorInfo[] actual = sut.GetConstructors(bindingAttr);

    // Assert
    actual.Should().BeEquivalentTo(expected, ConstructorInfoEqualityComparer.Default);
  }

  [Theory, SymbolTypeTestData]
  public void GetCustomAttributeData_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    IList<CustomAttributeData> expected = reference.GetCustomAttributesData();

    // Act
    IList<CustomAttributeData> actual = sut.GetCustomAttributesData();

    // Assert
    actual.Should().BeEquivalentTo(expected, CustomAttributeDataEqualityComparer.Default);
  }

  [Theory, SymbolTypeTestData]
  public void GetDefaultMembers_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    MemberInfo[] expected = reference.GetDefaultMembers();

    // Act
    MemberInfo[] actual = sut.GetDefaultMembers();

    // Assert
    actual.Should().BeEquivalentTo(expected, MemberInfoEqualityComparer.Default);
  }

  [Theory, SymbolTypeTestData]
  public void GetElementType_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    Type? expected = reference.GetElementType();

    // Act
    Type? actual = sut.GetElementType();

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory, AllBindingFlagsData]
  public void GetFields_ShouldBeCorrect(BindingFlags bindingAttr)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);
    FieldInfo[] expected = reference.GetFields(bindingAttr);

    // Act
    FieldInfo[] actual = sut.GetFields(bindingAttr);

    // Assert
    actual.Should().BeEquivalentTo(expected, FieldInfoEqualityComparer.Default);
  }

  [Theory, AllBindingFlagsData]
  public void GetMembers_ShouldBeCorrect(BindingFlags bindingAttr)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);
    MemberInfo[] expected = reference.GetMembers(bindingAttr);

    // Act
    MemberInfo[] actual = sut.GetMembers(bindingAttr);

    // Assert
    actual.Should().BeEquivalentTo(expected, MemberInfoEqualityComparer.Default);
  }

  [Theory, AllBindingFlagsData]
  public void GetMethods_ShouldBeCorrect(BindingFlags bindingAttr)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);
    MethodInfo[] expected = reference.GetMethods(bindingAttr);

    // Act
    MethodInfo[] actual = sut.GetMethods(bindingAttr);

    // Assert
    actual.Should().BeEquivalentTo(expected, MethodInfoEqualityComparer.Default);
  }

  [Theory, AllBindingFlagsData]
  public void GetNestedTypes_ShouldBeCorrect(BindingFlags bindingAttr)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);
    Type[] expected = reference.GetNestedTypes(bindingAttr);

    // Act
    Type[] actual = sut.GetNestedTypes(bindingAttr);

    // Assert
    actual.Should().BeEquivalentTo(expected, TypeEqualityComparer.Default);
  }

  [Theory, AllBindingFlagsData]
  public void GetProperties_ShouldBeCorrect(BindingFlags bindingAttr)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);
    PropertyInfo[] expected = reference.GetProperties(bindingAttr);

    // Act
    PropertyInfo[] actual = sut.GetProperties(bindingAttr);

    // Assert
    actual.Should().BeEquivalentTo(expected, PropertyInfoEqualityComparer.Default);
  }

  [Theory, SymbolTypeTestData]
  public void HasElementType_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.HasElementType;

    // Act
    bool actual = sut.HasElementType;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsAbstract_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsAbstract;

    // Act
    bool actual = sut.IsAbstract;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsAnsiClass_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsAnsiClass;

    // Act
    bool actual = sut.IsAnsiClass;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsArray_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsArray;

    // Act
    bool actual = sut.IsArray;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [InlineData(TypeCategory.WithAllMembers, TypeCategory.WithAllMembersBase)]
  [InlineData(TypeCategory.WithAllMembersBase, TypeCategory.WithAllMembers)]
  [InlineData(TypeCategory.WithAllMembers, TypeCategory.WithAllMembersRoot)]
  [InlineData(TypeCategory.WithAllMembersRoot, TypeCategory.WithAllMembers)]
  [InlineData(TypeCategory.WithAllMembers, TypeCategory.BaseInterface)]
  [InlineData(TypeCategory.BaseInterface, TypeCategory.WithAllMembers)]
  [InlineData(TypeCategory.WithAllMembers, TypeCategory.DerivedInterface)]
  [InlineData(TypeCategory.DerivedInterface, TypeCategory.WithAllMembers)]
  [InlineData(TypeCategory.WithAllMembersBase, TypeCategory.BaseInterface)]
  [InlineData(TypeCategory.BaseInterface, TypeCategory.WithAllMembersBase)]
  [InlineData(TypeCategory.DerivedInterface, TypeCategory.BaseInterface)]
  [InlineData(TypeCategory.BaseInterface, TypeCategory.DerivedInterface)]
  [InlineData(TypeCategory.WithAllMembersBase, TypeCategory.DerivedInterface)]
  [InlineData(TypeCategory.DerivedInterface, TypeCategory.WithAllMembersBase)]
  [InlineData(TypeCategory.WithAllMembersRoot, TypeCategory.Object)]
  [InlineData(TypeCategory.Object, TypeCategory.WithAllMembersRoot)]
  [InlineData(TypeCategory.OpenGeneric, TypeCategory.ClosedGeneric)]
  [InlineData(TypeCategory.ClosedGeneric, TypeCategory.OpenGeneric)]
  [InlineData(TypeCategory.Enum, TypeCategory.Int)]
  [InlineData(TypeCategory.Int, TypeCategory.Enum)]
  [InlineData(TypeCategory.ClosedGeneric, TypeCategory.Sealed)]
  [InlineData(TypeCategory.Sealed, TypeCategory.ClosedGeneric)]
  [InlineData(TypeCategory.Enum, TypeCategory.EnumBase)]
  [InlineData(TypeCategory.EnumBase, TypeCategory.Enum)]
  public void IsAssignableFrom_ShouldBeCorrect(TypeCategory category, TypeCategory targetCategory)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    SymbolType targetType = _fixture.GetDelegator(targetCategory);
    Type reference = _fixture.GetReference(category);
    Type referenceTargetType = _fixture.GetReference(targetCategory);
    bool expected = reference.IsAssignableFrom(referenceTargetType);
  
    // Act
    bool actual = sut.IsAssignableFrom(targetType);
  
    // Assert
    actual.Should().Be(expected);
  }
  
  [Theory]
  [InlineData(TypeCategory.WithAllMembers, TypeCategory.WithAllMembersBase)]
  [InlineData(TypeCategory.WithAllMembersBase, TypeCategory.WithAllMembers)]
  [InlineData(TypeCategory.WithAllMembers, TypeCategory.WithAllMembersRoot)]
  [InlineData(TypeCategory.WithAllMembersRoot, TypeCategory.WithAllMembers)]
  [InlineData(TypeCategory.WithAllMembers, TypeCategory.BaseInterface)]
  [InlineData(TypeCategory.BaseInterface, TypeCategory.WithAllMembers)]
  [InlineData(TypeCategory.WithAllMembers, TypeCategory.DerivedInterface)]
  [InlineData(TypeCategory.DerivedInterface, TypeCategory.WithAllMembers)]
  [InlineData(TypeCategory.WithAllMembersBase, TypeCategory.BaseInterface)]
  [InlineData(TypeCategory.BaseInterface, TypeCategory.WithAllMembersBase)]
  [InlineData(TypeCategory.DerivedInterface, TypeCategory.BaseInterface)]
  [InlineData(TypeCategory.BaseInterface, TypeCategory.DerivedInterface)]
  [InlineData(TypeCategory.WithAllMembersBase, TypeCategory.DerivedInterface)]
  [InlineData(TypeCategory.DerivedInterface, TypeCategory.WithAllMembersBase)]
  [InlineData(TypeCategory.WithAllMembersRoot, TypeCategory.Object)]
  [InlineData(TypeCategory.Object, TypeCategory.WithAllMembersRoot)]
  [InlineData(TypeCategory.OpenGeneric, TypeCategory.ClosedGeneric)]
  [InlineData(TypeCategory.ClosedGeneric, TypeCategory.OpenGeneric)]
  [InlineData(TypeCategory.Enum, TypeCategory.Int)]
  [InlineData(TypeCategory.Int, TypeCategory.Enum)]
  [InlineData(TypeCategory.ClosedGeneric, TypeCategory.Sealed)]
  [InlineData(TypeCategory.Sealed, TypeCategory.ClosedGeneric)]
  [InlineData(TypeCategory.Enum, TypeCategory.EnumBase)]
  [InlineData(TypeCategory.EnumBase, TypeCategory.Enum)]
  public void IsAssignableTo_ShouldBeCorrect(TypeCategory category, TypeCategory targetCategory)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    SymbolType targetType = _fixture.GetDelegator(targetCategory);
    Type reference = _fixture.GetReference(category);
    Type referenceTargetType = _fixture.GetReference(targetCategory);
    bool expected = reference.IsAssignableTo(referenceTargetType);
  
    // Act
    bool actual = sut.IsAssignableTo(targetType);
  
    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsAutoClass_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsAutoClass;

    // Act
    bool actual = sut.IsAutoClass;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsAutoLayout_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsAutoLayout;

    // Act
    bool actual = sut.IsAutoLayout;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsByRef_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsByRef;

    // Act
    bool actual = sut.IsByRef;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsClass_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsClass;

    // Act
    bool actual = sut.IsClass;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsCOMObject_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsCOMObject;

    // Act
    bool actual = sut.IsCOMObject;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsConstructedGenericType_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsConstructedGenericType;

    // Act
    bool actual = sut.IsConstructedGenericType;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsContextful_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsContextful;

    // Act
    bool actual = sut.IsContextful;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsEnum_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsEnum;

    // Act
    bool actual = sut.IsEnum;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsGenericType_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsGenericType;

    // Act
    bool actual = sut.IsGenericType;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsGenericTypeDefinition_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsGenericTypeDefinition;

    // Act
    bool actual = sut.IsGenericTypeDefinition;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsInterface_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsInterface;

    // Act
    bool actual = sut.IsInterface;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsPrimitive_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsPrimitive;

    // Act
    bool actual = sut.IsPrimitive;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsLayoutSequential_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsLayoutSequential;

    // Act
    bool actual = sut.IsLayoutSequential;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsMarshalByRef_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsMarshalByRef;

    // Act
    bool actual = sut.IsMarshalByRef;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsNested_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsNested;

    // Act
    bool actual = sut.IsNested;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsNestedAssembly_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsNestedAssembly;

    // Act
    bool actual = sut.IsNestedAssembly;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsNestedFamANDAssem_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsNestedFamANDAssem;

    // Act
    bool actual = sut.IsNestedFamANDAssem;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsNestedFamily_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsNestedFamily;

    // Act
    bool actual = sut.IsNestedFamily;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsNestedFamORAssem_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsNestedFamORAssem;

    // Act
    bool actual = sut.IsNestedFamORAssem;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsNestedPrivate_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsNestedPrivate;

    // Act
    bool actual = sut.IsNestedPrivate;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsNestedPublic_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsNestedPublic;

    // Act
    bool actual = sut.IsNestedPublic;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsNotPublic_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsNotPublic;

    // Act
    bool actual = sut.IsNotPublic;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsPointer_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsPointer;

    // Act
    bool actual = sut.IsPointer;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsPublic_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsPublic;

    // Act
    bool actual = sut.IsPublic;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsSealed_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsPublic;

    // Act
    bool actual = sut.IsPublic;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsSecurityCritical_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsSecurityCritical;

    // Act
    bool actual = sut.IsSecurityCritical;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsSecuritySafeCritical_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsSecuritySafeCritical;

    // Act
    bool actual = sut.IsSecuritySafeCritical;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsSecurityTransparent_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsSecurityTransparent;

    // Act
    bool actual = sut.IsSecurityTransparent;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsSerializable_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsSerializable;

    // Act
    bool actual = sut.IsSerializable;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsSignatureType_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsSignatureType;

    // Act
    bool actual = sut.IsSignatureType;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void IsSpecialName_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    bool expected = reference.IsSpecialName;

    // Act
    bool actual = sut.IsSpecialName;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [InlineData(TypeCategory.WithAllMembers, TypeCategory.WithAllMembersBase)]
  [InlineData(TypeCategory.WithAllMembersBase, TypeCategory.WithAllMembers)]
  [InlineData(TypeCategory.WithAllMembers, TypeCategory.WithAllMembersRoot)]
  [InlineData(TypeCategory.WithAllMembersRoot, TypeCategory.WithAllMembers)]
  [InlineData(TypeCategory.WithAllMembers, TypeCategory.BaseInterface)]
  [InlineData(TypeCategory.BaseInterface, TypeCategory.WithAllMembers)]
  [InlineData(TypeCategory.WithAllMembers, TypeCategory.DerivedInterface)]
  [InlineData(TypeCategory.DerivedInterface, TypeCategory.WithAllMembers)]
  [InlineData(TypeCategory.WithAllMembersBase, TypeCategory.BaseInterface)]
  [InlineData(TypeCategory.BaseInterface, TypeCategory.WithAllMembersBase)]
  [InlineData(TypeCategory.DerivedInterface, TypeCategory.BaseInterface)]
  [InlineData(TypeCategory.BaseInterface, TypeCategory.DerivedInterface)]
  [InlineData(TypeCategory.WithAllMembersRoot, TypeCategory.Object)]
  [InlineData(TypeCategory.Object, TypeCategory.WithAllMembersRoot)]
  [InlineData(TypeCategory.OpenGeneric, TypeCategory.ClosedGeneric)]
  [InlineData(TypeCategory.ClosedGeneric, TypeCategory.OpenGeneric)]
  [InlineData(TypeCategory.Enum, TypeCategory.Int)]
  [InlineData(TypeCategory.Int, TypeCategory.Enum)]
  [InlineData(TypeCategory.Enum, TypeCategory.EnumBase)]
  [InlineData(TypeCategory.EnumBase, TypeCategory.Enum)]
  [InlineData(TypeCategory.ClosedGeneric, TypeCategory.Sealed)]
  [InlineData(TypeCategory.Sealed, TypeCategory.ClosedGeneric)]
  public void IsSubclassOf_ShouldBeCorrect(TypeCategory category, TypeCategory targetCategory)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    SymbolType targetType = _fixture.GetDelegator(targetCategory);
    Type reference = _fixture.GetReference(category);
    Type referenceTargetType = _fixture.GetReference(targetCategory);
    bool expected = reference.IsSubclassOf(referenceTargetType);
  
    // Act
    bool actual = sut.IsSubclassOf(targetType);
  
    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void MakeArrayType_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    Type expected = reference.MakeArrayType();

    // Act
    Type actual = sut.MakeArrayType();

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory, SymbolTypeTestData]
  public void Module_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    Module expected = reference.Module;

    // Act
    Module actual = sut.Module;

    // Assert
    actual.Should().Equal(expected, ModuleEqualityComparer.Default);
  }

  [Theory, SymbolTypeTestData]
  public void Name_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    string? expected = reference.Name;

    // Act
    string? actual = sut.Name;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void Namespace_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    string? expected = reference.Namespace;

    // Act
    string? actual = sut.Namespace;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory, SymbolTypeTestData]
  public void ReflectedType_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    Type? expected = reference.ReflectedType;

    // Act
    Type? actual = sut.ReflectedType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory, SymbolTypeTestData]
  public void UnderlyingSystemType_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);
    Type expected = reference.UnderlyingSystemType;

    // Act
    Type actual = sut.UnderlyingSystemType;

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }
}
