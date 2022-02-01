using FluentAssertions;
using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using System;
using Xunit;
using static GeneratorKit.Interpret.InterpreterFixture;

namespace GeneratorKit.Interpret;

public class InterpreterTests : IClassFixture<InterpreterFixture>
{
  private readonly InterpreterFixture _fixture;
  private readonly FakeFrameDictionaryProvider _dictionaryProvider;
  private readonly FrameProvider _frameProvider;
  private readonly Interpreter _sut;
  private readonly GeneratorRuntime _runtime;

  public InterpreterTests(InterpreterFixture fixture)
  {
    _fixture = fixture;
    _dictionaryProvider = new FakeFrameDictionaryProvider();
    _frameProvider = new FrameProvider(_dictionaryProvider);
    _sut = _fixture.CreateSut(_frameProvider, out _runtime);
  }

  [Fact]
  public void Interpret_ShouldInterpret_SimpleAssignment()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.SimpleAssignment);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _frameProvider, _sut, InterpretedOperationType.SimpleAssignment);

    // Act
    _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    _dictionaryProvider.Values.Should().HaveCount(1);
    _dictionaryProvider.Values.Should().Contain("v1", 1);
  }

  [Fact]
  public void Interpret_ShouldInterpret_TupleAssignment()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.TupleAssignment);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _frameProvider, _sut, InterpretedOperationType.TupleAssignment);

    // Act
    _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    _dictionaryProvider.Values.Should().HaveCount(2);
    _dictionaryProvider.Values.Should().Contain("v1", 2);
    _dictionaryProvider.Values.Should().Contain("v2", "str");
  }

  [Fact]
  public void Interpret_ShouldInterpret_NameOf()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    const string expected = "NonGenericClass_Interpret";
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.NameOf);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _frameProvider, _sut, InterpretedOperationType.NameOf);

    // Act
    object? actual = _sut.Interpret(method, instanceFrame, arguments);

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
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.IfElse);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _frameProvider, _sut, InterpretedOperationType.IfElse);

    // Act
    object? actual = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_For()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.For);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _frameProvider, _sut, InterpretedOperationType.For);

    // Act
    object? actual = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(10);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Foreach()
  {
    // Arrange
    object?[] arguments = new object?[] { new int[] { 1, 2, 3 } };
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.ForEach);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _frameProvider, _sut, InterpretedOperationType.ForEach);

    // Act
    object? actual = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(6);
  }

  [Theory]
  [InlineData(typeof(string), 6)]
  [InlineData(typeof(char), 4)]
  public void Interpret_ShouldInterpret_Invocation_NonGenericMethod_GenericType(Type typeArgument, int expected)
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Invocation_NonGenericMethod_GenericType);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _frameProvider, _sut, InterpretedOperationType.Invocation_NonGenericMethod_GenericType, typeArgument);

    // Act
    object? actual = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Invocation_GenericMethod_GenericType1()
  {
    // Arrange
    const int expected = 6;
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Invocation_GenericMethod_GenericType1);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _frameProvider, _sut, InterpretedOperationType.Invocation_GenericMethod_GenericType1, typeof(int));

    // Act
    object? actual = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Invocation_GenericMethod_GenericType2()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Invocation_GenericMethod_GenericType2);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _frameProvider, _sut, InterpretedOperationType.Invocation_GenericMethod_GenericType2, typeof(int));

    // Act
    object? actual = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Invocation_StaticMethod()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Invocation_StaticMethod);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _frameProvider, _sut, InterpretedOperationType.Invocation_StaticMethod, typeof(int));

    // Act
    object? actual = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Property_InstanceProperty()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Property_InstanceProperty);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _frameProvider, _sut, InterpretedOperationType.Property_InstanceProperty, typeof(int));

    // Act
    object? actual = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Property_StaticProperty()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Property_StaticProperty);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _frameProvider, _sut, InterpretedOperationType.Property_StaticProperty, typeof(int));

    // Act
    object? actual = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    actual.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_GenericObjectCreation()
  {
    // Arrange
    const string arg1 = "argument1";
    object?[] arguments = new object?[] { arg1 };
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.GenericObjectCreation);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _frameProvider, _sut, InterpretedOperationType.GenericObjectCreation, typeof(string));

    // Act
    _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    _dictionaryProvider.Values.Should().Contain("value1", arg1);
    _dictionaryProvider.Values.Should().Contain("value2", 4);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForNonGenericType_WhenFirstCtor()
  {
    // Arrange
    const int value1 = 10;
    SymbolType sourceType = _fixture.GetSourceType(_runtime, SourceType.NonGenericClassWithCtors);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame classFrame = GetClassFrame(_frameProvider, sourceType);
    object?[] arguments = new object?[] { value1 };

    object?[] expected = new object?[] { value1 };

    // Act
    object?[] actual = _sut.GetProxyArguments(constructor, classFrame, arguments);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForNonGenericType_WhenSecondCtor()
  {
    // Arrange
    const int value1 = 42;
    SymbolType sourceType = _fixture.GetSourceType(_runtime, SourceType.NonGenericClassWithCtors);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[1];
    InterpreterFrame classFrame = GetClassFrame(_frameProvider, sourceType);
    object?[] arguments = Array.Empty<object?>();

    object?[] expected = new object?[] { value1 };

    // Act
    object?[] actual = _sut.GetProxyArguments(constructor, classFrame, arguments);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForNonGenericType_WhenThirdCtor()
  {
    // Arrange
    const int value1 = 10;
    const string value2 = "v2";
    SymbolType sourceType = _fixture.GetSourceType(_runtime, SourceType.NonGenericClassWithCtors);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[2];
    InterpreterFrame classFrame = GetClassFrame(_frameProvider, sourceType);
    object?[] arguments = new object?[] { value1, value2 };

    object?[] expected = new object?[] { value2 };

    // Act
    object?[] actual = _sut.GetProxyArguments(constructor, classFrame, arguments);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForGenericType()
  {
    // Arrange
    const int value1 = 10;
    const string value2 = "v2";
    SymbolType sourceType = _fixture.GetSourceType(_runtime, SourceType.GenericClassWithCtors);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame classFrame = GetClassFrame(_frameProvider, sourceType, typeof(string));
    object?[] arguments = new object?[] { value1, value2 };

    object?[] expected = new object?[] { value2 };

    // Act
    object?[] actual = _sut.GetProxyArguments(constructor, classFrame, arguments);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForInterfaceType()
  {
    // Arrange
    const int value1 = 10;
    SymbolType sourceType = _fixture.GetSourceType(_runtime, SourceType.Interface);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame classFrame = GetClassFrame(_frameProvider, sourceType);
    object?[] arguments = new object?[] { value1 };

    object?[] expected = Array.Empty<object?>();

    // Act
    object?[] actual = _sut.GetProxyArguments(constructor, classFrame, arguments);

    // Assert
    actual.Should().Equal(expected);
  }
}
