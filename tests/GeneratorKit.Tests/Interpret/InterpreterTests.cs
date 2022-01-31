using FluentAssertions;
using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using Moq;
using System;
using Xunit;
using static GeneratorKit.Interpret.InterpreterFixture;

namespace GeneratorKit.Interpret;

public class InterpreterTests : IClassFixture<InterpreterFixture>
{
  private readonly InterpreterFixture _fixture;
  private readonly FakeFrameProvider _frameProvider;

  public InterpreterTests(InterpreterFixture fixture)
  {
    _fixture = fixture;
    _frameProvider = new FakeFrameProvider();
  }

  [Fact]
  public void Interpret_ShouldInterpret_SimpleAssignment()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    Interpreter sut = _fixture.CreateSut(_frameProvider);
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(InterpretedOperationType.SimpleAssignment);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_frameProvider, sut, InterpretedOperationType.SimpleAssignment);

    // Act
    sut.Interpret(method, instanceFrame, arguments);

    // Assert
    _frameProvider.Values.Should().HaveCount(1);
    _frameProvider.Values.Should().Contain("v1", 1);
  }

  [Fact]
  public void Interpret_ShouldInterpret_TupleAssignment()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    Interpreter sut = _fixture.CreateSut(_frameProvider);
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(InterpretedOperationType.TupleAssignment);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_frameProvider, sut, InterpretedOperationType.TupleAssignment);

    // Act
    sut.Interpret(method, instanceFrame, arguments);

    // Assert
    _frameProvider.Values.Should().HaveCount(2);
    _frameProvider.Values.Should().Contain("v1", 2);
    _frameProvider.Values.Should().Contain("v2", "str");
  }

  [Fact]
  public void Interpret_ShouldInterpret_NameOf()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    const string expected = "NonGenericClass_Interpret";
    Interpreter sut = _fixture.CreateSut(_frameProvider);
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(InterpretedOperationType.NameOf);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_frameProvider, sut, InterpretedOperationType.NameOf);

    // Act
    object? actual = sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [InlineData(0, "zero")]
  [InlineData(5, "not zero")]
  public void Interpret_ShouldInterpret_IfElse(int arg, string expected)
  {
    // Arrange
    object?[] arguments = new object?[] { arg };
    Interpreter sut = _fixture.CreateSut(_frameProvider);
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(InterpretedOperationType.IfElse);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_frameProvider, sut, InterpretedOperationType.IfElse);

    // Act
    object? actual = sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Theory]
  [InlineData(typeof(string), 6)]
  [InlineData(typeof(char), 4)]
  public void Interpret_ShouldInterpret_Invocation_NonGenericMethod_GenericType(Type typeArgument, int expected)
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    Interpreter sut = _fixture.CreateSut(_frameProvider);
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(InterpretedOperationType.Invocation_NonGenericMethod_GenericType);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_frameProvider, sut, InterpretedOperationType.Invocation_NonGenericMethod_GenericType, typeArgument);

    // Act
    object? actual = sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Invocation_GenericMethod_GenericType1()
  {
    // Arrange
    const int expected = 6;
    object?[] arguments = Array.Empty<object?>();
    Interpreter sut = _fixture.CreateSut(_frameProvider);
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(InterpretedOperationType.Invocation_GenericMethod_GenericType1);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_frameProvider, sut, InterpretedOperationType.Invocation_GenericMethod_GenericType1, typeof(int));

    // Act
    object? actual = sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Invocation_GenericMethod_GenericType2()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    Interpreter sut = _fixture.CreateSut(_frameProvider);
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(InterpretedOperationType.Invocation_GenericMethod_GenericType2);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_frameProvider, sut, InterpretedOperationType.Invocation_GenericMethod_GenericType2, typeof(int));

    // Act
    object? actual = sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Invocation_StaticMethod()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    Interpreter sut = _fixture.CreateSut(_frameProvider);
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(InterpretedOperationType.Invocation_StaticMethod);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_frameProvider, sut, InterpretedOperationType.Invocation_StaticMethod, typeof(int));

    // Act
    object? actual = sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Property_InstanceProperty()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    Interpreter sut = _fixture.CreateSut(_frameProvider);
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(InterpretedOperationType.Property_InstanceProperty);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_frameProvider, sut, InterpretedOperationType.Property_InstanceProperty, typeof(int));

    // Act
    object? actual = sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Property_StaticProperty()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    Interpreter sut = _fixture.CreateSut(_frameProvider);
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(InterpretedOperationType.Property_StaticProperty);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_frameProvider, sut, InterpretedOperationType.Property_StaticProperty, typeof(int));

    // Act
    object? actual = sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForNonGenericType_WhenFirstCtor()
  {
    // Arrange
    const int value1 = 10;
    SymbolType sourceType = _fixture.GetSourceType(SourceType.NonGenericClassWithCtors);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame classFrame = _fixture.GetClassFrame(_frameProvider, sourceType);
    object?[] arguments = new object?[] { value1 };

    Interpreter sut = _fixture.CreateSut(_frameProvider);
    object?[] expected = new object?[] { value1 };

    // Act
    object?[] actual = sut.GetProxyArguments(constructor, classFrame, arguments);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForNonGenericType_WhenSecondCtor()
  {
    // Arrange
    const int value1 = 42;
    SymbolType sourceType = _fixture.GetSourceType(SourceType.NonGenericClassWithCtors);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[1];
    InterpreterFrame classFrame = _fixture.GetClassFrame(_frameProvider, sourceType);
    object?[] arguments = Array.Empty<object?>();

    Interpreter sut = _fixture.CreateSut(_frameProvider);
    object?[] expected = new object?[] { value1 };

    // Act
    object?[] actual = sut.GetProxyArguments(constructor, classFrame, arguments);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForNonGenericType_WhenThirdCtor()
  {
    // Arrange
    const int value1 = 10;
    const string value2 = "v2";
    SymbolType sourceType = _fixture.GetSourceType(SourceType.NonGenericClassWithCtors);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[2];
    InterpreterFrame classFrame = _fixture.GetClassFrame(_frameProvider, sourceType);
    object?[] arguments = new object?[] { value1, value2 };

    Interpreter sut = _fixture.CreateSut(_frameProvider);
    object?[] expected = new object?[] { value2 };

    // Act
    object?[] actual = sut.GetProxyArguments(constructor, classFrame, arguments);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForGenericType()
  {
    // Arrange
    const int value1 = 10;
    const string value2 = "v2";
    SymbolType sourceType = _fixture.GetSourceType(SourceType.GenericClassWithCtors);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame classFrame = _fixture.GetClassFrame(_frameProvider, sourceType, typeof(string));
    object?[] arguments = new object?[] { value1, value2 };

    Interpreter sut = _fixture.CreateSut(_frameProvider);
    object?[] expected = new object?[] { value2 };

    // Act
    object?[] actual = sut.GetProxyArguments(constructor, classFrame, arguments);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForInterfaceType()
  {
    // Arrange
    const int value1 = 10;
    SymbolType sourceType = _fixture.GetSourceType(SourceType.Interface);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame classFrame = _fixture.GetClassFrame(_frameProvider, sourceType);
    object?[] arguments = new object?[] { value1 };

    Interpreter sut = _fixture.CreateSut(_frameProvider);
    object?[] expected = Array.Empty<object?>();

    // Act
    object?[] actual = sut.GetProxyArguments(constructor, classFrame, arguments);

    // Assert
    actual.Should().Equal(expected);
  }
}
