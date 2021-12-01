using FluentAssertions;
using GeneratorKit.Comparers;
using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using System;
using System.Reflection;
using Xunit;

namespace GeneratorKit.Proxy;

public class ProxyTypeFactoryTests : IClassFixture<ProxyTypeFactoryFixture>
{
  private const BindingFlags s_allDeclared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
  private const string s_assemblyName = ProxyTypeFactoryFixture.AssemblyName;

  private readonly ProxyTypeFactoryFixture _fixture;

  public ProxyTypeFactoryTests(ProxyTypeFactoryFixture fixture)
  {
    _fixture = fixture;
  }

  [Fact]
  public void CreateProxyType_ShouldReturnRuntimeType()
  {
    // Arrange
    ProxyTypeFactory sut = new ProxyTypeFactory(s_assemblyName);

    SymbolType type = _fixture.GetSymbolType(TypeCategory.Class);

    // Act
    Type? actual = sut.CreateProxyType(_fixture.Runtime, type);

    // Assert
    actual!.Should().NotBeNull();
    actual!.GetType().FullName.Should().Be("System.RuntimeType");
    actual.Should().Equal(type, TypeEqualityComparer.Shallow);
  }

  [Fact]
  public void CreateProxyType_ShouldCreateTypeWithCorrectFields()
  {
    // Arrange
    ProxyTypeFactory sut = new ProxyTypeFactory(s_assemblyName);

    SymbolType type = _fixture.GetSymbolType(TypeCategory.Class);
    FieldInfo[] expected = type.GetFields(s_allDeclared);
    
    // Act
    FieldInfo[] actual = sut.CreateProxyType(_fixture.Runtime, type)!.GetFields(s_allDeclared);

    // Assert
    actual.Should().BeEquivalentTo(expected, FieldInfoEqualityComparer.Shallow);
  }

  [Fact]
  public void CreateProxyType_ShouldCreateTypeWithCorrectProperties()
  {
    // Arrange
    ProxyTypeFactory sut = new ProxyTypeFactory(s_assemblyName);

    SymbolType type = _fixture.GetSymbolType(TypeCategory.Class);
    PropertyInfo[] expected = type.GetProperties(s_allDeclared);

    // Act
    PropertyInfo[] actual = sut.CreateProxyType(_fixture.Runtime, type)!.GetProperties(s_allDeclared);

    // Assert
    actual.Should().BeEquivalentTo(expected, PropertyInfoEqualityComparer.Shallow);
  }

  [Fact]
  public void CreateProxyType_ShouldCreateTypeWithCorrectMethods()
  {
    // Arrange
    ProxyTypeFactory sut = new ProxyTypeFactory(s_assemblyName);

    SymbolType type = _fixture.GetSymbolType(TypeCategory.Class);
    MethodInfo[] expected = type.GetMethods(s_allDeclared);

    // Act
    MethodInfo[] actual = sut.CreateProxyType(_fixture.Runtime, type)!.GetMethods(s_allDeclared);

    // Assert
    actual.Should().BeEquivalentTo(expected, MethodInfoEqualityComparer.Shallow);
  }

  [Fact]
  public void CreateProxyType_ShouldCreateTypeWithCorrectConstructors()
  {
    // Arrange
    ProxyTypeFactory sut = new ProxyTypeFactory(s_assemblyName);

    SymbolType type = _fixture.GetSymbolType(TypeCategory.Class);
    ConstructorInfo[] expected = type.GetConstructors(s_allDeclared);

    // Act
    ConstructorInfo[] actual = sut.CreateProxyType(_fixture.Runtime, type)!.GetConstructors(s_allDeclared);

    // Assert
    actual.Should().BeEquivalentTo(expected, ConstructorInfoEqualityComparer.Shallow);
  }
}