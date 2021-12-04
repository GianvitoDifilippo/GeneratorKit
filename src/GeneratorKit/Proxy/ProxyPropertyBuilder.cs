using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace GeneratorKit.Proxy;

internal class ProxyPropertyBuilder
{
  private readonly IBuilderContext _context;
  private readonly TypeBuilder _typeBuilder;
  private readonly IReadOnlyDictionary<IPropertySymbol, MethodBuilder> _getters;
  private readonly IReadOnlyDictionary<IPropertySymbol, MethodBuilder> _setters;

  public ProxyPropertyBuilder(IBuilderContext context, IReadOnlyDictionary<IPropertySymbol, MethodBuilder> getters, IReadOnlyDictionary<IPropertySymbol, MethodBuilder> setters)
  {
    _context = context;
    _typeBuilder = context.TypeBuilder;
    _getters = getters;
    _setters = setters;
  }

  public void BuildProperty(SymbolPropertyInfo property)
  {
    IPropertySymbol propertySymbol = property.Symbol;

    Type returnType = _context.ResolveType(property.PropertyType);
    Type[]? parameterTypes = propertySymbol.IsIndexer
      ? property.GetIndexParameters().Select(x => _context.ResolveType(x.ParameterType)).ToArray()
      : null;
    PropertyBuilder propertyBuilder = _typeBuilder.DefineProperty(property.Name, property.Attributes, returnType, parameterTypes);

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