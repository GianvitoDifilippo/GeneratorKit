#pragma warning disable RS1024 // Compare symbols correctly

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace GeneratorKit.Interpret;

internal class OperationManager : IOperationManager
{
  private readonly Compilation _compilation;
  private readonly CancellationToken _cancellationToken;
  private readonly Dictionary<ISymbol, IOperation?> _operations;

  public OperationManager(Compilation compilation, CancellationToken cancellationToken)
  {
    _compilation = compilation;
    _cancellationToken = cancellationToken;
    _operations = new Dictionary<ISymbol, IOperation?>(SymbolEqualityComparer.Default);
  }

  public bool TryGetOperation(IMethodSymbol method, [NotNullWhen(true)] out IOperation? operation)
  {
    if (method.IsImplicitlyDeclared)
    {
      operation = null;
      return false;
    }

    if (!_operations.TryGetValue(method, out operation))
    {
      TryGetMethodOperation(method, out operation);
      _operations.Add(method, operation);
    }

    return operation is not null;
  }

  public bool TryGetOperation(IFieldSymbol field, [NotNullWhen(true)] out IOperation? operation)
  {
    Debug.Assert(!field.IsImplicitlyDeclared);

    if (!_operations.TryGetValue(field, out operation))
    {
      TryGetFieldOperation(field, out operation);
      _operations.Add(field, operation);
    }

    return operation is not null;
  }

  public bool TryGetOperation(IPropertySymbol property, [NotNullWhen(true)] out IOperation? operation)
  {
    if (!_operations.TryGetValue(property, out operation))
    {
      TryGetPropertyOperation(property, out operation);
      _operations.Add(property, operation);
    }

    return operation is not null;
  }

  private SemanticModel[] GetSemanticModels(ISymbol symbol)
  {
    int length = symbol.DeclaringSyntaxReferences.Length;
    SemanticModel[] semanticModels = new SemanticModel[length];
    for (int i = 0; i < length; i++)
    {
      SyntaxReference reference = symbol.DeclaringSyntaxReferences[i];
      semanticModels[i] = _compilation.GetSemanticModel(reference.SyntaxTree);
    }

    return semanticModels;
  }

  private bool TryGetMethodOperation(IMethodSymbol symbol, [NotNullWhen(true)] out IOperation? operation)
  {
    operation = null;
    SemanticModel[] semanticModels = GetSemanticModels(symbol);
    foreach (SyntaxReference reference in symbol.DeclaringSyntaxReferences)
    {
      SyntaxNode node = reference.GetSyntax(_cancellationToken);
      if (TryGetOperation(semanticModels, node, _cancellationToken, out operation))
        return true;
    }

    return false;
  }

  private bool TryGetFieldOperation(IFieldSymbol symbol, [NotNullWhen(true)] out IOperation? operation)
  {
    operation = null;
    SemanticModel[] semanticModels = GetSemanticModels(symbol);
    foreach (SyntaxReference reference in symbol.DeclaringSyntaxReferences)
    {
      if (reference.GetSyntax(_cancellationToken) is not VariableDeclaratorSyntax node)
        continue;
      if (node.Initializer is not { } initializer)
        continue;

      if (TryGetOperation(semanticModels, initializer.Value, _cancellationToken, out operation))
      {
        return true;
      }
    }

    return false;
  }

  private bool TryGetPropertyOperation(IPropertySymbol symbol, [NotNullWhen(true)] out IOperation? operation)
  {
    operation = null;
    SemanticModel[] semanticModels = GetSemanticModels(symbol);
    foreach (SyntaxReference reference in symbol.DeclaringSyntaxReferences)
    {
      if (reference.GetSyntax(_cancellationToken) is not PropertyDeclarationSyntax node)
        continue;
      if (node.Initializer is not { } initializer)
        continue;

      if (TryGetOperation(semanticModels, initializer.Value, _cancellationToken, out operation))
      {
        return true;
      }
    }

    return false;
  }

  private static bool TryGetOperation(SemanticModel[] semanticModels, SyntaxNode node, CancellationToken cancellationToken, [NotNullWhen(true)] out IOperation? operation)
  {
    operation = null;
    foreach (SemanticModel semanticModel in semanticModels)
    {
      operation = semanticModel.GetOperation(node, cancellationToken);
      if (operation is not null)
      {
        return true;
      }
    }

    return false;
  }
}
