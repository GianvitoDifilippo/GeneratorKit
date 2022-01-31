using GeneratorKit.Interpret;
using Microsoft.CodeAnalysis;

namespace GeneratorKit.TestHelpers;

internal class FakeOperationManager : IOperationManager
{
  private readonly Compilation _compilation;

  public FakeOperationManager(Compilation compilation)
  {
    _compilation = compilation;
  }

  public bool TryGetOperation(IMethodSymbol method, out IOperation? operation)
  {
    operation = null;
    if (method.DeclaringSyntaxReferences.Length == 0)
      return false;

    SyntaxReference syntaxReference = method.DeclaringSyntaxReferences[0];
    operation = _compilation.GetSemanticModel(syntaxReference.SyntaxTree).GetOperation(syntaxReference.GetSyntax());
    return operation is not null;
  }
}
