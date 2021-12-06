using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Reflection;
using System.Reflection.Emit;

namespace GeneratorKit.Proxy;


internal class ConstructorInitializerOperationVisitor : OperationVisitor
{
  private readonly GeneratorRuntime _runtime;
  private readonly ILGenerator _il;
  private readonly SymbolArgumentParameter[] _parameters;
  private ConstructorInfo? _constructor;

  public ConstructorInitializerOperationVisitor(GeneratorRuntime runtime, ILGenerator il, SymbolArgumentParameter[] parameters)
  {
    _runtime = runtime;
    _il = il;
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
    _constructor = _runtime.CreateConstructorInfoDelegator(operation.TargetMethod).RuntimeConstructor;

    _il.Emit(OpCodes.Ldarg_0);

    foreach (IArgumentOperation argument in operation.Arguments)
    {
      Visit(argument.Value);
    }
  }

  public override void VisitParameterReference(IParameterReferenceOperation operation)
  {
    int index = 0;
    foreach (SymbolArgumentParameter parameter in _parameters)
    {
      if (parameter.Symbol.Equals(operation.Parameter, SymbolEqualityComparer.Default))
      {
        break;
      }
      index++;
    }

    switch (index)
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
        _il.Emit(OpCodes.Ldarg_S, index + 1);
        break;
    }

    _il.Emit(OpCodes.Call, _constructor!);
  }
}