using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GeneratorKit.Proxy;

internal class ProxyPropertyBuilder
{
  private const BindingFlags s_allDeclared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

  private readonly IReadOnlyCollection<PropertyData> _properties;

  private ProxyPropertyBuilder(IReadOnlyCollection<PropertyData> properties)
  {
    _properties = properties;
  }

  public void Build(IReadOnlyDictionary<IPropertySymbol, MethodBuilder> getters, IReadOnlyDictionary<IPropertySymbol, MethodBuilder> setters)
  {
    foreach ((PropertyBuilder builder, IPropertySymbol symbol) in _properties)
    {
      if (getters.TryGetValue(symbol, out MethodBuilder getter))
      {
        builder.SetGetMethod(getter);
      }
      if (setters.TryGetValue(symbol, out MethodBuilder setter))
      {
        builder.SetSetMethod(setter);
      }
    }
  }

  public static ProxyPropertyBuilder Create(TypeBuilder typeBuilder, SymbolType type)
  {
    SymbolPropertyInfo[] properties = type.GetProperties(s_allDeclared);
    List<PropertyData> propertyList = new List<PropertyData>(properties.Length);

    foreach (SymbolPropertyInfo property in properties)
    {
      IPropertySymbol propertySymbol = property.Symbol;

      PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
        name: property.Name,
        attributes: property.Attributes,
        returnType: property.PropertyType.RuntimeType,
        parameterTypes: propertySymbol.IsIndexer
          ? property.GetIndexParameters().Select(x => x.ParameterType.RuntimeType).ToArray()
          : null);

      propertyList.Add(new PropertyData(propertyBuilder, propertySymbol));
    }

    return new ProxyPropertyBuilder(propertyList);
  }

  private readonly record struct PropertyData(PropertyBuilder Builder, IPropertySymbol Symbol);
}