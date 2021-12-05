using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace GeneratorKit.Proxy;

internal class ProxyConstructorBuilder
{
  private readonly IBuilderContext _context;
  private readonly IReadOnlyCollection<InitializerData> _initializers;
  private readonly TypeBuilder _typeBuilder;

  public ProxyConstructorBuilder(IBuilderContext context, IReadOnlyCollection<InitializerData> initializers)
  {
    _context = context;
    _initializers = initializers;
    _typeBuilder = context.TypeBuilder;
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
    ConstructorBuilder constructorBuilder = _typeBuilder.DefineConstructor(constructor.Attributes, constructor.CallingConvention, parameterTypes);

    ILGenerator il = constructorBuilder.GetILGenerator();

    foreach ((FieldBuilder field, IOperation initOperation) in _initializers)
    {
      new FieldInitializerOperationVisitor(il, field).Visit(initOperation);
    }

    // new ConstructorInitializerOperationVisitor(_runtime, il, parameters).Visit(operation);

    il.Emit(OpCodes.Ret);
  }

  private void BuildStaticConstructor(SymbolConstructorInfo constructor)
  {
    ConstructorBuilder constructorBuilder = _typeBuilder.DefineTypeInitializer();

    ILGenerator il = constructorBuilder.GetILGenerator();

    il.Emit(OpCodes.Ret);
  }
}