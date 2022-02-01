using FluentAssertions;
using GeneratorKit.Interpret;
using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using Moq;
using System;
using Xunit;
using static GeneratorKit.ActivatorFixture;
using static GeneratorKit.TestHelpers.ProxyTypes;

namespace GeneratorKit;

public class ActivatorTests : IClassFixture<ActivatorFixture>
{
  private readonly ActivatorFixture _fixture;
  private readonly Mock<IInterpreter> _interpreterMock;

  public ActivatorTests(ActivatorFixture fixture)
  {
    _fixture = fixture;
    _interpreterMock = new Mock<IInterpreter>(MockBehavior.Strict);

    _interpreterMock
      .Setup(x => x.Interpret(It.IsAny<IRuntimeConstructor>(), It.IsAny<InterpreterFrame>(), It.IsAny<object?[]>()));
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
    InterpreterFrame classFrame = GetClassFrame();

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _interpreterMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _interpreterMock
      .Setup(x => x.GetInstanceFrame(classFrame, type, It.IsAny<object>()))
      .Returns<InterpreterFrame, IRuntimeType, object>(GetInstanceFrame);

    Activator sut = new Activator(_interpreterMock.Object);

    // Act
    object instance = sut.CreateInstance(type, arguments);

    // Assert
    instance.Should().BeAssignableTo<IProxied>();
    instance.Should().BeAssignableTo<NonGenericClass>();
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
    InterpreterFrame classFrame = GetClassFrame();

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _interpreterMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _interpreterMock
      .Setup(x => x.GetInstanceFrame(classFrame, type, It.IsAny<object>()))
      .Returns<InterpreterFrame, IRuntimeType, object>(GetInstanceFrame);

    Activator sut = new Activator(_interpreterMock.Object);

    // Act
    object instance = sut.CreateInstance(type, arguments);

    // Assert
    NonGenericClassWithMembers typedInstance = instance.Should().BeAssignableTo<NonGenericClassWithMembers>().Subject;
    typedInstance.Value1.Should().Be(value1);
    typedInstance.Value2.Should().BeNull();
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
    InterpreterFrame classFrame = GetClassFrame();

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _interpreterMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _interpreterMock
      .Setup(x => x.GetInstanceFrame(classFrame, type, It.IsAny<object>()))
      .Returns<InterpreterFrame, IRuntimeType, object>(GetInstanceFrame);

    Activator sut = new Activator(_interpreterMock.Object);

    // Act
    object instance = sut.CreateInstance(type, arguments);

    // Assert
    NonGenericClassWithMembers typedInstance = instance.Should().BeAssignableTo<NonGenericClassWithMembers>().Subject;
    typedInstance.Value1.Should().Be(42);
    typedInstance.Value2.Should().BeNull();
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
    InterpreterFrame classFrame = GetClassFrame();

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _interpreterMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _interpreterMock
      .Setup(x => x.GetInstanceFrame(classFrame, type, It.IsAny<object>()))
      .Returns<InterpreterFrame, IRuntimeType, object>(GetInstanceFrame);

    Activator sut = new Activator(_interpreterMock.Object);

    // Act
    object instance = sut.CreateInstance(type, arguments);

    // Assert
    NonGenericClassWithMembers typedInstance = instance.Should().BeAssignableTo<NonGenericClassWithMembers>().Subject;
    typedInstance.Value1.Should().Be(0);
    typedInstance.Value2.Should().Be(value2);
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
    InterpreterFrame classFrame = GetClassFrame(typeArguments);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _interpreterMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _interpreterMock
      .Setup(x => x.GetInstanceFrame(classFrame, type, It.IsAny<object>()))
      .Returns<InterpreterFrame, IRuntimeType, object>(GetInstanceFrame);

    Activator sut = new Activator(_interpreterMock.Object);

    // Act
    object instance = sut.CreateInstance(type, arguments);

    // Assert
    instance.Should().BeAssignableTo<GenericClass<string>>();
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
    InterpreterFrame classFrame = GetClassFrame(typeArguments);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _interpreterMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _interpreterMock
      .Setup(x => x.GetInstanceFrame(classFrame, type, It.IsAny<object>()))
      .Returns<InterpreterFrame, IRuntimeType, object>(GetInstanceFrame);

    Activator sut = new Activator(_interpreterMock.Object);

    // Act
    object instance = sut.CreateInstance(type, arguments);

    // Assert
    GenericClassWithMembers<string> typedInstance = instance.Should().BeAssignableTo<GenericClassWithMembers<string>>().Subject;
    typedInstance.Value1.Should().Be(value1);
    typedInstance.Value2.Should().BeNull();
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
    InterpreterFrame classFrame = GetClassFrame(typeArguments);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _interpreterMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _interpreterMock
      .Setup(x => x.GetInstanceFrame(classFrame, type, It.IsAny<object>()))
      .Returns<InterpreterFrame, IRuntimeType, object>(GetInstanceFrame);

    Activator sut = new Activator(_interpreterMock.Object);

    // Act
    object instance = sut.CreateInstance(type, arguments);

    // Assert
    GenericClassWithMembers<string> typedInstance = instance.Should().BeAssignableTo<GenericClassWithMembers<string>>().Subject;
    typedInstance.Value1.Should().Be(42);
    typedInstance.Value2.Should().BeNull();
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
    InterpreterFrame classFrame = GetClassFrame(typeArguments);

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _interpreterMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _interpreterMock
      .Setup(x => x.GetInstanceFrame(classFrame, type, It.IsAny<object>()))
      .Returns<InterpreterFrame, IRuntimeType, object>(GetInstanceFrame);

    Activator sut = new Activator(_interpreterMock.Object);

    // Act
    object instance = sut.CreateInstance(type, arguments);

    // Assert
    GenericClassWithMembers<string> typedInstance = instance.Should().BeAssignableTo<GenericClassWithMembers<string>>().Subject;
    typedInstance.Value1.Should().Be(0);
    typedInstance.Value2.Should().Be(value2);
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
    InterpreterFrame classFrame = GetClassFrame();

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _interpreterMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _interpreterMock
      .Setup(x => x.GetInstanceFrame(classFrame, type, It.IsAny<object>()))
      .Returns<InterpreterFrame, IRuntimeType, object>(GetInstanceFrame);

    Activator sut = new Activator(_interpreterMock.Object);

    // Act
    object instance = sut.CreateInstance(type, arguments);

    // Assert
    GenericClassWithMembers<string> typedInstance = instance.Should().BeAssignableTo<GenericClassWithMembers<string>>().Subject;
    typedInstance.Value1.Should().Be(value1);
    typedInstance.Value2.Should().BeNull();
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
    InterpreterFrame classFrame = GetClassFrame();

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _interpreterMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _interpreterMock
      .Setup(x => x.GetInstanceFrame(classFrame, type, It.IsAny<object>()))
      .Returns<InterpreterFrame, IRuntimeType, object>(GetInstanceFrame);

    Activator sut = new Activator(_interpreterMock.Object);

    // Act
    object instance = sut.CreateInstance(type, arguments);

    // Assert
    GenericClassWithMembers<string> typedInstance = instance.Should().BeAssignableTo<GenericClassWithMembers<string>>().Subject;
    typedInstance.Value1.Should().Be(42);
    typedInstance.Value2.Should().BeNull();
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
    InterpreterFrame classFrame = GetClassFrame();

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _interpreterMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _interpreterMock
      .Setup(x => x.GetInstanceFrame(classFrame, type, It.IsAny<object>()))
      .Returns<InterpreterFrame, IRuntimeType, object>(GetInstanceFrame);

    Activator sut = new Activator(_interpreterMock.Object);

    // Act
    object instance = sut.CreateInstance(type, arguments);

    // Assert
    GenericClassWithMembers<string> typedInstance = instance.Should().BeAssignableTo<GenericClassWithMembers<string>>().Subject;
    typedInstance.Value1.Should().Be(0);
    typedInstance.Value2.Should().Be(value2);
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
    InterpreterFrame classFrame = GetClassFrame();

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _interpreterMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _interpreterMock
      .Setup(x => x.GetInstanceFrame(classFrame, type, It.IsAny<object>()))
      .Returns<InterpreterFrame, IRuntimeType, object>(GetInstanceFrame);

    Activator sut = new Activator(_interpreterMock.Object);

    // Act
    object instance = sut.CreateInstance(type, arguments);

    // Assert
    instance.Should().BeAssignableTo<IInterface>();
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
    InterpreterFrame classFrame = GetClassFrame();

    _interpreterMock
      .Setup(x => x.GetProxyArguments(constructor, It.IsAny<InterpreterFrame>(), arguments))
      .Returns(proxyArguments);
    _interpreterMock
      .Setup(x => x.GetClassFrame(type))
      .Returns(classFrame);
    _interpreterMock
      .Setup(x => x.GetInstanceFrame(classFrame, type, It.IsAny<object>()))
      .Returns<InterpreterFrame, IRuntimeType, object>(GetInstanceFrame);

    Activator sut = new Activator(_interpreterMock.Object);

    // Act
    object instance = sut.CreateInstance(type, arguments);

    // Assert
    instance.Should().BeAssignableTo<IInterface>();
    instance.Should().BeAssignableTo<IProxied>();
  }
}