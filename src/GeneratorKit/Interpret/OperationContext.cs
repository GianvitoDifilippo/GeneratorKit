using Microsoft.CodeAnalysis;

namespace GeneratorKit.Interpret;

internal record OperationContext(GeneratorRuntime Runtime, IOperation Operation, IMethodSymbol Method);