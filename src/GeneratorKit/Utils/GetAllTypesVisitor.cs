using GeneratorKit.Reflection;
using GeneratorKit.Reflection.Context;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace GeneratorKit.Utils;

internal class GetAllTypesVisitor : SymbolVisitor
{
  private readonly IReflectionContext _context;
  private readonly ICollection<SymbolType> _types;

  public GetAllTypesVisitor(IReflectionContext context, ICollection<SymbolType> types)
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