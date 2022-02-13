using GeneratorKit.Interpret;
using GeneratorKit.Interpret.Frame;
using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using GeneratorKit.Reflection.Context;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace GeneratorKit;

internal class GeneratorRuntime : GeneratorContext, IGeneratorRuntime
{
  private readonly IActivator _activator;
  private readonly IProxyManager _proxyManager;
  private readonly IInterpreter _interpreter;
  private readonly CancellationToken _cancellationToken;
  private SymbolAssembly? _compilationAssembly;

  public GeneratorRuntime(Compilation compilation, IDependencyFactory dependencyFactory, CancellationToken cancellationToken)
  {
    Compilation = compilation;
    dependencyFactory.CreateDependencies(this, out _activator, out _proxyManager, out _interpreter);
    _cancellationToken = cancellationToken;
  }

  public override Compilation Compilation { get; }

  public override object CreateInstance(Type type, object?[] arguments)
  {
    if (type.IsArray || type.IsPointer || type.IsGenericParameter || type.IsByRef)
      throw new NotSupportedException("Arrays, pointers, generic parameters and by ref types are not supported.");
    if (type is not SymbolNamedType symbolType)
      throw new ArgumentException("The type must be provided by this runtime.", nameof(type));

    return _activator.CreateInstance(symbolType, arguments);
  }

  public override Type GetContextType(ITypeParameterSymbol symbol)
  {
    return CreateTypeDelegator(symbol);
  }

  public override bool IsGenericTypeDefinition(INamedTypeSymbol symbol)
  {
    if (!symbol.IsGenericType)
      return false;

    ImmutableArray<ITypeParameterSymbol> typeParameters = symbol.TypeParameters;
    ImmutableArray<ITypeSymbol> typeArguments = symbol.TypeArguments;

    for (int i = 0; i < typeParameters.Length; i++)
    {
      ITypeParameterSymbol typeParameter = typeParameters[i];
      if (!typeParameter.Equals(typeArguments[i], SymbolEqualityComparer.Default))
        return false;
    }

    return true;
  }

  public override bool ContainsGenericParameters(INamedTypeSymbol symbol)
  {
    if (!symbol.IsGenericType)
      return false;

    foreach (ITypeSymbol typeArgument in symbol.TypeArguments)
    {
      switch (typeArgument.Kind)
      {
        case SymbolKind.TypeParameter:
          return true;
        case SymbolKind.NamedType:
          if (ContainsGenericParameters((INamedTypeSymbol)typeArgument))
            return true;
          break;
        case SymbolKind.ArrayType:
          if (ContainsGenericParameters((IArrayTypeSymbol)typeArgument))
            return true;
          break;
      }
    }

    return false;
  }

  public override bool IsGenericMethodDefinition(IMethodSymbol symbol)
  {
    if (!symbol.IsGenericMethod)
      return false;

    ImmutableArray<ITypeParameterSymbol> typeParameters = symbol.TypeParameters;
    ImmutableArray<ITypeSymbol> typeArguments = symbol.TypeArguments;

    for (int i = 0; i < typeParameters.Length; i++)
    {
      ITypeParameterSymbol typeParameter = typeParameters[i];
      if (!typeParameter.Equals(typeArguments[i], SymbolEqualityComparer.Default))
        return false;
    }

    return true;
  }

  public override bool ContainsGenericParameters(IMethodSymbol symbol)
  {
    if (symbol.ContainingType is not null && ContainsGenericParameters(symbol.ContainingType))
      return true;
    if (!symbol.IsGenericMethod)
      return false;

    foreach (ITypeSymbol typeArgument in symbol.TypeArguments)
    {
      switch (typeArgument.Kind)
      {
        case SymbolKind.TypeParameter:
          return true;
        case SymbolKind.NamedType:
          if (ContainsGenericParameters((INamedTypeSymbol)typeArgument))
            return true;
          break;
        case SymbolKind.ArrayType:
          if (ContainsGenericParameters((IArrayTypeSymbol)typeArgument))
            return true;
          break;
      }
    }

    return false;
  }

  public override object InvokeConstructor(SymbolConstructorInfo constructor, object?[] arguments)
  {
    if (!constructor.IsSource)
      return constructor.UnderlyingSystemConstructor.Invoke(arguments);

    return _activator.CreateInstance(constructor, arguments);
  }

  public override object? InvokeMethod(SymbolMethodInfo method, object? instance, object?[] arguments)
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

    return _interpreter.InterpretMethod(method.Symbol, frame, method.GetGenericArguments(), arguments);
  }

  public override object? GetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments)
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

    if (frame.IsDefined(property.Symbol))
    {
      return frame.Get(property.Symbol);
    }

    IMethodSymbol getter = property.Symbol.GetMethod ?? throw new InvalidOperationException($"Property {property} does not have a getter.");
    return _interpreter.InterpretMethod(getter, frame, Type.EmptyTypes, arguments);
  }

  public override void SetProperty(SymbolPropertyInfo property, object? instance, object?[] arguments, object? value)
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

    IMethodSymbol setter = property.Symbol.SetMethod ?? throw new InvalidOperationException($"Property {property} does not have a setter.");
    _interpreter.InterpretMethod(setter, frame, Type.EmptyTypes, setterArguments);
  }

  public override object? GetField(SymbolFieldInfo field, object? instance)
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

    return frame.Get(field.Symbol);
  }

  public override void SetField(SymbolFieldInfo field, object? instance, object? value)
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

    frame.Assign(field.Symbol, value);
  }

  public override Type GetRuntimeType(SymbolType type)
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
    bool def = typeDefinition.IsGenericTypeDefinition;
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



  T IGeneratorRuntime.CreateInstance<T>(Type type, object?[] arguments)
  {
    if (!typeof(T).IsAssignableFrom(type))
      throw new ArgumentException($"{type.FullName} is not assignable to {typeof(T).FullName}", nameof(type));

    return (T)CreateInstance(type, arguments);
  }

  Assembly IGeneratorRuntime.CompilationAssembly => _compilationAssembly ??= new SymbolAssembly(this, Compilation.Assembly, Compilation.GetEntryPoint(_cancellationToken));

  Assembly IGeneratorRuntime.CreateAssemblyDelegator(IAssemblySymbol symbol) => CreateAssemblyDelegator(symbol);

  ConstructorInfo IGeneratorRuntime.CreateConstructorInfoDelegator(IMethodSymbol symbol) => CreateConstructorInfoDelegator(symbol);

  FieldInfo IGeneratorRuntime.CreateFieldInfoDelegator(IFieldSymbol symbol) => CreateFieldInfoDelegator(symbol);

  MethodInfo IGeneratorRuntime.CreateMethodInfoDelegator(IMethodSymbol symbol) => CreateMethodInfoDelegator(symbol);

  Module IGeneratorRuntime.CreateModuleDelegator(IModuleSymbol symbol) => CreateModuleDelegator(symbol);

  PropertyInfo IGeneratorRuntime.CreatePropertyInfoDelegator(IPropertySymbol symbol) => CreatePropertyInfoDelegator(symbol);

  Type IGeneratorRuntime.CreateTypeDelegator(ITypeSymbol symbol) => CreateTypeDelegator(symbol);
}
