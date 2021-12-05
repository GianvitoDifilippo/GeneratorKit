#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GeneratorKit.Proxy;

internal class ProxyFieldBuilder
{
  private readonly IBuilderContext _context;
  private readonly TypeBuilder _typeBuilder;
  private readonly Dictionary<IPropertySymbol, FieldBuilder> _backingFields;
  private readonly List<InitializerData> _initializers;

  public ProxyFieldBuilder(IBuilderContext context)
  {
    _context = context;
    _typeBuilder = context.TypeBuilder;
    _backingFields = new Dictionary<IPropertySymbol, FieldBuilder>(SymbolEqualityComparer.Default);
    _initializers = new List<InitializerData>();
  }

  public IReadOnlyDictionary<IPropertySymbol, FieldBuilder> BackingFields => _backingFields;

  public IReadOnlyCollection<InitializerData> Initializers => _initializers;

  public void BuildField(SymbolFieldInfo field)
  {
    IFieldSymbol fieldSymbol = field.Symbol;

    Type fieldType = _context.ResolveType(field.FieldType);
    FieldBuilder fieldBuilder = _typeBuilder.DefineField(field.Name, fieldType, field.Attributes);

    if (fieldSymbol.IsImplicitlyDeclared && fieldSymbol.AssociatedSymbol is IPropertySymbol propertySymbol)
    {
      _backingFields.Add(propertySymbol, fieldBuilder);
    }

    if (field.Symbol.DeclaringSyntaxReferences.Length == 0)
      return;
    if (field.Symbol.DeclaringSyntaxReferences[0].GetSyntax() is not VariableDeclaratorSyntax syntax)
      return;
    if (syntax.Initializer?.Value is not ExpressionSyntax expression)
      return;

    SemanticModel semanticModel = _context.Runtime.Compilation.GetSemanticModel(field.Symbol.DeclaringSyntaxReferences[0].SyntaxTree);
    if (semanticModel.GetOperation(expression, _context.Runtime.CancellationToken) is not IOperation initOperation)
      return;

    _initializers.Add(new InitializerData(fieldBuilder, initOperation));
  }
}