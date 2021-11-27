using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace GeneratorKit.Utils;

internal class GetAllTypesVisitor : SymbolVisitor
{
  private readonly GeneratorRuntime _runtime;
  private readonly ICollection<SymbolType> _types;

  public GetAllTypesVisitor(GeneratorRuntime runtime, ICollection<SymbolType> types)
  {
    _runtime = runtime;
    _types = types;
  }

  public override void VisitNamespace(INamespaceSymbol symbol)
  {
    foreach (INamespaceOrTypeSymbol member in symbol.GetMembers())
    {
      member.Accept(this);
    }
  }

  public override void VisitNamedType(INamedTypeSymbol symbol)
  {
    _types.Add(_runtime.CreateTypeDelegator(symbol));
  }
}