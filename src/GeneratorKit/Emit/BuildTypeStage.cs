#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace GeneratorKit.Emit;

internal class BuildTypeStage
{
  private readonly IBuildContext _context;
  private readonly SymbolType _type;

  public BuildTypeStage(IBuildContext context, SymbolType type)
  {
    _context = context;
    _type = type;

    BaseType = SetBaseType();
    InterfaceTypes = SetInterfaces();
  }

  public Type BaseType { get; }
  public IReadOnlyDictionary<ITypeSymbol, Type> InterfaceTypes { get; }

  private Type SetBaseType()
  {
    if (_type.BaseType is null)
      throw new InvalidOperationException($"Cannot build a type which base type is null.");

    Type baseType = _context.ResolveType(_type.BaseType);
    _context.TypeBuilder.SetParent(baseType);

    return baseType;
  }

  private IReadOnlyDictionary<ITypeSymbol, Type> SetInterfaces()
  {
    Dictionary<ITypeSymbol, Type> interfaceTypes = new Dictionary<ITypeSymbol, Type>(SymbolEqualityComparer.Default);
    
    foreach (SymbolType interfaceType in _type.GetInterfaces())
    {
      Type resolvedType = _context.ResolveType(interfaceType);
      _context.TypeBuilder.AddInterfaceImplementation(resolvedType);
      interfaceTypes.Add(interfaceType.Symbol, resolvedType);
    }

    return interfaceTypes;
  }
}