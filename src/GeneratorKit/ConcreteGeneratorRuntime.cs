using GeneratorKit.Exceptions;
using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
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

  public override Type GetRuntimeType(SymbolType type)
  {
    try
    {
      return GetRuntimeTypeNoCatch(type);
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

    if (type.Symbol.ContainingAssembly is not ISourceAssemblySymbol)
      return GetReferencedType(type);

    return _typeFactory.CreateProxyType(this, type);
  }

  private Type GetRuntimeTypeWithElement(SymbolType type)
  {
    SymbolType elementType = type.GetElementType()!;

    if (elementType.Symbol.ContainingAssembly is not ISourceAssemblySymbol)
      return Type.GetType(type.AssemblyQualifiedName);

    Type runtimeElementType = GetRuntimeType(elementType);

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
    Type runtimeGenericDefinition = GetRuntimeType(type.GetGenericTypeDefinition());

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
        Type runtimeTypeArgument = GetRuntimeType(typeArgument);

        runtimeTypeArguments[i] = runtimeTypeArgument;
      }
    }

    return runtimeGenericDefinition.MakeGenericType(runtimeTypeArguments);
  }

  private Type GetRuntimeTypeParameter(SymbolType type)
  {
    if (type.DeclaringType is not SymbolType declaringType)
      throw new NotSupportedException("Generic parameter was supposed to have a declaring type.");

    Type runtimeDeclaringType = GetRuntimeType(declaringType);

    return runtimeDeclaringType.GenericTypeArguments[type.GenericParameterPosition];
  }

  private static Type GetReferencedType(Type type)
  {
    return Type.GetType(type.AssemblyQualifiedName, true, false);
  }
}