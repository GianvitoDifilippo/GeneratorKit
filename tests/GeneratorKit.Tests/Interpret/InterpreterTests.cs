#pragma warning disable RS1024 // Compare symbols correctly

using FluentAssertions;
using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using Xunit;
using static GeneratorKit.Interpret.InterpreterFixture;

namespace GeneratorKit.Interpret;

public class InterpreterTests : IClassFixture<InterpreterFixture>
{
  private readonly InterpreterFixture _fixture;
  private readonly FakeFrameProvider _frameProvider;
  private readonly Interpreter _sut;
  private readonly GeneratorRuntime _runtime;

  public InterpreterTests(InterpreterFixture fixture)
  {
    _fixture = fixture;
    _frameProvider = new FakeFrameProvider();
    _sut = _fixture.CreateSut(_frameProvider, out _runtime);
  }

  [Fact]
  public void Interpret_ShouldInterpret_SimpleAssignment()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.SimpleAssignment);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.SimpleAssignment);

    // Act
    _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    _frameProvider[2].Should().HaveCount(1);
    _frameProvider[2].Should().Contain("v1", 1);
  }

  [Fact]
  public void Interpret_ShouldInterpret_TupleAssignment()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.TupleAssignment);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.TupleAssignment);

    // Act
    _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    _frameProvider[2].Should().HaveCount(2);
    _frameProvider[2].Should().Contain("v1", 2);
    _frameProvider[2].Should().Contain("v2", "str");
  }

  [Fact]
  public void Interpret_ShouldInterpret_NameOf()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    const string expected = "NonGenericClass_Interpret";
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.NameOf);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.NameOf);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Theory]
  [InlineData(0, "zero")]
  [InlineData(5, "not zero")]
  public void Interpret_ShouldInterpret_IfElse(int arg, string expected)
  {
    // Arrange
    object?[] arguments = new object?[] { arg };
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.IfElse);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.IfElse);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_For()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.For);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.For);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(10);
  }

  [Fact]
  public void Interpret_ShouldInterpret_ForEach()
  {
    // Arrange
    object?[] arguments = new object?[] { new int[] { 1, 2, 3 } };
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.ForEach);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.ForEach);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(6);
  }

  [Fact]
  public void Interpret_ShouldInterpret_While()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.While);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.While);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(16);
  }

  [Fact]
  public void Interpret_ShouldInterpret_DoWhile()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.DoWhile);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.DoWhile);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(32);
  }

  [Fact]
  public void Interpret_ShouldInterpret_For_WithBreak()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.For_WithBreak);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.For_WithBreak);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(6);
  }

  [Fact]
  public void Interpret_ShouldInterpret_ForEach_WithBreak()
  {
    // Arrange
    object?[] arguments = new object?[] { new int[] { 1, 2, 3 } };
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.ForEach_WithBreak);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.ForEach_WithBreak);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(91);
  }

  [Fact]
  public void Interpret_ShouldInterpret_While_WithBreak()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.While_WithBreak);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.While_WithBreak);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(4);
  }

  [Fact]
  public void Interpret_ShouldInterpret_DoWhile_WithBreak()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.DoWhile_WithBreak);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.DoWhile_WithBreak);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(8);
  }

  [Fact]
  public void Interpret_ShouldInterpret_For_WithContinue()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.For_WithContinue);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.For_WithContinue);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(7);
  }

  [Fact]
  public void Interpret_ShouldInterpret_ForEach_WithContinue()
  {
    // Arrange
    object?[] arguments = new object?[] { new int[] { 1, 2, 3 } };
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.ForEach_WithContinue);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.ForEach_WithContinue);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(6);
  }

  [Fact]
  public void Interpret_ShouldInterpret_While_WithContinue()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.While_WithContinue);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.While_WithContinue);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(20);
  }

  [Fact]
  public void Interpret_ShouldInterpret_DoWhile_WithContinue()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.DoWhile_WithContinue);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.DoWhile_WithContinue);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(36);
  }

  [Fact]
  public void Interpret_ShouldInterpret_StringInterpolation()
  {
    // Arrange
    System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-US");
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.StringInterpolation);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.StringInterpolation);

    // Act
    _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    _frameProvider[2].Should().HaveCount(5);
    _frameProvider[2].Should().Contain("str1", "19:44:12");
    _frameProvider[2].Should().Contain("str2", "|Left   |  Right|");
    _frameProvider[2].Should().Contain("str3", "   3.141592653589793");
    _frameProvider[2].Should().Contain("str4", "               3.142");
    _frameProvider[2].Should().Contain("str5", "'str2' is shorter than 'str3'");
  }

  [Fact]
  public void Interpret_ShouldInterpret_FieldReference_Source()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.FieldReference_Source);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.FieldReference_Source);

    // Act
    _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    _frameProvider[2].Should().HaveCount(3);
    _frameProvider[2].Should().Contain("f1", 5);
    _frameProvider[2].Should().Contain("f2", 12);
  }

  [Fact]
  public void Interpret_ShouldInterpret_ObjectCreation()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.ObjectInitializer);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.ObjectInitializer);

    // Act
    _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    _frameProvider[2].Should().HaveCount(3);
    _frameProvider[2].Should().ContainKey("obj");
    _frameProvider[2].Should().Contain("prop1", 3);
    _frameProvider[2].Should().Contain("prop2", "prop");
  }

  [Theory]
  [InlineData(typeof(string), 6)]
  [InlineData(typeof(char), 4)]
  public void Interpret_ShouldInterpret_Invocation_NonGenericMethod_GenericType(Type typeArgument, int expected)
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Invocation_NonGenericMethod_GenericType);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Invocation_NonGenericMethod_GenericType, typeArgument);

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Invocation_GenericMethod_GenericType1()
  {
    // Arrange
    const int expected = 6;
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Invocation_GenericMethod_GenericType1);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Invocation_GenericMethod_GenericType1, typeof(int));

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Invocation_GenericMethod_GenericType2()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Invocation_GenericMethod_GenericType2);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Invocation_GenericMethod_GenericType2, typeof(int));

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Invocation_StaticMethod()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Invocation_StaticMethod);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Invocation_StaticMethod, typeof(int));

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Property_InstanceProperty()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Property_InstanceProperty);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Property_InstanceProperty, typeof(int));

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_Property_StaticProperty()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Property_StaticProperty);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Property_StaticProperty, typeof(int));

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void Interpret_ShouldInterpret_GenericObjectCreation()
  {
    // Arrange
    const string arg1 = "argument1";
    object?[] arguments = new object?[] { arg1 };
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.GenericObjectCreation);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.GenericObjectCreation, typeof(string));

    // Act
    _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    _frameProvider[2].Should().Contain("value1", arg1);
    _frameProvider[2].Should().Contain("value2", 4);
  }

  [Fact]
  public void Interpret_ShouldInterpret_FieldReference_Ref()
  {
    // Arrange
    const string arg1 = "argument1";
    object?[] arguments = new object?[] { arg1 };
    SymbolMethodInfo method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.FieldReference_Ref);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.FieldReference_Ref, typeof(string));

    // Act
    object? result = _sut.Interpret(method, instanceFrame, arguments);

    // Assert
    result.Should().Be(arg1);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForNonGenericType_WhenFirstCtor()
  {
    // Arrange
    const int value1 = 10;
    SymbolType sourceType = _fixture.GetSourceType(_runtime, SourceType.NonGenericClassWithCtors);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame classFrame = _sut.GetClassFrame(sourceType);
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
    InterpreterFrame classFrame = _sut.GetClassFrame(sourceType);
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
    InterpreterFrame classFrame = _sut.GetClassFrame(sourceType);
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
    InterpreterFrame classFrame = _sut.GetClassFrame(sourceType.MakeGenericType(typeof(string)));
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
    InterpreterFrame classFrame = _sut.GetClassFrame(sourceType);
    object?[] arguments = new object?[] { value1 };

    object?[] expected = Array.Empty<object?>();

    // Act
    object?[] actual = _sut.GetProxyArguments(constructor, classFrame, arguments);

    // Assert
    actual.Should().Equal(expected);
  }

  [Fact]
  public void GetClassFrame_ShouldInitializeInstanceFieldsAndProperties()
  {
    // Arrange
    SymbolType sourceType = _fixture.GetSourceType(_runtime, SourceType.NonGenericClass);

    // Act
    _sut.GetClassFrame(sourceType);

    // Assert
    _frameProvider[0].Should().Contain("_staticField", 13);
    _frameProvider[0].Should().Contain("_staticFieldFromCctor", 'a');
    _frameProvider[0].Should().Contain("StaticProperty", "prop2");
  }

  [Fact]
  public void GetInstanceFrame_ShouldInitializeInstanceFieldsAndProperties()
  {
    // Arrange
    SymbolType sourceType = _fixture.GetSourceType(_runtime, SourceType.NonGenericClass);
    InterpreterFrame classFrame = InterpreterFrame.NewClassFrame(null, new Dictionary<ISymbol, object?>(SymbolEqualityComparer.Default), Type.EmptyTypes);

    // Act
    _sut.GetInstanceFrame(classFrame, sourceType, new object());

    // Assert
    _frameProvider[0].Should().Contain("_instanceField", 12);
    _frameProvider[0].Should().Contain("InstanceProperty", "prop1");
  }
}
