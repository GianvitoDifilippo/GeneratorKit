#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace GeneratorKit.Proxy;

internal class ProxyFieldBuilder
{
  private const BindingFlags s_allDeclared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

  private ProxyFieldBuilder(IReadOnlyDictionary<IPropertySymbol, FieldBuilder> backingFields, IReadOnlyDictionary<FieldBuilder, ExpressionSyntax> initializers)
  {
    BackingFields = backingFields;
    Initializers = initializers;
  }

  public IReadOnlyDictionary<IPropertySymbol, FieldBuilder> BackingFields { get; }

  public IReadOnlyDictionary<FieldBuilder, ExpressionSyntax> Initializers { get; }

  public static ProxyFieldBuilder Create(TypeBuilder typeBuilder, SymbolType type)
  {
    SymbolFieldInfo[] fields = type.GetFields(s_allDeclared);
    Dictionary<IPropertySymbol, FieldBuilder> backingFields = new Dictionary<IPropertySymbol, FieldBuilder>(SymbolEqualityComparer.Default);
    Dictionary<FieldBuilder, ExpressionSyntax> initializers = new Dictionary<FieldBuilder, ExpressionSyntax>(); 

    foreach (SymbolFieldInfo field in fields)
    {
      IFieldSymbol fieldSymbol = field.Symbol;
      
      FieldBuilder fieldBuilder = typeBuilder.DefineField(
        fieldName: field.Name,
        type: field.FieldType.RuntimeType,
        attributes: field.Attributes);

      if (fieldSymbol.IsImplicitlyDeclared && fieldSymbol.AssociatedSymbol is IPropertySymbol propertySymbol)
      {
        backingFields.Add(propertySymbol, fieldBuilder);
      }

      if (field.Symbol.DeclaringSyntaxReferences.Length == 0)
        continue;
      if (field.Symbol.DeclaringSyntaxReferences[0].GetSyntax() is not VariableDeclaratorSyntax syntax)
        continue;
      if (syntax.Initializer?.Value is not ExpressionSyntax expression)
        continue;
      
      initializers.Add(fieldBuilder, expression);
    }

    return new ProxyFieldBuilder(backingFields, initializers);
  }
}