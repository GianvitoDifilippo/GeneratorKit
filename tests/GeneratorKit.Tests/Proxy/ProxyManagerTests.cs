using FluentAssertions;
using GeneratorKit.TestHelpers;
using System;
using Xunit;
using static GeneratorKit.Proxy.ProxyManagerFixture;
using static GeneratorKit.TestHelpers.ProxyTypes;

namespace GeneratorKit.Proxy;

public class ProxyManagerTests : IClassFixture<ProxyManagerFixture>
{
  private readonly ProxyManagerFixture _fixture;

  public ProxyManagerTests(ProxyManagerFixture fixture)
  {
    _fixture = fixture;
  }

  [Fact]
  public void ProxyManager_ShouldRegisterNonGenericClassProxy()
  {
    // Arrange
    ProxyManager sut = new ProxyManager();
    Type proxyType = typeof(NonGenericClassProxy);
    Type expected = proxyType;

    // Act
    sut.RegisterProxyType(proxyType);
    Type actual = sut.GetProxyType(_fixture.GetSourceType(SourceType.NonGenericClass));

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void ProxyManager_ShouldRegisterNonGenericClassGenericBaseProxy()
  {
    // Arrange
    ProxyManager sut = new ProxyManager();
    Type proxyType = typeof(GenericClassProxy<>);
    Type expected = proxyType.MakeGenericType(typeof(int));

    // Act
    sut.RegisterProxyType(proxyType);
    Type actual = sut.GetProxyType(_fixture.GetSourceType(SourceType.NonGenericClassGenericBase));

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void ProxyManager_ShouldRegisterGenericClassProxy()
  {
    // Arrange
    ProxyManager sut = new ProxyManager();
    Type proxyType = typeof(GenericClassProxy<>);
    Type expected = proxyType;

    // Act
    sut.RegisterProxyType(proxyType);
    Type actual = sut.GetProxyType(_fixture.GetSourceType(SourceType.GenericClass));

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void ProxyManager_ShouldRegisterGenericClassLessParametersProxy()
  {
    // Arrange
    ProxyManager sut = new ProxyManager();
    Type proxyType = typeof(GenericClassProxy<,>);
    Type expected = proxyType.MakeGenericType(typeof(string), Type.MakeGenericMethodParameter(0));

    // Act
    sut.RegisterProxyType(proxyType);
    Type actual = sut.GetProxyType(_fixture.GetSourceType(SourceType.GenericClassLessParameters));

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void ProxyManager_ShouldRegisterGenericClassMoreParameters1Proxy()
  {
    // Arrange
    ProxyManager sut = new ProxyManager();
    Type proxyType = typeof(GenericClassProxy<,>);
    Type expected = proxyType.MakeGenericType(Type.MakeGenericMethodParameter(2), Type.MakeGenericMethodParameter(0));

    // Act
    sut.RegisterProxyType(proxyType);
    Type actual = sut.GetProxyType(_fixture.GetSourceType(SourceType.GenericClassMoreParameters1));

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void ProxyManager_ShouldRegisterGenericClassMoreParameters2Proxy()
  {
    // Arrange
    ProxyManager sut = new ProxyManager();
    Type proxyType = typeof(GenericClassProxy<,>);
    Type expected = proxyType.MakeGenericType(typeof(string), Type.MakeGenericMethodParameter(2));

    // Act
    sut.RegisterProxyType(proxyType);
    Type actual = sut.GetProxyType(_fixture.GetSourceType(SourceType.GenericClassMoreParameters2));

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void ProxyManager_ShouldRegisterInterfaceProxy()
  {
    // Arrange
    ProxyManager sut = new ProxyManager();
    Type proxyType = typeof(InterfaceProxy);
    Type expected = proxyType;

    // Act
    sut.RegisterProxyType(proxyType);
    Type actual = sut.GetProxyType(_fixture.GetSourceType(SourceType.Interface));

    // Assert
    actual.Should().Equal(expected);
  }
}