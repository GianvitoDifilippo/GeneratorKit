using GeneratorKit.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using System;
using System.Linq;

namespace GeneratorKit.Emit;

internal class BuildContext : IBuildContext
{
  private readonly IReadOnlyDictionary<string, Type>? _typeGenericParameters;

  public BuildContext(GeneratorRuntime runtime, TypeBuilder typeBuilder, IReadOnlyDictionary<string, Type>? typeGenericParameters)
  {
    Runtime = runtime;
    TypeBuilder = typeBuilder;
    _typeGenericParameters = typeGenericParameters;
  }

  public GeneratorRuntime Runtime { get; }
  public TypeBuilder TypeBuilder { get; }

  public Type ResolveType(SymbolType type, IReadOnlyDictionary<string, Type>? genericParameters = null)
  {
    if (type.IsGenericType && !type.IsGenericTypeDefinition)
    {
      return ResolveGenericType(type, genericParameters);
    }
    if (type.IsGenericParameter)
    {
      return genericParameters is not null && genericParameters.TryGetValue(type.Name, out Type genericParameter)
        ? genericParameter
        : _typeGenericParameters is not null && _typeGenericParameters.TryGetValue(type.Name, out Type typeGenericParameter)
          ? typeGenericParameter
          : throw new InvalidOperationException($"Cannot resolve generic parameter {type.Name}.");
    }

    return type.UnderlyingSystemType;
  }

  private Type ResolveGenericType(SymbolType type, IReadOnlyDictionary<string, Type>? genericParameters)
  {
    Type[] typeArguments = type.GenericTypeArguments.Select(x => ResolveType(x, genericParameters)).ToArray();
    Type typeDefinition = ResolveType(type.GetGenericTypeDefinition(), genericParameters);
    return typeDefinition.MakeGenericType(typeArguments);
  }
}