using GeneratorKit.Exceptions;
using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection.Emit;

using InitializerCollection = System.Collections.Generic.IReadOnlyCollection<GeneratorKit.Emit.InitializerData>; // For short

namespace GeneratorKit.Emit;

internal class BuildConstructorsStage : BuildOperationsStage
{
  private readonly Type _baseType;
  private readonly InitializerCollection _instanceInitializers;
  private readonly InitializerCollection _staticInitializers;

  public BuildConstructorsStage(IBuildContext context, Type baseType, InitializerCollection instanceInitializers, InitializerCollection staticInitializers)
    : base(context)
  {
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

    IOperation? operation = _context.GetOperation(constructorSymbol);
    new ConstructorInitializerVisitor(_context.Runtime, il, _baseType).Visit(operation);

    if (!constructorSymbol.IsImplicitlyDeclared)
    {
      if (operation is null)
        throw new OperationResolutionException(constructorSymbol);

      BuildMethodBody(il, typeof(void), parameterTypes, operation, constructorSymbol);
    }

    il.Emit(OpCodes.Ret);
}

  private void BuildStaticConstructor(SymbolConstructorInfo constructor)
  {
    IMethodSymbol constructorSymbol = constructor.Symbol;

    // Not using 'DefineTypeInitializer' because 'HideBySig' is missing in the constructor attributes.
    ConstructorBuilder constructorBuilder = _context.TypeBuilder.DefineConstructor(constructor.Attributes, constructor.CallingConvention, null);

    ILGenerator il = constructorBuilder.GetILGenerator();

    foreach ((FieldBuilder field, IOperation initOperation) in _staticInitializers)
    {
      new FieldInitializerVisitor(il).Visit(initOperation);
      il.Emit(OpCodes.Stfld, field);
    }

    IOperation operation = _context.GetOperation(constructorSymbol) ?? throw new OperationResolutionException(constructorSymbol);
    BuildMethodBody(il, typeof(void), Type.EmptyTypes, operation, constructorSymbol);

    il.Emit(OpCodes.Ret);
  }
}
