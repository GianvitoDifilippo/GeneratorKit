#pragma warning disable RS1009 // Only internal implementations of this interface are allowed. Reason: Used only to extend the visitor to default constructors. We can find another solution if needed.

using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Emit;

namespace GeneratorKit.Emit;

internal class BuildConstructorsStage
{
  private static readonly ImplicitConstructorOperation s_implicitConstructorOperation = new ImplicitConstructorOperation();

  private readonly IBuildContext _context;
  private readonly Type _baseType;
  private readonly IReadOnlyCollection<InitializerData> _instanceInitializers;
  private readonly IReadOnlyCollection<InitializerData> _staticInitializers;

  public BuildConstructorsStage(
    IBuildContext context,
    Type baseType,
    IReadOnlyCollection<InitializerData> instanceInitializers,
    IReadOnlyCollection<InitializerData> staticInitializers)
  {
    _context = context;
    _baseType = baseType;
    _instanceInitializers = instanceInitializers;
    _staticInitializers = staticInitializers;
  }

  public void BuildConstructor(SymbolConstructorInfo constructor)
  {
    if (constructor.IsStatic)
    {
      BuildStaticConstructor(constructor);
    }
    else
    {
      BuildInstanceConstructor(constructor);
    }
  }

  private void BuildInstanceConstructor(SymbolConstructorInfo constructor)
  {
    IMethodSymbol constructorSymbol = constructor.Symbol;

    Type[] parameterTypes = constructor.GetParameters().Select(x => _context.ResolveType(x.ParameterType)).ToArray();
    ConstructorBuilder constructorBuilder = _context.TypeBuilder.DefineConstructor(constructor.Attributes, constructor.CallingConvention, parameterTypes);

    ILGenerator il = constructorBuilder.GetILGenerator();

    foreach ((FieldBuilder field, IOperation initOperation) in _instanceInitializers)
    {
      il.Emit(OpCodes.Ldarg_0);
      new FieldInitializerOperationVisitor(il).Visit(initOperation);
      il.Emit(OpCodes.Stfld, field);
    }

    IOperation? constructorOperation = GetOperation(constructorSymbol);
    if (constructorOperation is not null)
    {
      new ConstructorInitializerOperationVisitor(_context.Runtime, il, _baseType).Visit(constructorOperation);
    }

    il.Emit(OpCodes.Ret);
}

  private void BuildStaticConstructor(SymbolConstructorInfo constructor)
  {
    // Not using 'DefineTypeInitializers' because 'HideBySig' is missing in the constructor attributes.
    ConstructorBuilder constructorBuilder = _context.TypeBuilder.DefineConstructor(constructor.Attributes, constructor.CallingConvention, null);

    ILGenerator il = constructorBuilder.GetILGenerator();

    foreach ((FieldBuilder field, IOperation initOperation) in _staticInitializers)
    {
      new FieldInitializerOperationVisitor(il).Visit(initOperation);
      il.Emit(OpCodes.Stfld, field);
    }

    il.Emit(OpCodes.Ret);
  }

  private IOperation? GetOperation(IMethodSymbol constructorSymbol)
  {
    if (constructorSymbol.IsImplicitlyDeclared)
    {
      return s_implicitConstructorOperation;
    }

    SemanticModel semanticModel = _context.Runtime.Compilation.GetSemanticModel(constructorSymbol.DeclaringSyntaxReferences[0].SyntaxTree);
    SyntaxNode syntax = constructorSymbol.DeclaringSyntaxReferences[0].GetSyntax();
    return semanticModel.GetOperation(syntax, _context.Runtime.CancellationToken);
  }

  private class ImplicitConstructorOperation : IConstructorBodyOperation
  {
    #region Not implemented
    public ImmutableArray<ILocalSymbol> Locals => throw new NotImplementedException();
    public IBlockOperation? BlockBody => throw new NotImplementedException();
    public IBlockOperation? ExpressionBody => throw new NotImplementedException();
    public IOperation? Parent => throw new NotImplementedException();
    public OperationKind Kind => throw new NotImplementedException();
    public SyntaxNode Syntax => throw new NotImplementedException();
    public ITypeSymbol? Type => throw new NotImplementedException();
    public Optional<object?> ConstantValue => throw new NotImplementedException();
    public IEnumerable<IOperation> Children => throw new NotImplementedException();
    public string Language => throw new NotImplementedException();
    public bool IsImplicit => throw new NotImplementedException();
    public SemanticModel? SemanticModel => throw new NotImplementedException();
    public TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) => throw new NotImplementedException();
    #endregion

    public IOperation? Initializer => null;
    public void Accept(OperationVisitor visitor) => visitor.VisitConstructorBodyOperation(this);
  }
}
