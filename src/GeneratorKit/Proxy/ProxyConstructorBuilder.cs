using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;

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

    if (!constructor.Symbol.IsImplicitlyDeclared)
    {
      SemanticModel semanticModel = _context.Runtime.Compilation.GetSemanticModel(constructorSymbol.DeclaringSyntaxReferences[0].SyntaxTree);
      SyntaxNode syntax = constructor.Symbol.DeclaringSyntaxReferences[0].GetSyntax();
      IOperation operation = semanticModel.GetOperation(syntax, _context.Runtime.CancellationToken) ?? throw new InvalidOperationException();
      new ConstructorInitializerOperationVisitor(_context.Runtime, il, constructor.GetParameters()).Visit(operation);
    }

    il.Emit(OpCodes.Ret);
  }

  private void BuildStaticConstructor(SymbolConstructorInfo constructor)
  {
    ConstructorBuilder constructorBuilder = _typeBuilder.DefineTypeInitializer();

    ILGenerator il = constructorBuilder.GetILGenerator();

    il.Emit(OpCodes.Ret);
  }
}