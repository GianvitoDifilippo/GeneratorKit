using FluentAssertions;
using GeneratorKit.Interpret.Frame;
using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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
  public void InterpretMethod_ShouldInterpret_SimpleAssignment()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.SimpleAssignment);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.SimpleAssignment);

    // Act
    _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    _frameProvider[2].Should().HaveCount(1);
    _frameProvider[2].Should().Contain("v1", 1);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_TupleAssignment()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.TupleAssignment);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.TupleAssignment);

    // Act
    _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);
    
    // Assert
    _frameProvider[2].Should().HaveCount(2);
    _frameProvider[2].Should().Contain("v1", 2);
    _frameProvider[2].Should().Contain("v2", "str");
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_NameOf()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    const string expected = "NonGenericClass_Interpret";
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.NameOf);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.NameOf);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_Discard()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Discard);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Discard);

    // Act
    _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    _frameProvider[2].Should().HaveCount(0);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_Default()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    int expected = default;
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Default);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Default);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Theory]
  [InlineData(typeof(int))]
  [InlineData(typeof(string))]
  public void InterpretMethod_ShouldInterpret_DefaultGeneric(Type typeArgument)
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    object? expected = typeArgument.IsValueType ? System.Activator.CreateInstance(typeArgument) : null;
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.DefaultGeneric);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.DefaultGeneric);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, new[] { typeArgument }, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Theory]
  [InlineData(0, "zero")]
  [InlineData(5, "not zero")]
  public void InterpretMethod_ShouldInterpret_IfElse(int arg, string expected)
  {
    // Arrange
    object?[] arguments = new object?[] { arg };
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.IfElse);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.IfElse);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_For()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.For);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.For);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(10);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_ForEach()
  {
    // Arrange
    object?[] arguments = new object?[] { new int[] { 1, 2, 3 } };
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.ForEach);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.ForEach);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(6);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_While()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.While);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.While);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(16);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_DoWhile()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.DoWhile);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.DoWhile);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(32);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_For_WithBreak()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.For_WithBreak);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.For_WithBreak);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(6);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_ForEach_WithBreak()
  {
    // Arrange
    object?[] arguments = new object?[] { new int[] { 1, 2, 3 } };
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.ForEach_WithBreak);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.ForEach_WithBreak);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(91);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_While_WithBreak()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.While_WithBreak);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.While_WithBreak);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(4);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_DoWhile_WithBreak()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.DoWhile_WithBreak);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.DoWhile_WithBreak);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(8);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_For_WithContinue()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.For_WithContinue);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.For_WithContinue);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(7);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_ForEach_WithContinue()
  {
    // Arrange
    object?[] arguments = new object?[] { new int[] { 1, 2, 3 } };
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.ForEach_WithContinue);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.ForEach_WithContinue);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(6);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_While_WithContinue()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.While_WithContinue);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.While_WithContinue);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(20);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_DoWhile_WithContinue()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.DoWhile_WithContinue);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.DoWhile_WithContinue);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(36);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_StringInterpolation()
  {
    // Arrange
    System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-US");
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.StringInterpolation);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.StringInterpolation);

    // Act
    _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    _frameProvider[2].Should().HaveCount(5);
    _frameProvider[2].Should().Contain("str1", "19:44:12");
    _frameProvider[2].Should().Contain("str2", "|Left   |  Right|");
    _frameProvider[2].Should().Contain("str3", "   3.141592653589793");
    _frameProvider[2].Should().Contain("str4", "               3.142");
    _frameProvider[2].Should().Contain("str5", "'str2' is shorter than 'str3'");
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_FieldReference_Source()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.FieldReference_Source);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.FieldReference_Source);

    // Act
    _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    _frameProvider[2].Should().HaveCount(3);
    _frameProvider[2].Should().Contain("f1", 5);
    _frameProvider[2].Should().Contain("f2", 12);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_ObjectCreation()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.ObjectInitializer);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.ObjectInitializer);

    // Act
    _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    _frameProvider[2].Should().HaveCount(3);
    _frameProvider[2].Should().ContainKey("obj");
    _frameProvider[2].Should().Contain("prop1", 3);
    _frameProvider[2].Should().Contain("prop2", "prop");
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_Coalesce()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Coalesce);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Coalesce);

    // Act
    _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    _frameProvider[2].Should().HaveCount(3);
    _frameProvider[2].Should().Contain("i1", null);
    _frameProvider[2].Should().Contain("i2", "4");
    _frameProvider[2].Should().Contain("i3", "4");
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_CoalesceAssignment()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.CoalesceAssignment);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.CoalesceAssignment);

    // Act
    _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    _frameProvider[2].Should().HaveCount(1);
    _frameProvider[2].Should().Contain("i1", 4);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_ConditionalAccess()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.ConditionalAccess);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.ConditionalAccess);

    // Act
    _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    _frameProvider[2].Should().HaveCount(4);
    _frameProvider[2].Should().Contain("v1", null);
    _frameProvider[2].Should().Contain("v2", 4);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_IsType()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.IsType);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.IsType);

    // Act
    _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    _frameProvider[2].Should().HaveCount(11);
    _frameProvider[2].Should().Contain("b1", false);
    _frameProvider[2].Should().Contain("b2", false);
    _frameProvider[2].Should().Contain("b3", true);
    _frameProvider[2].Should().Contain("b4", true);
    _frameProvider[2].Should().Contain("b5", false);
    _frameProvider[2].Should().Contain("b6", true);
    _frameProvider[2].Should().Contain("b7", true);
    _frameProvider[2].Should().Contain("b8", false);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_Array()
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Array);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Array);
    int[] arr1 = new int[4] { 1, 2, 3, 4 };
    int[][] arr2 = new int[][] { new[] { 1, 2 }, new[] { 3, 4 }, new[] { 5, 6 } };
    int[,,] arr3 = new int[2, 3, 4] { { { 1, 2, 3, 4 }, { 5, 6, 7, 8 }, { 9, 10, 11, 12 } }, { { 13, 14, 15, 16 }, { 17, 18, 19, 20 }, { 21, 22, 23, 24 } } };
    
    // Act
    _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    IReadOnlyDictionary<string, object?> frame = _frameProvider[2];
    frame.Should().HaveCount(3);
    frame.Should().ContainKey("arr1");
    frame.Should().ContainKey("arr2");
    frame.Should().ContainKey("arr3");
    frame["arr1"].Should().BeOfType<int[]>().Which.Should().Equal(arr1);
    frame["arr2"].Should().BeOfType<int[][]>().Which.As<object>().Should().Equal(arr2, new ArrayArrayEqualityComparer<int>());
    frame["arr3"].Should().BeOfType<int[,,]>().Which.Should().Equal(arr3, new Array3EqualityComparer<int>());
  }

  [Theory]
  [InlineData(typeof(string), 6)]
  [InlineData(typeof(char), 4)]
  public void InterpretMethod_ShouldInterpret_Invocation_NonGenericMethod_GenericType(Type typeArgument, int expected)
  {
    // Arrange
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Invocation_NonGenericMethod_GenericType);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Invocation_NonGenericMethod_GenericType, typeArgument);

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_Invocation_GenericMethod_GenericType1()
  {
    // Arrange
    const int expected = 6;
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Invocation_GenericMethod_GenericType1);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Invocation_GenericMethod_GenericType1, typeof(int));

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_Invocation_GenericMethod_GenericType2()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Invocation_GenericMethod_GenericType2);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Invocation_GenericMethod_GenericType2, typeof(int));

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_Invocation_StaticMethod()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Invocation_StaticMethod);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Invocation_StaticMethod, typeof(int));

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_Property_InstanceProperty()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Property_InstanceProperty);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Property_InstanceProperty, typeof(int));

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_Property_StaticProperty()
  {
    // Arrange
    const int expected = 5;
    object?[] arguments = Array.Empty<object?>();
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.Property_StaticProperty);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.Property_StaticProperty, typeof(int));

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(expected);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_GenericObjectCreation()
  {
    // Arrange
    const string arg1 = "argument1";
    object?[] arguments = new object?[] { arg1 };
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.GenericObjectCreation);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.GenericObjectCreation, typeof(string));

    // Act
    _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    _frameProvider[2].Should().Contain("value1", arg1);
    _frameProvider[2].Should().Contain("value2", 4);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_FieldReference_Ref()
  {
    // Arrange
    const string arg1 = "argument1";
    object?[] arguments = new object?[] { arg1 };
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.FieldReference_Ref);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.FieldReference_Ref, typeof(string));

    // Act
    object? result = _sut.InterpretMethod(method, instanceFrame, Type.EmptyTypes, arguments);

    // Assert
    result.Should().Be(arg1);
  }

  [Fact]
  public void InterpretMethod_ShouldInterpret_MethodReference_Ref()
  {
    // Arrange
    const string arg1 = "argument1";
    object?[] arguments = new object?[] { arg1 };
    IMethodSymbol method = _fixture.GetInterpretedOperationMethod(_runtime, InterpretedOperationType.MethodReference_Ref);
    InterpreterFrame instanceFrame = _fixture.GetInstanceFrame(_runtime, _sut, InterpretedOperationType.MethodReference_Ref, typeof(string));

    // Act
    _sut.InterpretMethod(method, instanceFrame, new[] { typeof(int) }, arguments);

    // Assert
    _frameProvider[2].Should().Contain("value1", arg1);
    _frameProvider[2].Should().Contain("value2", 4);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForNonGenericType_WhenFirstCtor()
  {
    // Arrange
    const int value1 = 10;
    SymbolType sourceType = _fixture.GetSourceType(_runtime, SourceType.NonGenericClassWithCtors);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame typeFrame = _sut.GetTypeFrame(sourceType);
    object?[] arguments = new object?[] { value1 };

    object?[] expected = new object?[] { value1 };

    // Act
    object?[] result = _sut.GetProxyArguments(constructor.OriginalSymbol, typeFrame, arguments);

    // Assert
    result.Should().Equal(expected);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForNonGenericType_WhenSecondCtor()
  {
    // Arrange
    const int value1 = 42;
    SymbolType sourceType = _fixture.GetSourceType(_runtime, SourceType.NonGenericClassWithCtors);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[1];
    InterpreterFrame typeFrame = _sut.GetTypeFrame(sourceType);
    object?[] arguments = Array.Empty<object?>();

    object?[] expected = new object?[] { value1 };

    // Act
    object?[] result = _sut.GetProxyArguments(constructor.OriginalSymbol, typeFrame, arguments);

    // Assert
    result.Should().Equal(expected);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForNonGenericType_WhenThirdCtor()
  {
    // Arrange
    const int value1 = 10;
    const string value2 = "v2";
    SymbolType sourceType = _fixture.GetSourceType(_runtime, SourceType.NonGenericClassWithCtors);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[2];
    InterpreterFrame typeFrame = _sut.GetTypeFrame(sourceType);
    object?[] arguments = new object?[] { value1, value2 };

    object?[] expected = new object?[] { value2 };

    // Act
    object?[] result = _sut.GetProxyArguments(constructor.OriginalSymbol, typeFrame, arguments);

    // Assert
    result.Should().Equal(expected);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForGenericType()
  {
    // Arrange
    const int value1 = 10;
    const string value2 = "v2";
    SymbolType sourceType = _fixture.GetSourceType(_runtime, SourceType.GenericClassWithCtors);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame typeFrame = _sut.GetTypeFrame(sourceType.MakeGenericType(typeof(string)));
    object?[] arguments = new object?[] { value1, value2 };

    object?[] expected = new object?[] { value2 };

    // Act
    object?[] result = _sut.GetProxyArguments(constructor.OriginalSymbol, typeFrame, arguments);

    // Assert
    result.Should().Equal(expected);
  }

  [Fact]
  public void GetProxyArguments_ShouldReturnProxyArgumentsForInterfaceType()
  {
    // Arrange
    const int value1 = 10;
    SymbolType sourceType = _fixture.GetSourceType(_runtime, SourceType.Interface);
    SymbolConstructorInfo constructor = sourceType.GetConstructors()[0];
    InterpreterFrame typeFrame = _sut.GetTypeFrame(sourceType);
    object?[] arguments = new object?[] { value1 };

    object?[] expected = Array.Empty<object?>();

    // Act
    object?[] result = _sut.GetProxyArguments(constructor.OriginalSymbol, typeFrame, arguments);

    // Assert
    result.Should().Equal(expected);
  }

  [Fact]
  public void GetTypeFrame_ShouldInitializeInstanceFieldsAndProperties()
  {
    // Arrange
    SymbolType sourceType = _fixture.GetSourceType(_runtime, SourceType.NonGenericClass);

    // Act
    _sut.GetTypeFrame(sourceType);

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
    InterpreterFrame typeFrame = _sut.GetTypeFrame(sourceType);

    // Act
    _sut.GetInstanceFrame(typeFrame, sourceType, new object());

    // Assert
    _frameProvider[2].Should().Contain("_instanceField", 12);
    _frameProvider[2].Should().Contain("InstanceProperty", "prop1");
  }
}
