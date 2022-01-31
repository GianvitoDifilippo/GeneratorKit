#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Interpret;
using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit;

internal class Activator : IActivator
{
  private const BindingFlags s_allDeclared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
  
  private readonly IInterpreter _interpreter;
  private readonly IFrameProvider _frameProvider;
  private readonly Dictionary<ITypeSymbol, IReadOnlyDictionary<int, SymbolMethodInfo>> _methods;

  public Activator(IInterpreter interpreter, IFrameProvider frameProvider)
  {
    _interpreter = interpreter;
    _frameProvider = frameProvider;
    _methods = new Dictionary<ITypeSymbol, IReadOnlyDictionary<int, SymbolMethodInfo>>(SymbolEqualityComparer.Default);
  }

  public T CreateInstance<T>(IRuntimeType type, params object?[] arguments)
  {
    if (type.ContainsGenericParameters)
      throw new ArgumentException("Cannot create an instance of a type which contains generic parameters.", nameof(type));
    if (type.IsInterface)
      throw new ArgumentException("Cannot create an instance of an interface.", nameof(type));
    if (type.IsAbstract)
      throw new ArgumentException("Cannot create an instance of an abstract class.", nameof(type));

    SymbolType typeDefinition = type.Definition;

    Type proxyType = type.UnderlyingSystemType;
    if (!typeof(T).IsAssignableFrom(proxyType))
      throw new ArgumentException($"{type.FullName} is not assignable to {typeof(T).FullName}", nameof(type));

    InterpreterFrame classFrame = _frameProvider.GetClassFrame(type);
    
    SymbolConstructorInfo constructor = FindConstructor(typeDefinition, arguments, classFrame);
    object?[] proxyArguments = constructor.Symbol.IsImplicitlyDeclared
      ? arguments
      : _interpreter.GetProxyArguments(constructor, classFrame, arguments);
    ConstructorInfo proxyConstructor = FindConstructor(proxyType, proxyArguments);

    IProxied instance = (IProxied)proxyConstructor.Invoke(proxyArguments);
    InterpreterFrame instanceFrame = _frameProvider.GetInstanceFrame(classFrame, instance);
    IReadOnlyDictionary<int, SymbolMethodInfo> methods = GetMethods(typeDefinition);
    instance.Delegate = new OperationDelegate(_interpreter, instanceFrame, methods);

    return (T)instance;
  }

  private IReadOnlyDictionary<int, SymbolMethodInfo> GetMethods(SymbolType typeDefinition)
  {
    if (_methods.TryGetValue(typeDefinition.Symbol, out IReadOnlyDictionary<int, SymbolMethodInfo>? methods))
      return methods;

    Dictionary<int, SymbolMethodInfo> result = new Dictionary<int, SymbolMethodInfo>();
    foreach (SymbolMethodInfo method in typeDefinition.GetMethods(s_allDeclared))
    {
      MethodInfo underlyingSystemMethod = method.UnderlyingSystemMethod;
      OperationAttribute? attribute = underlyingSystemMethod.GetCustomAttribute<OperationAttribute>();
      if (attribute is null)
        continue;

      result.Add(attribute.OperationId, method);
    }

    _methods.Add(typeDefinition.Symbol, result);
    return result;
  }

  private static SymbolConstructorInfo FindConstructor(SymbolType typeDefinition, object?[] arguments, InterpreterFrame classFrame)
  {
    SymbolConstructorInfo[] constructors = typeDefinition.GetConstructors(s_allDeclared);
    SymbolConstructorInfo? match = null;
    int length = arguments.Length;

    foreach (SymbolConstructorInfo constructor in constructors)
    {
      SymbolParameterInfo[] parameters = constructor.GetParameters();
      if (parameters.Length != length)
        continue;

      bool isMatch = true;
      for (int i = 0; i < length; i++)
      {
        SymbolParameterInfo parameter = parameters[i];
        object? argument = arguments[i];
        SymbolType parameterSymbolType = parameter.ParameterType;
        Type parameterType = parameterSymbolType.IsGenericParameter
          ? classFrame.GetGenericArgument((ITypeParameterSymbol)parameterSymbolType.Symbol)
          : parameterSymbolType;

        if (!MatchesArgument(parameterType, argument))
        {
          isMatch = false;
          break;
        }
      }

      if (isMatch)
      {
        match = match is null ? constructor : throw new AmbiguousMatchException();
      }
    }

    return match ?? throw new MissingMethodException($"Constructor on type '{typeDefinition.FullName}' not found.");
  }

  private static ConstructorInfo FindConstructor(Type type, object?[] arguments)
  {
    ConstructorInfo[] constructors = type.GetConstructors(s_allDeclared);
    int length = arguments.Length;

    foreach (ConstructorInfo constructor in constructors)
    {
      ParameterInfo[] parameters = constructor.GetParameters();
      if (parameters.Length != length)
        continue;

      bool isMatch = true;
      for (int i = 0; i < length; i++)
      {
        ParameterInfo parameter = parameters[i];
        object? argument = arguments[i];
        Type parameterType = parameter.ParameterType;

        if (!MatchesArgument(parameterType, argument))
        {
          isMatch = false;
          break;
        }
      }

      if (isMatch)
        return constructor;
    }

    throw Errors.Unreacheable;
  }

  private static bool MatchesArgument(Type parameterType, object? argument)
  {
    return argument is null ? !parameterType.IsValueType : parameterType.IsAssignableFrom(argument.GetType());
  }
}
