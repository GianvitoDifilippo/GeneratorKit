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

  public ConcreteGeneratorRuntime(Compilation compilation, IProxyManager proxyManager, IDependencyFactory dependencyFactory, CancellationToken cancellationToken)
    : base(compilation)
  {
    _proxyManager = proxyManager;
    CancellationToken = cancellationToken;
    dependencyFactory.GetDependencies(this, out _activator, out _interpreter);

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
    if (!typeof(T).IsAssignableFrom(runtimeType.RuntimeType.UnderlyingSystemType))
      throw new ArgumentException($"{type.FullName} is not assignable to {typeof(T).FullName}", nameof(type));

    return (T)_activator.CreateInstance(runtimeType, arguments);
  }

  public override object InvokeConstructor(IRuntimeConstructor constructor, object?[] arguments)
  {
    if (!constructor.Symbol.IsSource())
      return constructor.UnderlyingSystemConstructor.Invoke(arguments);

    return _activator.CreateInstance(constructor, arguments);
  }

  public override object? InvokeMethod(IRuntimeMethod method, object? instance, object?[] arguments)
  {
    if (!method.Definition.Symbol.IsSource())
      return method.UnderlyingSystemMethod.Invoke(instance, arguments);

    InterpreterFrame frame;
    if (method.IsStatic)
    {
      Debug.Assert(instance is null);

      frame = _interpreter.GetClassFrame(method.DeclaringType);
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

  public override object? GetProperty(IRuntimeProperty property, object? instance, object?[] arguments)
  {
    if (!property.Symbol.IsSource())
      return property.UnderlyingSystemProperty.GetValue(instance, arguments);

    InterpreterFrame frame;
    if (property.IsStatic)
    {
      Debug.Assert(instance is null);

      frame = _interpreter.GetClassFrame(property.DeclaringType);
    }
    else
    {
      Debug.Assert(instance is not null);
      if (instance is not IProxied proxied || proxied.Delegate is not OperationDelegate @delegate)
        throw new ArgumentException("The instance must be provided by the runtime.", nameof(instance));

      frame = @delegate.InstanceFrame;
    }

    if (frame.IsDefined(property.Symbol))
    {
      return frame.Get(property.Symbol);
    }

    IRuntimeMethod getter = property.Getter ?? throw new InvalidOperationException($"Property {property} does not have a getter.");
    return _interpreter.Interpret(getter, frame, arguments);
  }

  public override void SetProperty(IRuntimeProperty property, object? instance, object?[] arguments, object? value)
  {
    if (!property.Symbol.IsSource())
    {
      property.UnderlyingSystemProperty.SetValue(instance, value, arguments);
      return;
    }

    InterpreterFrame frame;
    if (property.IsStatic)
    {
      Debug.Assert(instance is null);

      frame = _interpreter.GetClassFrame(property.DeclaringType);
    }
    else
    {
      Debug.Assert(instance is not null);
      if (instance is not IProxied proxied || proxied.Delegate is not OperationDelegate @delegate)
        throw new ArgumentException("The instance must be provided by the runtime.", nameof(instance));

      frame = @delegate.InstanceFrame;
    }

    if (frame.IsDefined(property.Symbol))
    {
      frame.Assign(property.Symbol, value);
      return;
    }

    object?[] setterArguments;
    if (arguments.Length == 0)
    {
      setterArguments = new object?[] { value };
    }
    else
    {
      setterArguments = new object[arguments.Length + 1];
      Array.Copy(arguments, setterArguments, arguments.Length);
      setterArguments[arguments.Length] = value;
    }

    IRuntimeMethod setter = property.Setter ?? throw new InvalidOperationException($"Property {property} does not have a setter.");
    _interpreter.Interpret(setter, frame, setterArguments);
  }
  
  public override object? GetField(IRuntimeField field, object? instance)
  {
    if (!field.Symbol.IsSource())
      return field.UnderlyingSystemField.GetValue(instance);

    InterpreterFrame frame;
    if (field.IsStatic)
    {
      Debug.Assert(instance is null);

      frame = _interpreter.GetClassFrame(field.DeclaringType);
    }
    else
    {
      Debug.Assert(instance is not null);
      if (instance is not IProxied proxied || proxied.Delegate is not OperationDelegate @delegate)
        throw new ArgumentException("The instance must be provided by the runtime.", nameof(instance));

      frame = @delegate.InstanceFrame;
    }

    return frame.Get(field.Symbol);
  }

  public override void SetField(IRuntimeField field, object? instance, object? value)
  {
    if (!field.Symbol.IsSource())
    {
      field.UnderlyingSystemField.SetValue(instance, value);
      return;
    }

    InterpreterFrame frame;
    if (field.IsStatic)
    {
      Debug.Assert(instance is null);

      frame = _interpreter.GetClassFrame(field.DeclaringType);
    }
    else
    {
      Debug.Assert(instance is not null);
      if (instance is not IProxied proxied || proxied.Delegate is not OperationDelegate @delegate)
        throw new ArgumentException("The instance must be provided by the runtime.", nameof(instance));

      frame = @delegate.InstanceFrame;
    }

    frame.Assign(field.Symbol, value);
  }

  public override Type GetRuntimeType(IRuntimeType type)
  {
    if (type.HasElementType)
      return GetRuntimeTypeWithElement(type);

    if (type.IsConstructedGenericType)
      return GetRuntimeConstructedType(type.Definition, type.TypeArguments);

    if (type.IsGenericParameter)
      return GetRuntimeTypeParameter(type);

    // _cache.CacheType(type);

    if (!type.Definition.Symbol.IsSource())
      return GetReferencedType(type);

    return _proxyManager.GetProxyType(type);
  }

  private Type GetRuntimeTypeWithElement(IRuntimeType type)
  {
    Type runtimeElementType = type.ElementType.RuntimeType;

    if (type.IsArray)
    {
      int rank = type.ArrayRank;
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

  private Type GetRuntimeConstructedType(IRuntimeType typeDefinition, Type[] typeArguments)
  {
    Type runtimeGenericDefinition = typeDefinition.RuntimeType;
    if (runtimeGenericDefinition == typeof(ObjectProxy))
      return runtimeGenericDefinition;

    return runtimeGenericDefinition.MakeGenericType(typeArguments.Map(t => t.UnderlyingSystemType));
  }

  private Type GetRuntimeTypeParameter(IRuntimeType type)
  {
    return type.DeclaringType!.TypeParameters[type.GenericParameterPosition];
  }

  private static Type GetReferencedType(IRuntimeType type)
  {
    return Type.GetType(type.AssemblyQualifiedName, true, false);
  }
}