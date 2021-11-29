using FluentAssertions;
using GeneratorKit.Comparers;
using GeneratorKit.TestHelpers;
using System;
using System.Collections;
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
  public void Sut_ShouldBeEquivalentToReference(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    // Act

    // Assert
    sut.Should().Equal(reference, TypeEqualityComparer.Default);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory, NamedTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [TypeParametersData]
  public void DeclaringMethod_ShouldBeCorrect_WhenTypeIsGenericParameter(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    MethodBase? expected = reference.DeclaringMethod;

    // Act
    MethodBase? actual = sut.DeclaringMethod;

    // Assert
    actual.Should().Equal(expected, MethodBaseEqualityComparer.Default);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData]
  public void DeclaringMethod_ShouldThrow_WhenTypeIsNotGenericParameter(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    Func<MethodBase?> expected = () => reference.DeclaringMethod;

    // Act
    Func<MethodBase?> actual = () => sut.DeclaringMethod;

    // Assert
    actual.Should().ThrowSameExceptionsAs(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [TypeParametersData]
  public void GenericParameterAttributes_ShouldBeCorrect_WhenTypeIsGenericParameter(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    GenericParameterAttributes expected = reference.GenericParameterAttributes;

    // Act
    GenericParameterAttributes actual = sut.GenericParameterAttributes;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData]
  public void GenericParameterAttributes_ShouldThrow_WhenTypeIsNotGenericParameter(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    Func<GenericParameterAttributes> expected = () => reference.GenericParameterAttributes;

    // Act
    Func<GenericParameterAttributes> actual = () => sut.GenericParameterAttributes;

    // Assert
    actual.Should().ThrowSameExceptionsAs(expected);
  }

  [Theory]
  [TypeParametersData]
  public void GenericParameterPosition_ShouldBeCorrect_WhenTypeIsGenericParameter(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    int expected = reference.GenericParameterPosition;

    // Act
    int actual = sut.GenericParameterPosition;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData]
  public void GenericParameterPosition_ShouldThrow_WhenTypeIsNotGenericParameter(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    Func<int> expected = () => reference.GenericParameterPosition;

    // Act
    Func<int> actual = () => sut.GenericParameterPosition;

    // Assert
    actual.Should().ThrowSameExceptionsAs(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [ArrayTypesData]
  public void GetArrayRank_ShouldBeCorrect_WhenTypeIsArrayType(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    int expected = reference.GetArrayRank();

    // Act
    int actual = sut.GetArrayRank();

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, TypeParametersData]
  public void GetArrayRank_ShouldThrow_WhenTypeIsNotArrayType(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    Func<int> expected = () => reference.GetArrayRank();

    // Act
    Func<int> actual = () => sut.GetArrayRank();

    // Assert
    actual.Should().ThrowSameExceptionsAs(expected);
  }

  [Fact]
  public void GetConstructor1Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    Type[] types = new[] { typeof(int) };
    ConstructorInfo? expected = reference.GetConstructor(types);

    // Act
    ConstructorInfo? actual = sut.GetConstructor(types);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, ConstructorInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetConstructor2Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
    Type[] types = new[] { typeof(float), typeof(float) };
    ConstructorInfo? expected = reference.GetConstructor(bindingAttr, types);

    // Act
    ConstructorInfo? actual = sut.GetConstructor(bindingAttr, types);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, ConstructorInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetConstructor4Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    Type[] types = new[] { typeof(float), typeof(float) };
    BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
    ConstructorInfo? expected = reference.GetConstructor(bindingAttr, null, types, null);

    // Act
    ConstructorInfo? actual = sut.GetConstructor(bindingAttr, null, types, null);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, ConstructorInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetConstructor5Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
    CallingConventions callConvention = CallingConventions.Standard | CallingConventions.HasThis;
    Type[] types = new[] { typeof(float), typeof(float) };
    ConstructorInfo? expected = reference.GetConstructor(bindingAttr, null, callConvention, types, null);

    // Act
    ConstructorInfo? actual = sut.GetConstructor(bindingAttr, null, callConvention, types, null);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, ConstructorInfoEqualityComparer.Default);
  }

  [Theory]
  [GetMembersData]
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

  [Theory]
  [NamedTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Fact]
  public void GetEnumName_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.Enum);
    Type reference = _fixture.GetReference(TypeCategory.Enum);

    object value = ((IList)reference.GetEnumValues())[0]!;
    string? expected = reference.GetEnumName(value);
    
    // Act
    string? actual = sut.GetEnumName(value);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void GetEnumNames_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.Enum);
    Type reference = _fixture.GetReference(TypeCategory.Enum);

    string[] expected = reference.GetEnumNames();

    // Act
    string[] actual = sut.GetEnumNames();

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetEnumUnderlyingType_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.Enum);
    Type reference = _fixture.GetReference(TypeCategory.Enum);

    Type expected = reference.GetEnumUnderlyingType();

    // Act
    Type actual = sut.GetEnumUnderlyingType();

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Fact]
  public void GetEnumValues_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.Enum);
    Type reference = _fixture.GetReference(TypeCategory.Enum);

    IEnumerable<object> expected = CastToGeneric(reference.GetEnumValues());

    // Act
    IEnumerable<object> actual = CastToGeneric(sut.GetEnumValues());

    // Assert
    actual.Should().Equal(expected);

    static IEnumerable<object> CastToGeneric(Array array)
    {
      foreach (object item in array)
      {
        yield return item;
      }
    }
  }

  [Fact]
  public void GetField1Args_ShouldBeCorrect()
  {
    // Arrange
    string name = "PublicField";
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    FieldInfo? expected = reference.GetField(name);

    // Act
    FieldInfo? actual = sut.GetField(name);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, FieldInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetField2Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "PrivateField";
    BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
    FieldInfo? expected = reference.GetField(name, bindingAttr);

    // Act
    FieldInfo? actual = sut.GetField(name, bindingAttr);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, FieldInfoEqualityComparer.Default);
  }

  [Theory]
  [GetMembersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
  public void GetGenericArguments_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    Type[] expected = reference.GetGenericArguments();

    // Act
    Type[] actual = sut.GetGenericArguments();

    // Assert
    actual.Should().BeEquivalentTo(expected, MemberInfoEqualityComparer.Default);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData]
  public void GetGenericParameterConstraints_ShouldThrow_WhenTypeIsNotGenericParameter(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    Func<Type[]> expected = () => reference.GetGenericParameterConstraints();

    // Act
    Func<Type[]> actual = () => sut.GetGenericParameterConstraints();

    // Assert
    actual.Should().ThrowSameExceptionsAs(expected);
  }

  [Theory]
  [TypeParametersData]
  public void GetGenericParameterConstraints_ShouldBeCorrect_WhenTypeIsGenericParameter(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    Type[] expected = reference.GetGenericParameterConstraints();

    // Act
    Type[] actual = sut.GetGenericParameterConstraints();

    // Assert
    actual.Should().BeEquivalentTo(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [InlineData(TypeCategory.ClosedGeneric)]
  [InlineData(TypeCategory.ClosedGenericWithGenericTypeArguments)]
  [InlineData(TypeCategory.OpenGeneric)]
  public void GetGenericTypeDefinition_ShouldBeCorrect_WhenTypeIsGeneric(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    Type expected = reference.GetGenericTypeDefinition();

    // Act
    Type actual = sut.GetGenericTypeDefinition();

    // Assert
    actual.Should().Equal(expected, MemberInfoEqualityComparer.Default);
  }

  [Theory]
  [NamedTypesDataExcept(TypeCategory.ClosedGeneric, TypeCategory.ClosedGenericWithGenericTypeArguments, TypeCategory.OpenGeneric)]
  [SpecialTypesData, ArrayTypesData, TypeParametersData]
  public void GetGenericTypeDefinition_ShouldThrow_WhenTypeIsNotGeneric(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    Func<Type> expected = () => reference.GetGenericTypeDefinition();
    
    // Act
    Func<Type> actual = () => sut.GetGenericTypeDefinition();

    // Assert
    actual.Should().ThrowSameExceptionsAs(expected);
  }

  [Fact]
  public void GetInterface1Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "IDerivedInterface";
    Type? expected = reference.GetInterface(name);

    // Act
    Type? actual = sut.GetInterface(name);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Fact]
  public void GetInterface2Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "iderivedinterface";
    Type? expected = reference.GetInterface(name, true);
    
    // Act
    Type? actual = sut.GetInterface(name, true);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
  public void GetInterfaces_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    Type[] expected = reference.GetInterfaces();

    // Act
    Type[] actual = sut.GetInterfaces();

    // Assert
    actual.Should().BeEquivalentTo(expected, MemberInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetMember1Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "PublicStaticProperty";
    MemberInfo[] expected = reference.GetMember(name);

    // Act
    MemberInfo[] actual = sut.GetMember(name);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().BeEquivalentTo(expected, MemberInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetMember2Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "ProtectedStaticProperty";
    BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Static;
    MemberInfo[] expected = reference.GetMember(name, bindingAttr);

    // Act
    MemberInfo[] actual = sut.GetMember(name, bindingAttr);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().BeEquivalentTo(expected, MemberInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetMember3Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "PrivateProtectedStaticProperty";
    MemberTypes type = MemberTypes.Property;
    BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Static;
    MemberInfo[] expected = reference.GetMember(name, type, bindingAttr);
    
    // Act
    MemberInfo[] actual = sut.GetMember(name, type, bindingAttr);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().BeEquivalentTo(expected, MemberInfoEqualityComparer.Default);
  }

  [Theory]
  [GetMembersData]
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

  [Fact]
  public void GetMethod1Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "PublicMethod";
    MethodInfo? expected = reference.GetMethod(name);

    // Act
    MethodInfo? actual = sut.GetMethod(name);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetMethod2ArgsBindingAttrs_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "PrivateMethod";
    BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
    MethodInfo? expected = reference.GetMethod(name, bindingAttr);

    // Act
    MethodInfo? actual = sut.GetMethod(name, bindingAttr);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetMethod2ArgsTypes_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "PublicStaticMethod";
    Type[] types = new[] { typeof(int), typeof(string) };
    MethodInfo? expected = reference.GetMethod(name, types);

    // Act
    MethodInfo? actual = sut.GetMethod(name, types);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetMethod3Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "PrivateMethod";
    BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
    Type[] types = new[] { typeof(int), typeof(string) };
    MethodInfo? expected = reference.GetMethod(name, bindingAttr, types);

    // Act
    MethodInfo? actual = sut.GetMethod(name, bindingAttr, types);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetMethod5Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "PrivateStaticMethod";
    BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Static;
    Type[] types = new[] { typeof(int), typeof(string) };
    MethodInfo? expected = reference.GetMethod(name, bindingAttr, null, types, null);

    // Act
    MethodInfo? actual = sut.GetMethod(name, bindingAttr, null, types, null);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetMethod6Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "ProtectedStaticMethod";
    BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Static;
    CallingConventions callConvention = CallingConventions.Standard | CallingConventions.HasThis;
    Type[] types = new[] { typeof(int), typeof(string) };
    MethodInfo? expected = reference.GetMethod(name, bindingAttr, null, callConvention, types, null);

    // Act
    MethodInfo? actual = sut.GetMethod(name, bindingAttr, null, callConvention, types, null);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, MethodInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetMethods0Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    MethodInfo[] expected = reference.GetMethods();

    // Act
    MethodInfo[] actual = sut.GetMethods();

    // Assert
    actual.Should().BeEquivalentTo(expected, MethodInfoEqualityComparer.Default);
  }

  [Theory]
  [GetMembersData]
  public void GetMethods1Args_ShouldBeCorrect(BindingFlags bindingAttr)
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

  [Fact]
  public void GetNestedType1Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "NestedPublicClass";
    Type? expected = reference.GetNestedType(name);

    // Act
    Type? actual = sut.GetNestedType(name);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Fact]
  public void GetNestedType2Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "NestedProtectedClass";
    BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
    Type? expected = reference.GetNestedType(name, bindingAttr);

    // Act
    Type? actual = sut.GetNestedType(name, bindingAttr);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [GetMembersData]
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

  [Fact]
  public void GetProperties0Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    PropertyInfo[] expected = reference.GetProperties();

    // Act
    PropertyInfo[] actual = sut.GetProperties();

    // Assert
    actual.Should().BeEquivalentTo(expected, PropertyInfoEqualityComparer.Default);
  }

  [Theory]
  [GetMembersData]
  public void GetProperties1Args_ShouldBeCorrect(BindingFlags bindingAttr)
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

  [Fact]
  public void GetProperty1Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "PublicProperty";
    PropertyInfo? expected = reference.GetProperty(name);

    // Act
    PropertyInfo? actual = sut.GetProperty(name);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, PropertyInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetProperty2ArgsBindingAttr_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "InternalProperty";
    BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
    PropertyInfo? expected = reference.GetProperty(name, bindingAttr);

    // Act
    PropertyInfo? actual = sut.GetProperty(name, bindingAttr);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, PropertyInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetProperty2ArgsTypes_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "Item";
    Type[] types = new[] { typeof(int), typeof(int) };
    PropertyInfo? expected = reference.GetProperty(name, types);

    // Act
    PropertyInfo? actual = sut.GetProperty(name, types);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, PropertyInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetProperty3Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "Item";
    Type[] types = new[] { typeof(int), typeof(int) };
    Type returnType = typeof(int);
    PropertyInfo? expected = reference.GetProperty(name, returnType, types);

    // Act
    PropertyInfo? actual = sut.GetProperty(name, returnType, types);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, PropertyInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetProperty4Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "Item";
    Type[] types = new[] { typeof(int), typeof(int) };
    Type returnType = typeof(int);
    PropertyInfo? expected = reference.GetProperty(name, returnType, types, null);

    // Act
    PropertyInfo? actual = sut.GetProperty(name, returnType, types, null);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, PropertyInfoEqualityComparer.Default);
  }

  [Fact]
  public void GetProperty5Args_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.WithAllMembers);
    Type reference = _fixture.GetReference(TypeCategory.WithAllMembers);

    string name = "Item";
    Type[] types = new[] { typeof(int), typeof(int) };
    Type returnType = typeof(int);
    BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance;
    PropertyInfo? expected = reference.GetProperty(name, bindingAttr, null, returnType, types, null);

    // Act
    PropertyInfo? actual = sut.GetProperty(name, bindingAttr, null, returnType, types, null);

    // Assert
    expected.Should().NotBeNull();
    actual.Should().Equal(expected, PropertyInfoEqualityComparer.Default);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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
    Type reference = _fixture.GetReference(category);

    SymbolType targetType = _fixture.GetDelegator(targetCategory);
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
    Type reference = _fixture.GetReference(category);

    SymbolType targetType = _fixture.GetDelegator(targetCategory);
    Type referenceTargetType = _fixture.GetReference(targetCategory);
    bool expected = reference.IsAssignableTo(referenceTargetType);
  
    // Act
    bool actual = sut.IsAssignableTo(targetType);
  
    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Fact]
  public void IsEnumDefined_ShouldBeCorrect()
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(TypeCategory.Enum);
    Type reference = _fixture.GetReference(TypeCategory.Enum);

    object value = ((IList)reference.GetEnumValues())[0]!;
    bool expected = reference.IsEnumDefined(value);

    // Act
    bool actual = sut.IsEnumDefined(value);

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
  public void IsExplicitLayout_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    bool expected = reference.IsExplicitLayout;

    // Act
    bool actual = sut.IsExplicitLayout;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
  public void IsGenericMethodParameter_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    bool expected = reference.IsGenericMethodParameter;

    // Act
    bool actual = sut.IsGenericMethodParameter;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
  public void IsGenericParameter_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    bool expected = reference.IsGenericParameter;

    // Act
    bool actual = sut.IsGenericParameter;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
  public void IsGenericTypeParameter_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    bool expected = reference.IsGenericTypeParameter;
    
    // Act
    bool actual = sut.IsGenericTypeParameter;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
  public void IsImport_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    bool expected = reference.IsImport;
    
    // Act
    bool actual = sut.IsImport;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [InlineData(TypeCategory.Int)]
  [InlineData(TypeCategory.Internal)]
  public void IsInstanceOfType_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    object instance = Activator.CreateInstance(reference)!;
    bool expected = reference.IsInstanceOfType(instance);

    // Act
    bool actual = sut.IsInstanceOfType(instance);

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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
    Type reference = _fixture.GetReference(category);

    SymbolType targetType = _fixture.GetDelegator(targetCategory);
    Type referenceTargetType = _fixture.GetReference(targetCategory);
    bool expected = reference.IsSubclassOf(referenceTargetType);
  
    // Act
    bool actual = sut.IsSubclassOf(targetType);
  
    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
  public void IsUnicodeClass_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    bool expected = reference.IsUnicodeClass;

    // Act
    bool actual = sut.IsUnicodeClass;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
  public void IsValueType_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    bool expected = reference.IsValueType;

    // Act
    bool actual = sut.IsValueType;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
  public void IsVisible_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    bool expected = reference.IsVisible;

    // Act
    bool actual = sut.IsVisible;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Fact]
  public void MakeGenericType_ShouldBeCorrect()
  {
    // Arrange
    Type[] types = new[] { typeof(int), typeof(string) };
    SymbolType sut = _fixture.GetDelegator(TypeCategory.OpenGeneric);
    Type reference = _fixture.GetReference(TypeCategory.OpenGeneric);
    Type expected = reference.MakeGenericType(types);
    
    // Act
    Type actual = sut.MakeGenericType(types);

    // Assert
    actual.Should().Equal(expected, TypeEqualityComparer.Default);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
  public void MemberType_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    MemberTypes expected = reference.MemberType;
    
    // Act
    MemberTypes actual = sut.MemberType;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
  public void TypeHandle_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    RuntimeTypeHandle expected = reference.TypeHandle;

    // Act
    RuntimeTypeHandle actual = sut.TypeHandle;

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [NamedTypesData, ArrayTypesData, TypeParametersData]
  public void TypeInitializer_ShouldBeCorrect(TypeCategory category)
  {
    // Arrange
    SymbolType sut = _fixture.GetDelegator(category);
    Type reference = _fixture.GetReference(category);

    ConstructorInfo? expected = reference.TypeInitializer;

    // Act
    ConstructorInfo? actual = sut.TypeInitializer;

    // Assert
    actual.Should().Equal(expected, ConstructorInfoEqualityComparer.Default);
  }

  [Theory]
  [NamedTypesData, SpecialTypesData, ArrayTypesData, TypeParametersData]
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
