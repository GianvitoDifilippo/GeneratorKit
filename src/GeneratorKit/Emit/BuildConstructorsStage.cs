using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace GeneratorKit.Emit;

internal class BuildConstructorsStage
{
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

    if (!constructor.Symbol.IsImplicitlyDeclared)
    {
      SemanticModel semanticModel = _context.Runtime.Compilation.GetSemanticModel(constructorSymbol.DeclaringSyntaxReferences[0].SyntaxTree);
      SyntaxNode syntax = constructor.Symbol.DeclaringSyntaxReferences[0].GetSyntax();
      IOperation operation = semanticModel.GetOperation(syntax, _context.Runtime.CancellationToken) ?? throw new InvalidOperationException();

      new ConstructorInitializerOperationVisitor(_context.Runtime, il, _baseType).Visit(operation);
    }

    il.Emit(OpCodes.Ret);
}

  private void BuildStaticConstructor(SymbolConstructorInfo constructor)
  {
    ConstructorBuilder constructorBuilder = _context.TypeBuilder.DefineTypeInitializer();

    ILGenerator il = constructorBuilder.GetILGenerator();

    foreach ((FieldBuilder field, IOperation initOperation) in _staticInitializers)
    {
      new FieldInitializerOperationVisitor(il).Visit(initOperation);
      il.Emit(OpCodes.Stfld, field);
    }

    il.Emit(OpCodes.Ret);
  }
}
