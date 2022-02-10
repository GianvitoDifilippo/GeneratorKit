using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace GeneratorKit.Utils;

internal class GetAllTypesVisitor : SymbolVisitor
{
  private readonly IGeneratorContext _context;
  private readonly ICollection<SymbolType> _types;

  public GetAllTypesVisitor(IGeneratorContext context, ICollection<SymbolType> types)
  {
    _context = context;
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
    _types.Add(_context.CreateTypeDelegator(symbol));
  }
}