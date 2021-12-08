using FluentAssertions;
using GeneratorKit.Comparers;
using GeneratorKit.Emit;
using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using System;
using System.Reflection;
using Xunit;

namespace GeneratorKit;

public class ProxyTypeFactoryTests : IClassFixture<ProxyTypeFactoryFixture>
{
  private const BindingFlags s_allDeclared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
  private const string s_assemblyName = ProxyTypeFactoryFixture.AssemblyName;

  private readonly ProxyTypeFactoryFixture _fixture;

  public ProxyTypeFactoryTests(ProxyTypeFactoryFixture fixture)
  {
    _fixture = fixture;
  }

  [Theory]
  [AllCategoriesData]
  public void CreateProxyType_ShouldReturnRuntimeType(TypeCategory category)
  {
    // Arrange
    ProxyTypeFactory sut = new ProxyTypeFactory(s_assemblyName);

    SymbolType type = _fixture.GetSymbolType(category);

    // Act
    Type? actual = sut.CreateProxyType(_fixture.Runtime, type);

    // Assert
    actual!.Should().NotBeNull();
    actual!.GetType().FullName.Should().Be("System.RuntimeType");
    actual.Should().Equal(type, TypeEqualityComparer.Shallow);
  }

  [Theory]
  [AllCategoriesData]
  public void CreateProxyType_ShouldCreateTypeWithCorrectFields(TypeCategory category)
  {
    // Arrange
    ProxyTypeFactory sut = new ProxyTypeFactory(s_assemblyName);

    SymbolType type = _fixture.GetSymbolType(category);
    FieldInfo[] expected = type.GetFields(s_allDeclared);
    
    // Act
    FieldInfo[] actual = sut.CreateProxyType(_fixture.Runtime, type)!.GetFields(s_allDeclared);

    // Assert
    actual.Should().BeEquivalentTo(expected, FieldInfoEqualityComparer.Shallow);
  }

  [Theory]
  [AllCategoriesData]
  public void CreateProxyType_ShouldCreateTypeWithCorrectProperties(TypeCategory category)
  {
    // Arrange
    ProxyTypeFactory sut = new ProxyTypeFactory(s_assemblyName);

    SymbolType type = _fixture.GetSymbolType(category);
    PropertyInfo[] expected = type.GetProperties(s_allDeclared);

    // Act
    PropertyInfo[] actual = sut.CreateProxyType(_fixture.Runtime, type)!.GetProperties(s_allDeclared);

    // Assert
    actual.Should().BeEquivalentTo(expected, PropertyInfoEqualityComparer.Shallow);
  }

  [Theory]
  [AllCategoriesData]
  public void CreateProxyType_ShouldCreateTypeWithCorrectMethods(TypeCategory category)
  {
    // Arrange
    ProxyTypeFactory sut = new ProxyTypeFactory(s_assemblyName);

    SymbolType type = _fixture.GetSymbolType(category);
    MethodInfo[] expected = type.GetMethods(s_allDeclared);

    // Act
    MethodInfo[] actual = sut.CreateProxyType(_fixture.Runtime, type)!.GetMethods(s_allDeclared);

    // Assert
    actual.Should().BeEquivalentTo(expected, MethodInfoEqualityComparer.Shallow);
  }

  [Theory]
  [AllCategoriesData]
  public void CreateProxyType_ShouldCreateTypeWithCorrectConstructors(TypeCategory category)
  {
    // Arrange
    ProxyTypeFactory sut = new ProxyTypeFactory(s_assemblyName);

    SymbolType type = _fixture.GetSymbolType(category);
    ConstructorInfo[] expected = type.GetConstructors(s_allDeclared);

    // Act
    ConstructorInfo[] actual = sut.CreateProxyType(_fixture.Runtime, type)!.GetConstructors(s_allDeclared);

    // Assert
    actual.Should().BeEquivalentTo(expected, ConstructorInfoEqualityComparer.Shallow);
  }
}