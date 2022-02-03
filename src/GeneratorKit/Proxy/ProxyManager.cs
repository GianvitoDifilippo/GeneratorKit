using GeneratorKit.Comparers;
using GeneratorKit.Exceptions;
using GeneratorKit.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Proxy;

internal class ProxyManager : IProxyManager, IProxyTypeSetup
{
  private readonly Dictionary<Type, Type> _proxyTypes;

  public ProxyManager()
  {
    _proxyTypes = new Dictionary<Type, Type>(TypeEqualityComparer.Default);
  }

  public Type GetProxyType(IRuntimeType type)
  {
    IRuntimeType baseType = type.BaseType!;
    IRuntimeType? signatureType = null;

    bool found = _proxyTypes.TryGetValue(baseType.Definition, out Type? proxyTypeDefinition);
    if (found)
    {
      signatureType = baseType;
    }
    else
    {
      if (!baseType.Definition.Equals(typeof(object)))
        throw ProxyMatchException.NotFound(type);
    }

    foreach (IRuntimeType interfaceType in type.Interfaces)
    {
      if (_proxyTypes.TryGetValue(interfaceType.Definition, out Type? proxyTypeFromInterface))
      {
        if (found)
          throw ProxyMatchException.AmbiguousMatch(type);

        proxyTypeDefinition = proxyTypeFromInterface;
        signatureType = interfaceType;
        found = true;
      }
    }

    if (!found)
      return typeof(ObjectProxy);

    return ProxyTypeBuilderInstantiation.Create(type, signatureType!, proxyTypeDefinition);
  }

  public void RegisterProxyType(Type proxyType)
  {
    if (proxyType is null)
      throw new ArgumentNullException(nameof(proxyType));
    proxyType = proxyType.UnderlyingSystemType;

    if (proxyType.IsValueType)
      throw new ArgumentException($"'{nameof(proxyType)}' should be a reference type.", nameof(proxyType));
    if (proxyType.IsAbstract || proxyType.IsInterface)
      throw new ArgumentException($"'{nameof(proxyType)}' should be a concrete type.", nameof(proxyType));
    if (proxyType.IsConstructedGenericType)
      throw new ArgumentException($"'{nameof(proxyType)}' should not be a constructed generic type.", nameof(proxyType));

    if (proxyType.GetCustomAttribute<ProxyClassAttribute>(false) is not { } proxyClassAttribute)
      throw new ArgumentException($"'{nameof(proxyType)}' should be decorated with the {typeof(ProxyClassAttribute).Name} attribute.", nameof(proxyType));
    Type proxeeType = proxyClassAttribute.Type;

    if (proxeeType.IsInterface)
    {
      if (proxyType.BaseType != typeof(object))
        throw new ArgumentException($"'{nameof(proxyType)}' should not extend any base type.", nameof(proxyType));

      Type[] interfaceTypes = proxyType.GetInterfaces();
      if (interfaceTypes.Length != 2)
        throw new ArgumentException($"'{nameof(proxyType)}' should implement {nameof(IProxied)} and {proxeeType.Name} only.", nameof(proxyType));

      bool foundIProxiedType = false;
      bool foundProxeeType = false;
      foreach (Type interfaceType in interfaceTypes)
      {
        if (!foundProxeeType && IsProxeeType(interfaceType, proxeeType))
        {
          foundProxeeType = true;
          continue;
        }

        if (!foundIProxiedType && interfaceType == typeof(IProxied))
          foundIProxiedType = true;
      }

      if (!foundIProxiedType || !foundProxeeType)
        throw new ArgumentException($"'{nameof(proxyType)}' should implement {nameof(IProxied)} and {proxeeType.Name} only.", nameof(proxyType));
    }
    else
    {
      if (!IsProxeeType(proxyType.BaseType, proxeeType))
        throw new ArgumentException($"'{nameof(proxyType)}' should extend {proxeeType.Name}.");

      Type[] interfaceTypes = proxyType.GetInterfaces();
      if (interfaceTypes.Length != 1 || interfaceTypes[0] != typeof(IProxied))
        throw new ArgumentException($"'{nameof(proxyType)}' should implement {nameof(IProxied)} only.", nameof(proxyType));
    }

    _proxyTypes.Add(proxeeType, proxyType);
  }

  private static bool IsProxeeType(Type type, Type proxeeType)
  {
    return proxeeType.IsGenericType
      ? type.IsGenericType && type.GetGenericTypeDefinition() == proxeeType
      : type == proxeeType;
  }
}