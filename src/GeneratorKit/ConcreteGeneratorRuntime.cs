using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Threading;

namespace GeneratorKit;

internal class ConcreteGeneratorRuntime : GeneratorRuntime
{
  private readonly IProxyTypeFactory _typeFactory;
  private readonly SymbolAssembly _compilationAssembly;

  public ConcreteGeneratorRuntime(Compilation compilation, IProxyTypeFactory typeFactory, CancellationToken cancellationToken)
    : base(compilation)
  {
    _typeFactory = typeFactory;
    CancellationToken = cancellationToken;

    _compilationAssembly = new SymbolAssembly(this, compilation.Assembly, compilation.GetEntryPoint(cancellationToken));
  }

  public override SymbolAssembly CompilationAssembly => _compilationAssembly;

  public override CancellationToken CancellationToken { get; }

  public override Type? GetRuntimeType(SymbolType type)
  {
    if (type.HasElementType)
      return GetRuntimeTypeWithElement(type);

    if (type.IsConstructedGenericType)
      return GetRuntimeConstructedType(type);

    if (type.IsGenericParameter)
      return GetRuntimeTypeParameter(type);

    if (type.Symbol.ContainingAssembly is not ISourceAssemblySymbol)
      return Type.GetType(type.AssemblyQualifiedName);

    return _typeFactory.CreateProxyType(this, type);
  }

  public override MethodInfo? GetRuntimeMethod(SymbolMethodInfo method)
  {
    throw new NotImplementedException();
  }

  public override ITypeSymbol? GetTypeSymbol(Type type)
  {
    if (type is SymbolType symbolType)
      return symbolType.Symbol;

    if (type.HasElementType || type.IsGenericType)
    {
      throw new NotSupportedException("To be supported.");
    }

    return Compilation.GetTypeByMetadataName(type.FullName);
  }

  private Type? GetRuntimeTypeWithElement(SymbolType type)
  {
    SymbolType elementType = type.GetElementType()!;

    if (elementType.Symbol.ContainingAssembly is not ISourceAssemblySymbol)
      return Type.GetType(type.AssemblyQualifiedName);

    Type? runtimeElementType = GetRuntimeType(elementType);
    if (runtimeElementType is null)
      return null;

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

    return null;
  }

  private Type? GetRuntimeConstructedType(SymbolType type)
  {
    Type? runtimeGenericDefinition = GetRuntimeType(type.GetGenericTypeDefinition());
    if (runtimeGenericDefinition is null)
      return null;

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
        Type? runtimeTypeArgument = GetRuntimeType(typeArgument);
        if (runtimeTypeArgument is null)
          return null;

        runtimeTypeArguments[i] = runtimeTypeArgument;
      }
    }

    return runtimeGenericDefinition.MakeGenericType(runtimeTypeArguments);
  }

  private Type? GetRuntimeTypeParameter(SymbolType type)
  {
    throw new NotSupportedException("Cannot create runtime type for generic type parameters.");
  }
}