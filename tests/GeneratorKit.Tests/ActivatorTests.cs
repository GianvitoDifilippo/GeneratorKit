using FluentAssertions;
using GeneratorKit.Interpret;
using GeneratorKit.Reflection;
using Moq;
using Xunit;
using System;
using GeneratorKit.Proxy;
using System.Collections.Generic;
using static GeneratorKit.TestHelpers.ProxyTypes;
using static GeneratorKit.ActivatorFixture;

namespace GeneratorKit;

public class ActivatorTests : IClassFixture<ActivatorFixture>
{
  private readonly ActivatorFixture _fixture;
  private readonly Mock<IInterpreter> _interpreterMock;
  private readonly Mock<IFrameProvider> _frameProviderMock;

  public ActivatorTests(ActivatorFixture fixture)
  {
    _fixture = fixture;
    _interpreterMock = new Mock<IInterpreter>(MockBehavior.Strict);
    _frameProviderMock = new Mock<IFrameProvider>(MockBehavior.Strict);
  }

  [Fact]
  public void CreateInstance_ShouldCreateInstanceOfNonGenericType_WhenParameterlessCtor()
  {
    // Arrange
    object?[] arguments = Array.Empty<object>();
    object?[] proxyArguments = Array.Empty<object>();

    SymbolType sourceType = _fixture.GetSourceType(SourceType.NonGenericClass);
    SymbolType type = sourceType;
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame classFrame = InterpreterFrame.NewClassFrame(null, new Dictionary<string, object?>(), Type.EmptyTypes);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _frameProviderMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _frameProviderMock
      .Setup(x => x.GetConstructorFrame(classFrame, constructor, arguments))
      .Returns(InterpreterFrame.NewMethodFrame(classFrame, new Dictionary<string, object?>(), Type.EmptyTypes));
    _frameProviderMock
      .Setup(x => x.GetInstanceFrame(classFrame, It.IsAny<object>()))
      .Returns<InterpreterFrame, object>((parent, instance) => InterpreterFrame.NewInstanceFrame(parent, new Dictionary<string, object?>(), instance));

    Activator sut = new Activator(_interpreterMock.Object, _frameProviderMock.Object);

    // Act
    NonGenericClass instance = sut.CreateInstance<NonGenericClass>(type, arguments);

    // Assert
    instance.Should().BeAssignableTo<IProxied>();
  }

  [Fact]
  public void CreateInstance_ShouldCreateInstanceOfNonGenericType_WhenFirstCtor()
  {
    // Arrange
    const int value1 = 10;
    object?[] arguments = new object?[1] { value1 };
    object?[] proxyArguments = new object?[1] { value1 };

    SymbolType sourceType = _fixture.GetSourceType(SourceType.NonGenericClassWithCtors);
    SymbolType type = sourceType;
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame classFrame = InterpreterFrame.NewClassFrame(null, new Dictionary<string, object?>(), Type.EmptyTypes);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _frameProviderMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _frameProviderMock
      .Setup(x => x.GetConstructorFrame(classFrame, constructor, arguments))
      .Returns(InterpreterFrame.NewMethodFrame(classFrame, new Dictionary<string, object?>(), Type.EmptyTypes));
    _frameProviderMock
      .Setup(x => x.GetInstanceFrame(classFrame, It.IsAny<object>()))
      .Returns<InterpreterFrame, object>((parent, instance) => InterpreterFrame.NewInstanceFrame(parent, new Dictionary<string, object?>(), instance));

    Activator sut = new Activator(_interpreterMock.Object, _frameProviderMock.Object);

    // Act
    NonGenericClassWithMembers instance = sut.CreateInstance<NonGenericClassWithMembers>(type, arguments);

    // Assert
    instance.Value1.Should().Be(value1);
    instance.Value2.Should().BeNull();
    instance.Should().BeAssignableTo<IProxied>();
  }

  [Fact]
  public void CreateInstance_ShouldCreateInstanceOfNonGenericType_WhenSecondCtor()
  {
    // Arrange
    const int value1 = 42;
    object?[] arguments = Array.Empty<object?>();
    object?[] proxyArguments = new object?[1] { value1 };

    SymbolType sourceType = _fixture.GetSourceType(SourceType.NonGenericClassWithCtors);
    SymbolType type = sourceType;
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[1];
    InterpreterFrame classFrame = InterpreterFrame.NewClassFrame(null, new Dictionary<string, object?>(), Type.EmptyTypes);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _frameProviderMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _frameProviderMock
      .Setup(x => x.GetConstructorFrame(classFrame, constructor, arguments))
      .Returns(InterpreterFrame.NewMethodFrame(classFrame, new Dictionary<string, object?>(), Type.EmptyTypes));
    _frameProviderMock
      .Setup(x => x.GetInstanceFrame(classFrame, It.IsAny<object>()))
      .Returns<InterpreterFrame, object>((parent, instance) => InterpreterFrame.NewInstanceFrame(parent, new Dictionary<string, object?>(), instance));

    Activator sut = new Activator(_interpreterMock.Object, _frameProviderMock.Object);

    // Act
    NonGenericClassWithMembers instance = sut.CreateInstance<NonGenericClassWithMembers>(type, arguments);

    // Assert
    instance.Value1.Should().Be(42);
    instance.Value2.Should().BeNull();
    instance.Should().BeAssignableTo<IProxied>();
  }

  [Fact]
  public void CreateInstance_ShouldCreateInstanceOfNonGenericType_WhenThirdCtor()
  {
    // Arrange
    const string value2 = "v2";
    object?[] arguments = new object?[2] { 30, value2 };
    object?[] proxyArguments = new object?[1] { value2 };

    SymbolType sourceType = _fixture.GetSourceType(SourceType.NonGenericClassWithCtors);
    SymbolType type = sourceType;
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[2];
    InterpreterFrame classFrame = InterpreterFrame.NewClassFrame(null, new Dictionary<string, object?>(), Type.EmptyTypes);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _frameProviderMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _frameProviderMock
      .Setup(x => x.GetConstructorFrame(classFrame, constructor, arguments))
      .Returns(InterpreterFrame.NewMethodFrame(classFrame, new Dictionary<string, object?>(), Type.EmptyTypes));
    _frameProviderMock
      .Setup(x => x.GetInstanceFrame(classFrame, It.IsAny<object>()))
      .Returns<InterpreterFrame, object>((parent, instance) => InterpreterFrame.NewInstanceFrame(parent, new Dictionary<string, object?>(), instance));

    Activator sut = new Activator(_interpreterMock.Object, _frameProviderMock.Object);

    // Act
    NonGenericClassWithMembers instance = sut.CreateInstance<NonGenericClassWithMembers>(type, arguments);

    // Assert
    instance.Value1.Should().Be(0);
    instance.Value2.Should().Be(value2);
    instance.Should().BeAssignableTo<IProxied>();
  }

  [Fact]
  public void CreateInstance_ShouldCreateInstanceOfGenericType_WhenParameterlessCtor()
  {
    // Arrange
    object?[] arguments = Array.Empty<object>();
    object?[] proxyArguments = Array.Empty<object>();
    Type[] typeArguments = new[] { typeof(string) };

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClass);
    HybridGenericType type = sourceType.MakeGenericType(typeArguments);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame classFrame = InterpreterFrame.NewClassFrame(null, new Dictionary<string, object?>(), typeArguments);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _frameProviderMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _frameProviderMock
      .Setup(x => x.GetConstructorFrame(classFrame, constructor, arguments))
      .Returns(InterpreterFrame.NewMethodFrame(classFrame, new Dictionary<string, object?>(), Type.EmptyTypes));
    _frameProviderMock
      .Setup(x => x.GetInstanceFrame(classFrame, It.IsAny<object>()))
      .Returns<InterpreterFrame, object>((parent, instance) => InterpreterFrame.NewInstanceFrame(parent, new Dictionary<string, object?>(), instance));

    Activator sut = new Activator(_interpreterMock.Object, _frameProviderMock.Object);

    // Act
    GenericClass<string> instance = sut.CreateInstance<GenericClass<string>>(type, arguments);

    // Assert
    instance.Should().BeAssignableTo<IProxied>();
  }

  [Fact]
  public void CreateInstance_ShouldCreateInstanceOfGenericType_WhenFirstCtor()
  {
    // Arrange
    const int value1 = 10;
    object?[] arguments = new object?[1] { value1 };
    object?[] proxyArguments = new object?[1] { value1 };
    Type[] typeArguments = new[] { typeof(string) };

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClassWithCtors);
    HybridGenericType type = sourceType.MakeGenericType(typeArguments);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame classFrame = InterpreterFrame.NewClassFrame(null, new Dictionary<string, object?>(), typeArguments);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _frameProviderMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _frameProviderMock
      .Setup(x => x.GetConstructorFrame(classFrame, constructor, arguments))
      .Returns(InterpreterFrame.NewMethodFrame(classFrame, new Dictionary<string, object?>(), Type.EmptyTypes));
    _frameProviderMock
      .Setup(x => x.GetInstanceFrame(classFrame, It.IsAny<object>()))
      .Returns<InterpreterFrame, object>((parent, instance) => InterpreterFrame.NewInstanceFrame(parent, new Dictionary<string, object?>(), instance));

    Activator sut = new Activator(_interpreterMock.Object, _frameProviderMock.Object);

    // Act
    GenericClassWithMembers<string> instance = sut.CreateInstance<GenericClassWithMembers<string>>(type, arguments);

    // Assert
    instance.Value1.Should().Be(value1);
    instance.Value2.Should().BeNull();
    instance.Should().BeAssignableTo<IProxied>();
  }

  [Fact]
  public void CreateInstance_ShouldCreateInstanceOfGenericType_WhenSecondCtor()
  {
    // Arrange
    const int value1 = 42;
    object?[] arguments = Array.Empty<object?>();
    object?[] proxyArguments = new object?[1] { value1 };
    Type[] typeArguments = new[] { typeof(string) };

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClassWithCtors);
    HybridGenericType type = sourceType.MakeGenericType(typeArguments);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[1];
    InterpreterFrame classFrame = InterpreterFrame.NewClassFrame(null, new Dictionary<string, object?>(), typeArguments);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _frameProviderMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _frameProviderMock
      .Setup(x => x.GetConstructorFrame(classFrame, constructor, arguments))
      .Returns(InterpreterFrame.NewMethodFrame(classFrame, new Dictionary<string, object?>(), Type.EmptyTypes));
    _frameProviderMock
      .Setup(x => x.GetInstanceFrame(classFrame, It.IsAny<object>()))
      .Returns<InterpreterFrame, object>((parent, instance) => InterpreterFrame.NewInstanceFrame(parent, new Dictionary<string, object?>(), instance));

    Activator sut = new Activator(_interpreterMock.Object, _frameProviderMock.Object);

    // Act
    GenericClassWithMembers<string> instance = sut.CreateInstance<GenericClassWithMembers<string>>(type, arguments);

    // Assert
    instance.Value1.Should().Be(42);
    instance.Value2.Should().BeNull();
    instance.Should().BeAssignableTo<IProxied>();
  }

  [Fact]
  public void CreateInstance_ShouldCreateInstanceOfGenericType_WhenThirdCtor()
  {
    // Arrange
    const string value2 = "v2";
    object?[] arguments = new object?[2] { 30, value2 };
    object?[] proxyArguments = new object?[1] { value2 };
    Type[] typeArguments = new[] { typeof(string) };

    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClassWithCtors);
    HybridGenericType type = sourceType.MakeGenericType(typeArguments);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[2];
    InterpreterFrame classFrame = InterpreterFrame.NewClassFrame(null, new Dictionary<string, object?>(), typeArguments);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _frameProviderMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _frameProviderMock
      .Setup(x => x.GetConstructorFrame(classFrame, constructor, arguments))
      .Returns(InterpreterFrame.NewMethodFrame(classFrame, new Dictionary<string, object?>(), Type.EmptyTypes));
    _frameProviderMock
      .Setup(x => x.GetInstanceFrame(classFrame, It.IsAny<object>()))
      .Returns<InterpreterFrame, object>((parent, instance) => InterpreterFrame.NewInstanceFrame(parent, new Dictionary<string, object?>(), instance));

    Activator sut = new Activator(_interpreterMock.Object, _frameProviderMock.Object);

    // Act
    GenericClassWithMembers<string> instance = sut.CreateInstance<GenericClassWithMembers<string>>(type, arguments);

    // Assert
    instance.Value1.Should().Be(0);
    instance.Value2.Should().Be(value2);
    instance.Should().BeAssignableTo<IProxied>();
  }

  [Fact]
  public void CreateInstance_ShouldCreateInstanceOfNonGenericTypeWithGenericBase_WhenFirstCtor()
  {
    // Arrange
    const int value1 = 10;
    object?[] arguments = new object?[1] { value1 };
    object?[] proxyArguments = new object?[1] { value1 };

    SymbolType sourceType = _fixture.GetSourceType(SourceType.NonGenericClassGenericBaseSource);
    SymbolType type = sourceType;
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame classFrame = InterpreterFrame.NewClassFrame(null, new Dictionary<string, object?>(), Type.EmptyTypes);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _frameProviderMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _frameProviderMock
      .Setup(x => x.GetConstructorFrame(classFrame, constructor, arguments))
      .Returns(InterpreterFrame.NewMethodFrame(classFrame, new Dictionary<string, object?>(), Type.EmptyTypes));
    _frameProviderMock
      .Setup(x => x.GetInstanceFrame(classFrame, It.IsAny<object>()))
      .Returns<InterpreterFrame, object>((parent, instance) => InterpreterFrame.NewInstanceFrame(parent, new Dictionary<string, object?>(), instance));

    Activator sut = new Activator(_interpreterMock.Object, _frameProviderMock.Object);

    // Act
    GenericClassWithMembers<string> instance = sut.CreateInstance<GenericClassWithMembers<string>>(type, arguments);

    // Assert
    instance.Value1.Should().Be(value1);
    instance.Value2.Should().BeNull();
    instance.Should().BeAssignableTo<IProxied>();
  }

  [Fact]
  public void CreateInstance_ShouldCreateInstanceOfNonGenericTypeWithGenericBase_WhenSecondCtor()
  {
    // Arrange
    const int value1 = 42;
    object?[] arguments = Array.Empty<object?>();
    object?[] proxyArguments = new object?[1] { value1 };

    SymbolType sourceType = _fixture.GetSourceType(SourceType.NonGenericClassGenericBaseSource);
    SymbolType type = sourceType;
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[1];
    InterpreterFrame classFrame = InterpreterFrame.NewClassFrame(null, new Dictionary<string, object?>(), Type.EmptyTypes);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _frameProviderMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _frameProviderMock
      .Setup(x => x.GetConstructorFrame(classFrame, constructor, arguments))
      .Returns(InterpreterFrame.NewMethodFrame(classFrame, new Dictionary<string, object?>(), Type.EmptyTypes));
    _frameProviderMock
      .Setup(x => x.GetInstanceFrame(classFrame, It.IsAny<object>()))
      .Returns<InterpreterFrame, object>((parent, instance) => InterpreterFrame.NewInstanceFrame(parent, new Dictionary<string, object?>(), instance));

    Activator sut = new Activator(_interpreterMock.Object, _frameProviderMock.Object);

    // Act
    GenericClassWithMembers<string> instance = sut.CreateInstance<GenericClassWithMembers<string>>(type, arguments);

    // Assert
    instance.Value1.Should().Be(42);
    instance.Value2.Should().BeNull();
    instance.Should().BeAssignableTo<IProxied>();
  }

  [Fact]
  public void CreateInstance_ShouldCreateInstanceOfNonGenericTypeWithGenericBase_WhenThirdCtor()
  {
    // Arrange
    const string value2 = "v2";
    object?[] arguments = new object?[2] { 30, value2 };
    object?[] proxyArguments = new object?[1] { value2 };

    SymbolType sourceType = _fixture.GetSourceType(SourceType.NonGenericClassGenericBaseSource);
    SymbolType type = sourceType;
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[2];
    InterpreterFrame classFrame = InterpreterFrame.NewClassFrame(null, new Dictionary<string, object?>(), Type.EmptyTypes);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _frameProviderMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _frameProviderMock
      .Setup(x => x.GetConstructorFrame(classFrame, constructor, arguments))
      .Returns(InterpreterFrame.NewMethodFrame(classFrame, new Dictionary<string, object?>(), Type.EmptyTypes));
    _frameProviderMock
      .Setup(x => x.GetInstanceFrame(classFrame, It.IsAny<object>()))
      .Returns<InterpreterFrame, object>((parent, instance) => InterpreterFrame.NewInstanceFrame(parent, new Dictionary<string, object?>(), instance));

    Activator sut = new Activator(_interpreterMock.Object, _frameProviderMock.Object);

    // Act
    GenericClassWithMembers<string> instance = sut.CreateInstance<GenericClassWithMembers<string>>(type, arguments);

    // Assert
    instance.Value1.Should().Be(0);
    instance.Value2.Should().Be(value2);
    instance.Should().BeAssignableTo<IProxied>();
  }

  [Fact]
  public void CreateInstance_ShouldCreateInstanceOfNonGenericTypeImplementingInterface_WhenFirstCtor()
  {
    // Arrange
    const int value1 = 10;
    object?[] arguments = new object?[1] { value1 };
    object?[] proxyArguments = Array.Empty<object?>();

    SymbolType sourceType = _fixture.GetSourceType(SourceType.Interface);
    SymbolType type = sourceType;
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame classFrame = InterpreterFrame.NewClassFrame(null, new Dictionary<string, object?>(), Type.EmptyTypes);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _frameProviderMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _frameProviderMock
      .Setup(x => x.GetConstructorFrame(classFrame, constructor, arguments))
      .Returns(InterpreterFrame.NewMethodFrame(classFrame, new Dictionary<string, object?>(), Type.EmptyTypes));
    _frameProviderMock
      .Setup(x => x.GetInstanceFrame(classFrame, It.IsAny<object>()))
      .Returns<InterpreterFrame, object>((parent, instance) => InterpreterFrame.NewInstanceFrame(parent, new Dictionary<string, object?>(), instance));

    Activator sut = new Activator(_interpreterMock.Object, _frameProviderMock.Object);

    // Act
    IInterface instance = sut.CreateInstance<IInterface>(type, arguments);

    // Assert
    instance.Should().BeAssignableTo<IProxied>();
  }

  [Fact]
  public void CreateInstance_ShouldCreateInstanceOfNonGenericTypeImplementingInterface_WhenSecondCtor()
  {
    // Arrange
    const string value2 = "v2";
    object?[] arguments = new object?[1] { value2 };
    object?[] proxyArguments = Array.Empty<object?>();

    SymbolType sourceType = _fixture.GetSourceType(SourceType.Interface);
    SymbolType type = sourceType;
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[1];
    InterpreterFrame classFrame = InterpreterFrame.NewClassFrame(null, new Dictionary<string, object?>(), Type.EmptyTypes);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _frameProviderMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _frameProviderMock
      .Setup(x => x.GetConstructorFrame(classFrame, constructor, arguments))
      .Returns(InterpreterFrame.NewMethodFrame(classFrame, new Dictionary<string, object?>(), Type.EmptyTypes));
    _frameProviderMock
      .Setup(x => x.GetInstanceFrame(classFrame, It.IsAny<object>()))
      .Returns<InterpreterFrame, object>((parent, instance) => InterpreterFrame.NewInstanceFrame(parent, new Dictionary<string, object?>(), instance));

    Activator sut = new Activator(_interpreterMock.Object, _frameProviderMock.Object);

    // Act
    IInterface instance = sut.CreateInstance<IInterface>(type, arguments);

    // Assert
    instance.Should().BeAssignableTo<IProxied>();
  }
}