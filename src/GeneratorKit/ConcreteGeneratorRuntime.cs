using GeneratorKit.Interpret;
using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Threading;

namespace GeneratorKit;

internal class ConcreteGeneratorRuntime : GeneratorRuntime
{
  private readonly SymbolAssembly _compilationAssembly;
  // private readonly ReflectionCache _cache;
  private readonly IProxyManager _proxyManager;
  private readonly IActivator _activator;
  private readonly IInterpreter _interpreter;
  private readonly IFrameProvider _frameProvider;

  public ConcreteGeneratorRuntime(Compilation compilation, IProxyManager proxyManager, IDependencyFactory dependencyFactory, CancellationToken cancellationToken)
    : base(compilation)
  {
    _proxyManager = proxyManager;
    CancellationToken = cancellationToken;
    dependencyFactory.GetDependencies(this, out _activator, out _interpreter, out _frameProvider);

    _compilationAssembly = new SymbolAssembly(this, compilation.Assembly, compilation.GetEntryPoint(cancellationToken));
    // _cache = new ReflectionCache();
  }

  public override SymbolAssembly CompilationAssembly => _compilationAssembly;

  public override CancellationToken CancellationToken { get; }

  public override T CreateInstance<T>(Type type, params object?[] arguments)
  {
    if (type.IsArray || type.IsPointer || type.IsByRef)
      throw new NotSupportedException("Arrays, pointers and by ref types are not supported.");
    if (type is not IRuntimeType runtimeType)
      throw new ArgumentException("The type must be provided by this runtime.", nameof(type));

    return _activator.CreateInstance<T>(runtimeType, arguments);
  }

  public override object? InvokeMethod(IRuntimeMethod method, object? instance, object?[] arguments)
  {
    Debug.Assert(method.IsSource);
    InterpreterFrame frame;
    if (method.IsStatic)
    {
      Debug.Assert(instance is null);

      frame = _frameProvider.GetClassFrame(method.DeclaringType);
    }
    else
    {
      Debug.Assert(instance is not null);
      if (instance is not IProxied proxied || proxied.Delegate is not OperationDelegate @delegate)
        throw new ArgumentException("The instance must be provided by the runtime.", nameof(instance));

      frame = @delegate.InstanceFrame;
    }

    return _interpreter.Interpret(method, frame, arguments);
  }

  public override Type GetRuntimeType(SymbolType type)
  {
    if (type.HasElementType)
      return GetRuntimeTypeWithElement(type);

    if (type.IsConstructedGenericType)
      return GetRuntimeConstructedType(type.GetGenericTypeDefinition(), type.GetGenericArguments());

    if (type.IsGenericParameter)
      return GetRuntimeTypeParameter(type);

    // _cache.CacheType(type);

    if (type.Symbol.ContainingAssembly is not ISourceAssemblySymbol)
      return GetReferencedType(type);

    return _proxyManager.GetProxyType(type);
  }

  public override Type GetRuntimeType(HybridGenericType type)
  {
    return GetRuntimeConstructedType(type.GetGenericTypeDefinition(), type.GetGenericArguments());
  }

  private Type GetRuntimeTypeWithElement(Type type)
  {
    Type runtimeElementType = type.GetElementType()!.UnderlyingSystemType;

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

  private Type GetRuntimeConstructedType(SymbolType typeDefinition, Type[] typeArguments)
  {
    Type runtimeGenericDefinition = typeDefinition.RuntimeType;

    Type[]? runtimeDefinitionTypeArguments = null;
    Type[] runtimeTypeArguments = new Type[typeArguments.Length];
    for (int i = 0; i < typeArguments.Length; i++)
    {
      Type typeArgument = typeArguments[i];
      if (typeArgument.IsGenericParameter)
      {
        runtimeDefinitionTypeArguments ??= typeArgument.DeclaringType!.UnderlyingSystemType.GetGenericArguments(); // TODO: This can be optimized
        runtimeTypeArguments[i] = runtimeDefinitionTypeArguments[i];
      }
      else
      {
        runtimeTypeArguments[i] = typeArgument.UnderlyingSystemType;
      }
    }

    return runtimeGenericDefinition.MakeGenericType(runtimeTypeArguments);
  }

  private Type GetRuntimeTypeParameter(Type type)
  {
    if (type.DeclaringType is not SymbolType declaringType)
      throw new NotSupportedException("Generic parameter was supposed to have a declaring type.");

    Type runtimeDeclaringType = declaringType.UnderlyingSystemType;

    return runtimeDeclaringType.GenericTypeArguments[type.GenericParameterPosition];
  }

  private static Type GetReferencedType(SymbolType type)
  {
    return Type.GetType(type.AssemblyQualifiedName, true, false);
  }
}