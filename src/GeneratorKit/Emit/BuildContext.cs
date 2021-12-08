using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GeneratorKit.Emit;

internal class BuildContext : IBuildContext
{
  private readonly IReadOnlyDictionary<string, Type>? _typeGenericParameters;
  private readonly SemanticModel[] _semanticModels;

  public BuildContext(
    GeneratorRuntime runtime,
    TypeBuilder typeBuilder,
    SemanticModel[] semanticModels,
    IReadOnlyDictionary<string, Type>? typeGenericParameters)
  {
    Runtime = runtime;
    TypeBuilder = typeBuilder;
    _semanticModels = semanticModels;
    _typeGenericParameters = typeGenericParameters;
  }

  public GeneratorRuntime Runtime { get; }
  public TypeBuilder TypeBuilder { get; }

  public IOperation? GetOperation(SyntaxNode node)
  {
    foreach (SemanticModel semanticModel in _semanticModels)
    {
      if (semanticModel.GetOperation(node, Runtime.CancellationToken) is { } operation)
        return operation;
    }

    return null;
  }

  public IOperation? GetOperation(ISymbol symbol)
  {
    foreach (SyntaxReference reference in symbol.DeclaringSyntaxReferences)
    {
      SyntaxNode node = reference.GetSyntax(Runtime.CancellationToken);
      if (GetOperation(node) is { } operation)
        return operation;
    }

    return null;
  }

  public Type ResolveType(SymbolType type, IReadOnlyDictionary<string, Type>? genericParameters = null)
  {
    if (type.IsGenericType && !type.IsGenericTypeDefinition)
    {
      return ResolveGenericType(type, genericParameters);
    }
    if (type.IsGenericParameter)
    {
      return genericParameters is not null && genericParameters.TryGetValue(type.Name, out Type genericParameter)
        ? genericParameter
        : _typeGenericParameters is not null && _typeGenericParameters.TryGetValue(type.Name, out Type typeGenericParameter)
          ? typeGenericParameter
          : throw new InvalidOperationException($"Cannot resolve generic parameter {type.Name}.");
    }

    return type.UnderlyingSystemType;
  }

  private Type ResolveGenericType(SymbolType type, IReadOnlyDictionary<string, Type>? genericParameters)
  {
    Type[] typeArguments = type.GenericTypeArguments.Map(x => ResolveType(x, genericParameters));
    Type typeDefinition = ResolveType(type.GetGenericTypeDefinition(), genericParameters);
    return typeDefinition.MakeGenericType(typeArguments);
  }
}