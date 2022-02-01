using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace GeneratorKit.Interpret;

internal interface IOperationManager
{
  bool TryGetOperation(IMethodSymbol method, [NotNullWhen(true)] out IOperation? operation);
  bool TryGetOperation(IFieldSymbol field, [NotNullWhen(true)] out IOperation? operation);
  bool TryGetOperation(IPropertySymbol property, [NotNullWhen(true)] out IOperation? operation);
}