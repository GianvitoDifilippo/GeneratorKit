using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;

namespace GeneratorKit.Interpret;

internal class ConstructorInitializerVisitor : InterpreterVisitor
{
  public ConstructorInitializerVisitor(GeneratorRuntime runtime, IFrameProvider frameProvider, InterpreterFrame constructorFrame)
    : base(runtime, frameProvider, constructorFrame)
  {
  }

  public override object? VisitConstructorBodyOperation(IConstructorBodyOperation operation, Optional<object?> argument)
  {
    return operation.Initializer is not null
      ? operation.Initializer.Accept(this, default)
      : Array.Empty<object?>();
  }

  public override object? VisitInvocation(IInvocationOperation operation, Optional<object?> argument)
  {
    return operation.Arguments.Map(op => VisitArgument(op, default));
  }
}
