using GeneratorKit.Interpret;
using GeneratorKit.Interpret.Frame;
using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace GeneratorKit;

internal class GeneratorRuntime : IReflectionRuntime, IGeneratorRuntime
{
  private readonly IGeneratorContext _context;
  private readonly IActivator _activator;
  private readonly IProxyManager _proxyManager;
  private readonly IInterpreter _interpreter;
  private readonly CancellationToken _cancellationToken;
  private SymbolAssembly? _compilationAssembly;

  public GeneratorRuntime(Compilation compilation, IDependencyFactory dependencyFactory, CancellationToken cancellationToken)
  {
    Compilation = compilation;
    _cancellationToken = cancellationToken;
    dependencyFactory.CreateDependencies(this, out _context, out _activator, out _proxyManager, out _interpreter);
  }

  public Compilation Compilation { get; }

  public Assembly CompilationAssembly => _compilationAssembly ??= new SymbolAssembly(_context, Compilation.Assembly, Compilation.GetEntryPoint(_cancellationToken));

  public T CreateInstance<T>(Type type, object?[] arguments)
  {
    if (type.IsArray || type.IsPointer || type.IsByRef)
      throw new NotSupportedException("Arrays, pointers and by ref types are not supported.");
    if (type is not SymbolType symbolType)
      throw new ArgumentException("The type must be provided by this runtime.", nameof(type));
    if (!typeof(T).IsAssignableFrom(symbolType.UnderlyingSystemType))
      throw new ArgumentException($"{type.FullName} is not assignable to {typeof(T).FullName}", nameof(type));

    return (T)_activator.CreateInstance(symbolType, arguments);
  }

  public Assembly CreateAssemblyDelegator(IAssemblySymbol symbol)
  {
    return _context.CreateAssemblyDelegator(symbol);
  }

  public ConstructorInfo CreateConstructorInfoDelegator(IMethodSymbol symbol)
  {
    return _context.CreateConstructorInfoDelegator(symbol);
  }

  public FieldInfo CreateFieldInfoDelegator(IFieldSymbol symbol)
  {
    return _context.CreateFieldInfoDelegator(symbol);
  }

  public MethodInfo CreateMethodInfoDelegator(IMethodSymbol symbol)
  {
    return _context.CreateMethodInfoDelegator(symbol);
  }

  public Module CreateModuleDelegator(IModuleSymbol symbol)
  {
    return _context.CreateModuleDelegator(symbol);
  }

  public PropertyInfo CreatePropertyInfoDelegator(IPropertySymbol symbol)
  {
    return _context.CreatePropertyInfoDelegator(symbol);
  }

  public Type CreateTypeDelegator(ITypeSymbol symbol)
  {
    return _context.CreateTypeDelegator(symbol);
  }

  public object InvokeConstructor(SymbolConstructorInfo constructor, object?[] arguments)
  {
    if (!constructor.IsSource)
      return constructor.UnderlyingSystemConstructor.Invoke(arguments);

    return _activator.CreateInstance(constructor, arguments);
  }

  public object? InvokeMethod(SymbolMethodInfo method, object? instance, object?[] arguments)
  {
    if (!method.IsSource)
      return method.UnderlyingSystemMethod.Invoke(instance, arguments);

    InterpreterFrame frame;
    if (method.IsStatic)
    {
      Debug.Assert(instance is null, "Instance of non-static method must not be null.");

      frame = _interpreter.GetTypeFrame(method.DeclaringType);
    }
    else
    {
      Debug.Assert(instance is not null, "Instance of static method must be null.");
      if (instance is not IProxied proxied || proxied.Delegate is not OperationDelegate @delegate)
        throw new ArgumentException("The instance must be provided by the runtime.", nameof(instance));

      frame = @delegate.InstanceFrame;
    }

    return _interpreter.InterpretMethod(method.OriginalSymbol, frame, method.GetGenericArguments(), arguments);
  }

  public object? GetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments)
  {
    if (!property.IsSource)
      return property.UnderlyingSystemProperty.GetValue(instance, arguments);

    InterpreterFrame frame;
    if (property.IsStatic)
    {
      Debug.Assert(instance is null, "Instance of non-static property must not be null.");

      frame = _interpreter.GetTypeFrame(property.DeclaringType);
    }
    else
    {
      Debug.Assert(instance is not null, "Instance of static property must be null.");
      if (instance is not IProxied proxied || proxied.Delegate is not OperationDelegate @delegate)
        throw new ArgumentException("The instance must be provided by the runtime.", nameof(instance));

      frame = @delegate.InstanceFrame;
    }

    if (frame.IsDefined(property.OriginalSymbol))
    {
      return frame.Get(property.OriginalSymbol);
    }

    IMethodSymbol getter = property.OriginalSymbol.GetMethod ?? throw new InvalidOperationException($"Property {property} does not have a getter.");
    return _interpreter.InterpretMethod(getter, frame, Type.EmptyTypes, arguments);
  }

  public void SetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments, object? value)
  {
    if (!property.IsSource)
    {
      property.UnderlyingSystemProperty.SetValue(instance, value, arguments);
      return;
    }

    InterpreterFrame frame;
    if (property.IsStatic)
    {
      Debug.Assert(instance is null, "Instance of non-static property must not be null.");

      frame = _interpreter.GetTypeFrame(property.DeclaringType);
    }
    else
    {
      Debug.Assert(instance is not null, "Instance of static property must be null.");
      if (instance is not IProxied proxied || proxied.Delegate is not OperationDelegate @delegate)
        throw new ArgumentException("The instance must be provided by the runtime.", nameof(instance));

      frame = @delegate.InstanceFrame;
    }

    if (frame.IsDefined(property.OriginalSymbol))
    {
      frame.Assign(property.OriginalSymbol, value);
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

    IMethodSymbol setter = property.OriginalSymbol.SetMethod ?? throw new InvalidOperationException($"Property {property} does not have a setter.");
    _interpreter.InterpretMethod(setter, frame, Type.EmptyTypes, setterArguments);
  }

  public object? GetField(SymbolFieldInfo field, object? instance)
  {
    if (!field.IsSource)
      return field.UnderlyingSystemField.GetValue(instance);

    InterpreterFrame frame;
    if (field.IsStatic)
    {
      Debug.Assert(instance is null, "Instance of non-static field must not be null.");

      frame = _interpreter.GetTypeFrame(field.DeclaringType);
    }
    else
    {
      Debug.Assert(instance is not null, "Instance of static field must be null.");
      if (instance is not IProxied proxied || proxied.Delegate is not OperationDelegate @delegate)
        throw new ArgumentException("The instance must be provided by the runtime.", nameof(instance));

      frame = @delegate.InstanceFrame;
    }

    return frame.Get(field.OriginalSymbol);
  }

  public void SetField(SymbolFieldInfo field, object? instance, object? value)
  {
    if (!field.IsSource)
    {
      field.UnderlyingSystemField.SetValue(instance, value);
      return;
    }

    InterpreterFrame frame;
    if (field.IsStatic)
    {
      Debug.Assert(instance is null, "Instance of non-static field must not be null.");

      frame = _interpreter.GetTypeFrame(field.DeclaringType);
    }
    else
    {
      Debug.Assert(instance is not null, "Instance of static field must be null.");
      if (instance is not IProxied proxied || proxied.Delegate is not OperationDelegate @delegate)
        throw new ArgumentException("The instance must be provided by the runtime.", nameof(instance));

      frame = @delegate.InstanceFrame;
    }

    frame.Assign(field.OriginalSymbol, value);
  }

  public Type GetRuntimeType(SymbolType type)
  {
    if (type.HasElementType)
      return GetRuntimeTypeWithElement(type);

    if (type.IsConstructedGenericType)
      return GetRuntimeConstructedType(type.GetGenericTypeDefinition(), type.GetGenericArguments());

    if (type.IsGenericParameter)
      return GetRuntimeTypeParameter(type);

    if (!type.IsSource)
      return GetReferencedType(type);

    return _proxyManager.GetProxyType(type);
  }

  private Type GetRuntimeTypeWithElement(SymbolType type)
  {
    Type runtimeElementType = type.GetElementType()!.RuntimeType;

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
    if (runtimeGenericDefinition == typeof(ObjectProxy))
      return runtimeGenericDefinition;

    return runtimeGenericDefinition.MakeGenericType(typeArguments.Map(t => t.UnderlyingSystemType));
  }

  private Type GetRuntimeTypeParameter(SymbolType type)
  {
    return type.DeclaringType!.GetGenericTypeDefinition().UnderlyingSystemType.GetGenericArguments()[type.GenericParameterPosition];
  }

  private static Type GetReferencedType(SymbolType type)
  {
    return Type.GetType(type.AssemblyQualifiedName, true, false);
  }
}
