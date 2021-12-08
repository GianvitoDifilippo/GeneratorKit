#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GeneratorKit.Emit;

internal class BuildFieldsStage
{
  private readonly IBuildContext _context;
  private readonly Dictionary<IPropertySymbol, FieldBuilder> _backingFields;
  private readonly List<InitializerData> _instanceInitializers;
  private readonly List<InitializerData> _staticInitializers;

  public BuildFieldsStage(IBuildContext context)
  {
    _context = context;
    _backingFields = new Dictionary<IPropertySymbol, FieldBuilder>(SymbolEqualityComparer.Default);
    _instanceInitializers = new List<InitializerData>();
    _staticInitializers = new List<InitializerData>();
  }

  public IReadOnlyDictionary<IPropertySymbol, FieldBuilder> BackingFields => _backingFields;

  public IReadOnlyCollection<InitializerData> InstanceInitializers => _instanceInitializers;

  public IReadOnlyCollection<InitializerData> StaticInitializers => _staticInitializers;

  public void BuildField(SymbolFieldInfo field)
  {
    IFieldSymbol fieldSymbol = field.Symbol;

    Type fieldType = _context.ResolveType(field.FieldType);
    FieldBuilder fieldBuilder = _context.TypeBuilder.DefineField(field.Name, fieldType, field.Attributes);

    if (fieldSymbol.IsImplicitlyDeclared && fieldSymbol.AssociatedSymbol is IPropertySymbol propertySymbol)
    {
      _backingFields.Add(propertySymbol, fieldBuilder);
    }

    if (fieldSymbol.DeclaringSyntaxReferences.Length == 0)
      return;
    if (fieldSymbol.DeclaringSyntaxReferences[0].GetSyntax(_context.Runtime.CancellationToken) is not VariableDeclaratorSyntax syntax)
      return;
    if (syntax.Initializer?.Value is not ExpressionSyntax expression)
      return;

    if (_context.GetOperation(expression) is not IOperation initOperation)
      return;

    InitializerData data = new InitializerData(fieldBuilder, initOperation);
    if (field.IsStatic)
    {
      _staticInitializers.Add(data);
    }
    else
    {
      _instanceInitializers.Add(data);
    }
  }
}