#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Expressions;
using GeneratorKit.Expressions.Operations;
using GeneratorKit.Expressions.Simple;
using GeneratorKit.Expressions.Symbol;
using GeneratorKit.Interpret.Frame;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

namespace GeneratorKit.Interpret.Context;

internal class ExpressionContext : OperationVisitor<Unit, IExpression>, IExpressionContext
{
  private static readonly MethodInfo s_getMethod;

  static ExpressionContext()
  {
    s_getMethod = typeof(InterpreterFrame).GetMethod(nameof(InterpreterFrame.Get))
      ?? throw new MissingMethodException($"Method '{nameof(InterpreterFrame.Get)}' not found on type {typeof(InterpreterFrame)}.");
  }

  private readonly IInterpreterContext _context;
  private readonly InterpreterFrame _frame;
  private readonly IAnonymousFunctionOperation _operation;
  private readonly Dictionary<IParameterSymbol, IParameterExpression> _parameters;

  public ExpressionContext(IInterpreterContext context, InterpreterFrame frame, IAnonymousFunctionOperation operation)
  {
    _context = context;
    _frame = frame;
    _operation = operation;
    _parameters = new Dictionary<IParameterSymbol, IParameterExpression>(SymbolEqualityComparer.Default);
  }

  public ILambdaExpression Expression => new AnonymousFunctionOperationExpression(this, _operation);

  public Type GetDelegateType(IMethodSymbol symbol)
  {
    ImmutableArray<IParameterSymbol> parameters = symbol.Parameters;
    if (parameters.Length > 16)
      throw new NotSupportedException("Delegates with more than 16 parameters are not supported.");
    
    INamedTypeSymbol type;
    if (symbol.ReturnsVoid)
    {
      type = parameters.Length == 0
        ? _context.Compilation.GetTypeByMetadataName("System.Action")!
        : _context.Compilation.GetTypeByMetadataName($"System.Action`{parameters.Length}")!.Construct(parameters.Map(parameter => parameter.Type));
    }
    else
    {
      ITypeSymbol[] typeArguments = new ITypeSymbol[parameters.Length + 1];
      for (int i = 0; i < parameters.Length; i++)
      {
        typeArguments[i] = parameters[i].Type;
      }
      typeArguments[parameters.Length] = symbol.ReturnType;
      type = _context.Compilation.GetTypeByMetadataName($"System.Func`{typeArguments.Length}")!.Construct(typeArguments);
    }

    return _context.GetType(type);
  }

  public Type GetType(ITypeSymbol symbol)
  {
    return _context.GetType(symbol);
  }

  public MethodInfo GetMethodInfo(IMethodSymbol symbol)
  {
    return _context.GetMethodInfo(symbol);
  }

  public ConstructorInfo GetConstructorInfo(IMethodSymbol symbol)
  {
    return _context.GetConstructorInfo(symbol);
  }

  public MemberInfo GetPropertyInfo(IPropertySymbol symbol)
  {
    return _context.GetPropertyInfo(symbol);
  }

  public MemberInfo GetFieldInfo(IFieldSymbol symbol)
  {
    return _context.GetFieldInfo(symbol);
  }

  public bool IsNullableType(ITypeSymbol symbol)
  {
    if (symbol is not INamedTypeSymbol namedType)
      return false;

    INamedTypeSymbol nullableType = _context.Compilation.GetSpecialType(SpecialType.System_Nullable_T);
    return namedType.ConstructedFrom.Equals(nullableType, SymbolEqualityComparer.Default);
  }

  public IExpression GetExpression(IOperation operation)
  {
    return operation.Accept(this, Unit.Instance)!;
  }

  public IParameterExpression GetParameter(IParameterSymbol symbol)
  {
    Debug.Assert(symbol.ContainingSymbol.Equals(_operation.Symbol, SymbolEqualityComparer.Default)); // TODO: Refactor

    if (!_parameters.TryGetValue(symbol, out IParameterExpression expression))
    {
      expression = new ParameterSymbolExpression(this, symbol);
      _parameters.Add(symbol, expression);
    }

    return expression;
  }


  public override IExpression? DefaultVisit(IOperation operation, Unit argument)
  {
    throw new NotSupportedException($"Operation of kind {operation.Kind} cannot be converted to an expression.");
  }

  public override IExpression? VisitAnonymousFunction(IAnonymousFunctionOperation operation, Unit argument)
  {
    throw new NotImplementedException();
  }

  public override IExpression? VisitArgument(IArgumentOperation operation, Unit argument)
  {
    if (operation.Parameter is null)
      throw new NotSupportedException("__arglist parameters are not supported.");

    return operation.Value.Accept(this, argument);
  }

  public override IExpression? VisitArrayCreation(IArrayCreationOperation operation, Unit argument)
  {
    return new ArrayCreationOperation(this, operation);
  }

  public override IExpression? VisitArrayElementReference(IArrayElementReferenceOperation operation, Unit argument)
  {
    return new ArrayElementReferenceOperationExpression(this, operation);
  }

  public override IExpression? VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, Unit argument)
  {
    return new AnonymousObjectCreationOperationExpression(this, operation);
  }

  public override IExpression? VisitBinaryOperator(IBinaryOperation operation, Unit argument)
  {
    return new BinaryOperationExpression(this, operation);
  }

  public override IExpression? VisitBlock(IBlockOperation operation, Unit argument)
  {
    return operation.Operations[0].Accept(this, argument);
  }

  public override IExpression? VisitConditional(IConditionalOperation operation, Unit argument)
  {
    return new ConditionalOperationExpression(this, operation);
  }

  public override IExpression? VisitConversion(IConversionOperation operation, Unit argument)
  {
    if (operation.Operand is IAnonymousObjectCreationOperation operand && operation.Conversion.IsImplicit)
    {
      return new AnonymousObjectCreationOperationExpression(this, operand); // TODO: Figure out if this can be avoided
    }
    return new ConversionOperationExpression(this, operation);
  }

  public override IExpression? VisitFieldReference(IFieldReferenceOperation operation, Unit argument)
  {
    return new FieldReferenceOperationExpression(this, operation);
  }

  public override IExpression? VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation, Unit argument)
  {
    return new IncrementOrDecrementOperationExpression(this, operation);
  }

  public override IExpression? VisitInvocation(IInvocationOperation operation, Unit argument)
  {
    return new InvocationOperationExpression(this, operation);
  }

  public override IExpression? VisitIsType(IIsTypeOperation operation, Unit argument)
  {
    return new IsTypeOperationExpression(this, operation);
  }

  public override IExpression? VisitLiteral(ILiteralOperation operation, Unit argument)
  {
    return new LiteralOperationExpression(this, operation);
  }

  public override IExpression? VisitObjectCreation(IObjectCreationOperation operation, Unit argument)
  {
    return new ObjectCreationOperationExpression(this, operation);
  }

  public override IExpression? VisitParameterReference(IParameterReferenceOperation operation, Unit argument)
  {
    if (!operation.Parameter.ContainingSymbol.Equals(_operation.Symbol, SymbolEqualityComparer.Default))
    {
      IConstantExpression frameExpression = new SimpleConstantExpression(_frame, typeof(InterpreterFrame));
      IConstantExpression parameterExpression = new SimpleConstantExpression(operation.Parameter, typeof(ISymbol));
      return new SimpleMethodCallExpression(s_getMethod, frameExpression, new[] { parameterExpression }); // TODO: Add conversion
    }
    return GetParameter(operation.Parameter);
  }

  public override IExpression? VisitPropertyReference(IPropertyReferenceOperation operation, Unit argument)
  {
    return new PropertyReferenceOperationExpression(this, operation);
  }

  public override IExpression? VisitReturn(IReturnOperation operation, Unit argument)
  {
    return operation.ReturnedValue!.Accept(this, argument);
  }
}
