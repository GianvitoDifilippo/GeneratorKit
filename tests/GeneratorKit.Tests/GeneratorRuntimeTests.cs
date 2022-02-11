﻿using FluentAssertions;
using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using Microsoft.CodeAnalysis;
using Moq;
using System;
using Xunit;
using static GeneratorKit.GeneratorRuntimeFixture;
using static GeneratorKit.TestHelpers.ProxyTypes;

namespace GeneratorKit;

public class GeneratorRuntimeTests : IClassFixture<GeneratorRuntimeFixture>
{
  private readonly GeneratorRuntimeFixture _fixture;
  private readonly Mock<IProxyManager> _proxyManagerMock;

  public GeneratorRuntimeTests(GeneratorRuntimeFixture fixture)
  {
    _fixture = fixture;
    _proxyManagerMock = new Mock<IProxyManager>(MockBehavior.Strict);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeIsNotGeneric()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);

    SymbolType sourceType = _fixture.GetSourceType(SourceType.NonGenericClass, sut);
    SymbolType type = sourceType;
    
    Type proxyType = typeof(NonGenericClassProxy);
    Type expected = typeof(NonGenericClassProxy);
    
    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(proxyType);

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeIsGeneric_AndIsGenericDefinition()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);
    
    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClass, sut);
    SymbolType type = sourceType;
    
    Type proxyType = typeof(GenericClassProxy<>);
    Type expected = typeof(GenericClassProxy<>);
    
    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(proxyType);

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeIsGeneric_AndIsConstructedGeneric()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClass, sut);
    SymbolType type = sourceType.MakeGenericType(_fixture.GetSpecialType(sut, SpecialType.System_Int32));

    Type proxyType = typeof(GenericClassProxy<>);
    Type expected = typeof(GenericClassProxy<int>);

    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(proxyType);

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeIsGeneric_AndIsHybridGeneric()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClass, sut);
    SymbolType type = sourceType.MakeGenericType(typeof(int));

    Type proxyType = typeof(GenericClassProxy<>);
    Type expected = typeof(GenericClassProxy<int>);

    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(proxyType);

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeIsNonGenericClassWithGenericBase()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);

    SymbolType sourceType = _fixture.GetSourceType(SourceType.NonGenericClassGenericBase, sut);
    SymbolType type = sourceType;

    Type proxyType = typeof(GenericClassProxy<>);
    Type expected = typeof(GenericClassProxy<int>);

    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(ProxyTypeBuilderInstantiation.Create(sourceType, sourceType.BaseType!, proxyType));

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeIsGenericClassWithLessTypeParametersThanBase_AndIsGenericDefinition()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClassLessParameters, sut);
    SymbolType type = sourceType;

    Type proxyType = typeof(GenericClassProxy<,>);
    Type expected = typeof(GenericClassProxy<,>).MakeGenericType(typeof(string), Type.MakeGenericMethodParameter(0));

    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(ProxyTypeBuilderInstantiation.Create(sourceType, sourceType.BaseType!, proxyType));

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeIsGenericClassWithLessTypeParametersThanBase_AndIsConstructedGeneric()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClassLessParameters, sut);
    SymbolType type = sourceType.MakeGenericType(_fixture.GetSpecialType(sut, SpecialType.System_Int32));

    Type proxyType = typeof(GenericClassProxy<,>);
    Type expected = typeof(GenericClassProxy<,>).MakeGenericType(typeof(string), typeof(int));

    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(ProxyTypeBuilderInstantiation.Create(sourceType, sourceType.BaseType!, proxyType));

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeIsGenericClassWithLessTypeParametersThanBase_AndIsHybridGeneric()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClassLessParameters, sut);
    SymbolType type = sourceType.MakeGenericType(typeof(int));

    Type proxyType = typeof(GenericClassProxy<,>);
    Type expected = typeof(GenericClassProxy<,>).MakeGenericType(typeof(string), typeof(int));

    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(ProxyTypeBuilderInstantiation.Create(sourceType, sourceType.BaseType!, proxyType));

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeIsGenericClassWithMoreTypeParametersThanBase1_AndIsGenericDefinition()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClassMoreParameters1, sut);
    SymbolType type = sourceType;

    Type proxyType = typeof(GenericClassProxy<,>);
    Type expected = typeof(GenericClassProxy<,>).MakeGenericType(Type.MakeGenericMethodParameter(2), Type.MakeGenericMethodParameter(0));

    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(ProxyTypeBuilderInstantiation.Create(sourceType, sourceType.BaseType!, proxyType));

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeIsGenericClassWithMoreTypeParametersThanBase1_AndIsConstructedGeneric()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClassMoreParameters1, sut);
    SymbolType type = sourceType.MakeGenericType(
      _fixture.GetSpecialType(sut, SpecialType.System_Byte),
      _fixture.GetSpecialType(sut, SpecialType.System_Int32),
      _fixture.GetSpecialType(sut, SpecialType.System_Char));

    Type proxyType = typeof(GenericClassProxy<,>);
    Type expected = typeof(GenericClassProxy<,>).MakeGenericType(typeof(char), typeof(byte));

    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(ProxyTypeBuilderInstantiation.Create(sourceType, sourceType.BaseType!, proxyType));

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeIsGenericClassWithMoreTypeParametersThanBase1_AndIsHybridGeneric()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClassMoreParameters1, sut);
    SymbolType type = sourceType.MakeGenericType(
      typeof(byte),
      typeof(int),
      typeof(char));

    Type proxyType = typeof(GenericClassProxy<,>);
    Type expected = typeof(GenericClassProxy<,>).MakeGenericType(typeof(char), typeof(byte));

    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(ProxyTypeBuilderInstantiation.Create(sourceType, sourceType.BaseType!, proxyType));

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeIsGenericClassWithMoreTypeParametersThanBase2_AndIsGenericDefinition()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClassMoreParameters2, sut);
    SymbolType type = sourceType;

    Type proxyType = typeof(GenericClassProxy<,>);
    Type expected = typeof(GenericClassProxy<,>).MakeGenericType(typeof(string), Type.MakeGenericMethodParameter(2));

    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(ProxyTypeBuilderInstantiation.Create(sourceType, sourceType.BaseType!, proxyType));

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeIsGenericClassWithMoreTypeParametersThanBase2_AndIsConstructedGeneric()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClassMoreParameters2, sut);
    SymbolType type = sourceType.MakeGenericType(
      _fixture.GetSpecialType(sut, SpecialType.System_Byte),
      _fixture.GetSpecialType(sut, SpecialType.System_Int32),
      _fixture.GetSpecialType(sut, SpecialType.System_Char));

    Type proxyType = typeof(GenericClassProxy<,>);
    Type expected = typeof(GenericClassProxy<,>).MakeGenericType(typeof(string), typeof(char));

    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(ProxyTypeBuilderInstantiation.Create(sourceType, sourceType.BaseType!, proxyType));

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeIsGenericClassWithMoreTypeParametersThanBase2_AndIsHybridGeneric()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClassMoreParameters2, sut);
    SymbolType type = sourceType.MakeGenericType(
      typeof(byte),
      typeof(int),
      typeof(char));

    Type proxyType = typeof(GenericClassProxy<,>);
    Type expected = typeof(GenericClassProxy<,>).MakeGenericType(typeof(string), typeof(char));

    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(ProxyTypeBuilderInstantiation.Create(sourceType, sourceType.BaseType!, proxyType));

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetRuntimeType_ShouldReturnEquivalentProxyType_WhenSourceTypeImplementsInterface()
  {
    // Arrange
    GeneratorRuntime sut = _fixture.CreateSut(_proxyManagerMock.Object);

    SymbolType sourceType = _fixture.GetSourceType(SourceType.Interface, sut);
    SymbolType type = sourceType;

    Type proxyType = typeof(InterfaceProxy);
    Type expected = typeof(InterfaceProxy);

    _proxyManagerMock
      .Setup(x => x.GetProxyType(sourceType))
      .Returns(proxyType);

    // Act
    Type actual = sut.GetRuntimeType(type);

    // Assert
    actual.Should().Equal(expected);
  }
}