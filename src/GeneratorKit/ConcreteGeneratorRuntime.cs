#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Exceptions;
using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GeneratorKit;

internal class ConcreteGeneratorRuntime : GeneratorRuntime
{
  private readonly IProxyTypeFactory _typeFactory;
  private readonly SymbolAssembly _compilationAssembly;
  private readonly Dictionary<ITypeSymbol, SymbolType> _typeCache;

  public ConcreteGeneratorRuntime(Compilation compilation, IProxyTypeFactory typeFactory, CancellationToken cancellationToken)
    : base(compilation)
  {
    _typeFactory = typeFactory;
    CancellationToken = cancellationToken;

    _typeCache = new Dictionary<ITypeSymbol, SymbolType>(SymbolEqualityComparer.Default);
    _compilationAssembly = new SymbolAssembly(this, compilation.Assembly, compilation.GetEntryPoint(cancellationToken));
  }

  public override SymbolAssembly CompilationAssembly => _compilationAssembly;

  public override CancellationToken CancellationToken { get; }

  public override SymbolType CreateTypeDelegator(ITypeSymbol symbol)
  {
    if (_typeCache.TryGetValue(symbol, out SymbolType? type))
      return type;

    return base.CreateTypeDelegator(symbol);
  }

  public override Type GetRuntimeType(SymbolType type)
  {
    try
    {
      Type result = GetRuntimeTypeNoCatch(type);
      _typeCache[type.Symbol] = type;
      return result;
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      throw new TypeCreationException(type, ex);
    }
  }

  private Type GetRuntimeTypeNoCatch(SymbolType type)
  {
    if (type.HasElementType)
      return GetRuntimeTypeWithElement(type);

    if (type.IsConstructedGenericType)
      return GetRuntimeConstructedType(type);

    if (type.IsGenericParameter)
      return GetRuntimeTypeParameter(type);

    if (IsProvidedByUs(type, out Type? providedType))
      return providedType!;

    if (type.Symbol.ContainingAssembly is not ISourceAssemblySymbol)
      return GetLoadedType(type);

    return _typeFactory.CreateProxyType(this, type);
  }

  private Type GetRuntimeTypeWithElement(SymbolType type)
  {
    SymbolType elementType = type.GetElementType()!;

    if (elementType.Symbol.ContainingAssembly is not ISourceAssemblySymbol)
      return Type.GetType(type.AssemblyQualifiedName);

    Type runtimeElementType = elementType.UnderlyingSystemType;

    if (type.IsArray)
    {
      int rank = type.GetArrayRank();
      return rank == 1
        ? runtimeElementType.MakeArrayType()
        : runtimeElementType.MakeArrayType(rank);
    }
    if (type.IsPointer)
    {
      return runtimeElementType.MakePointerType();
    }
    if (type.IsByRef)
    {
      return runtimeElementType.MakeByRefType();
    }

    throw Errors.Unreacheable;
  }

  private Type GetRuntimeConstructedType(SymbolType type)
  {
    Type runtimeGenericDefinition = type.GetGenericTypeDefinition().UnderlyingSystemType;

    Type[]? runtimeDefinitionTypeArguments = null;
    SymbolType[] typeArguments = type.GenericTypeArguments;
    Type[] runtimeTypeArguments = new Type[typeArguments.Length];
    for (int i = 0; i < typeArguments.Length; i++)
    {
      SymbolType typeArgument = typeArguments[i];
      if (typeArgument.IsGenericParameter)
      {
        runtimeDefinitionTypeArguments ??= runtimeGenericDefinition.GenericTypeArguments;
        runtimeTypeArguments[i] = runtimeDefinitionTypeArguments[i];
      }
      else
      {
        runtimeTypeArguments[i] = typeArgument.UnderlyingSystemType;
      }
    }

    return runtimeGenericDefinition.MakeGenericType(runtimeTypeArguments);
  }

  private Type GetRuntimeTypeParameter(SymbolType type)
  {
    if (type.DeclaringType is not SymbolType declaringType)
      throw new NotSupportedException("Generic parameter was supposed to have a declaring type.");

    Type runtimeDeclaringType = declaringType.UnderlyingSystemType;

    return runtimeDeclaringType.GenericTypeArguments[type.GenericParameterPosition];
  }

  private static Type GetLoadedType(Type type)
  {
    return Type.GetType(type.AssemblyQualifiedName, true, false);
  }

  private static bool IsProvidedByUs(SymbolType type, out Type? providedType)
  {
    switch (type.FullName)
    {
      case "System.Index":
        providedType = typeof(Index);
        return true;
      case "System.Range":
        providedType = typeof(Range);
        return true;
      default:
        providedType = null;
        return false;
    }
  }
}