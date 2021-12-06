using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace GeneratorKit.Proxy;

internal class ConstructorInitializerOperationVisitor : OperationVisitor
{
  private readonly GeneratorRuntime _runtime;
  private readonly ILGenerator _il;
  private readonly Type _baseType;
  private readonly SymbolArgumentParameter[] _parameters;

  public ConstructorInitializerOperationVisitor(GeneratorRuntime runtime, ILGenerator il, Type baseType, SymbolArgumentParameter[] parameters)
  {
    _runtime = runtime;
    _il = il;
    _baseType = baseType;
    _parameters = parameters;
  }

  public override void VisitArgument(IArgumentOperation operation)
  {
    base.VisitArgument(operation);
  }

  public override void VisitConstructorBodyOperation(IConstructorBodyOperation operation)
  {
    if (operation.Initializer is not null)
    {
      Visit(operation.Initializer);
    }
  }

  public override void VisitExpressionStatement(IExpressionStatementOperation operation)
  {
    Visit(operation.Operation);
  }

  public override void VisitInvocation(IInvocationOperation operation)
  {
    IMethodSymbol targetMethodSymbol = operation.TargetMethod;
    ConstructorInfo constructorDefinition = _runtime.CreateConstructorInfoDelegator(targetMethodSymbol.OriginalDefinition).RuntimeConstructor;

    ConstructorInfo constructor = TypeBuilder.GetConstructor(_baseType, constructorDefinition);

    _il.Emit(OpCodes.Ldarg_0);

    foreach (IArgumentOperation argument in operation.Arguments)
    {
      Visit(argument.Value);
    }

    _il.Emit(OpCodes.Call, constructor);
  }

  public override void VisitParameterReference(IParameterReferenceOperation operation)
  {
    int position = operation.Parameter.Ordinal;
    switch (position)
    {
      case 0:
        _il.Emit(OpCodes.Ldarg_1);
        break;
      case 1:
        _il.Emit(OpCodes.Ldarg_2);
        break;
      case 2:
        _il.Emit(OpCodes.Ldarg_3);
        break;
      default:
        _il.Emit(OpCodes.Ldarg_S, position + 1);
        break;
    }
  }
}