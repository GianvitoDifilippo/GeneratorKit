using Microsoft.CodeAnalysis;

namespace GeneratorKit.Interpret;

internal interface IOperationManager
{
  bool TryGetOperation(IMethodSymbol method, out IOperation? operation);
}