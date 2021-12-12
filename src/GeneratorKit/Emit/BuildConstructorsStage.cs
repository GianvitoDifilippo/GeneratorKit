using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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

    Type[] parameterTypes = constructor.GetParameters().Map(x => _context.ResolveType(x.ParameterType));
    ConstructorBuilder constructorBuilder = _context.TypeBuilder.DefineConstructor(constructor.Attributes, constructor.CallingConvention, parameterTypes);

    ILGenerator il = constructorBuilder.GetILGenerator();

    foreach ((FieldBuilder field, IOperation initOperation) in _instanceInitializers)
    {
      il.Emit(OpCodes.Ldarg_0);
      new FieldInitializerVisitor(il).Visit(initOperation);
      il.Emit(OpCodes.Stfld, field);
    }

    IOperation? constructorOperation = constructorSymbol.IsImplicitlyDeclared
      ? s_implicitConstructorOperation
      : _context.GetOperation(constructorSymbol);
    if (constructorOperation is not null)
    {
      new ConstructorInitializerVisitor(_context.Runtime, il, _baseType).Visit(constructorOperation);
    }

    il.Emit(OpCodes.Ret);
}

  private void BuildStaticConstructor(SymbolConstructorInfo constructor)
  {
    // Not using 'DefineTypeInitializer' because 'HideBySig' is missing in the constructor attributes.
    ConstructorBuilder constructorBuilder = _context.TypeBuilder.DefineConstructor(constructor.Attributes, constructor.CallingConvention, null);

    ILGenerator il = constructorBuilder.GetILGenerator();

    foreach ((FieldBuilder field, IOperation initOperation) in _staticInitializers)
    {
      new FieldInitializerVisitor(il).Visit(initOperation);
      il.Emit(OpCodes.Stfld, field);
    }

    il.Emit(OpCodes.Ret);
  }

#pragma warning disable RS1009 // Only internal implementations of this interface are allowed. Reason: Used only to extend the visitor to default constructors. We can find another solution if needed.
  private class ImplicitConstructorOperation : IConstructorBodyOperation
  {
    #region Not implemented
    [ExcludeFromCodeCoverage] public ImmutableArray<ILocalSymbol> Locals => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public IBlockOperation? BlockBody => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public IBlockOperation? ExpressionBody => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public IOperation? Parent => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public OperationKind Kind => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public SyntaxNode Syntax => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public ITypeSymbol? Type => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public Optional<object?> ConstantValue => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public IEnumerable<IOperation> Children => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public string Language => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public bool IsImplicit => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public SemanticModel? SemanticModel => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) => throw new NotImplementedException();
    #endregion

    public IOperation? Initializer => null;
    public void Accept(OperationVisitor visitor) => visitor.VisitConstructorBodyOperation(this);
  }
#pragma warning restore RS1009 // Only internal implementations of this interface are allowed.
}
