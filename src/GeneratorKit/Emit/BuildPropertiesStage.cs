using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GeneratorKit.Emit;

internal class BuildPropertiesStage
{
  private readonly IBuildContext _context;
  private readonly IReadOnlyDictionary<IPropertySymbol, MethodBuilder> _getters;
  private readonly IReadOnlyDictionary<IPropertySymbol, MethodBuilder> _setters;

  public BuildPropertiesStage(IBuildContext context, IReadOnlyDictionary<IPropertySymbol, MethodBuilder> getters, IReadOnlyDictionary<IPropertySymbol, MethodBuilder> setters)
  {
    _context = context;
    _getters = getters;
    _setters = setters;
  }

  public void BuildProperty(SymbolPropertyInfo property)
  {
    IPropertySymbol propertySymbol = property.Symbol;

    Type returnType = _context.ResolveType(property.PropertyType);
    Type[]? parameterTypes = propertySymbol.IsIndexer
      ? property.GetIndexParameters().Map(x => _context.ResolveType(x.ParameterType))
      : null;
    PropertyBuilder propertyBuilder = _context.TypeBuilder.DefineProperty(property.Name, property.Attributes, returnType, parameterTypes);

    if (property.CanRead)
    {
      propertyBuilder.SetGetMethod(_getters[propertySymbol]);
    }
    if (property.CanWrite)
    {
      propertyBuilder.SetSetMethod(_setters[propertySymbol]);
    }
  }
}