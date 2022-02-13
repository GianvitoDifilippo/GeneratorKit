﻿#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Interpret;
using GeneratorKit.Interpret.Frame;
using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace GeneratorKit;

internal class Activator : IActivator
{
  private const BindingFlags s_allDeclared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
  private static readonly IReadOnlyDictionary<int, IMethodSymbol> s_noMethods = new Dictionary<int, IMethodSymbol>();

  private readonly IInterpreter _interpreter;
  private readonly Dictionary<ITypeSymbol, IReadOnlyDictionary<int, IMethodSymbol>> _methods;

  public Activator(IInterpreter interpreter)
  {
    _interpreter = interpreter;
    _methods = new Dictionary<ITypeSymbol, IReadOnlyDictionary<int, IMethodSymbol>>(SymbolDefinitionEqualityComparer.Default);
  }

  public object CreateInstance(SymbolNamedType type, object?[] arguments)
  {
    InterpreterFrame typeFrame = _interpreter.GetTypeFrame(type);
    SymbolConstructorInfo constructor = FindConstructor(type, arguments);

    return constructor.Symbol.IsImplicitlyDeclared
      ? type.IsAnonymousType
        ? CreateAnonymousTypeInstance(type, typeFrame)
        : CreateWithDefaultConstructor(type, typeFrame)
      : CreateInstance(constructor.Symbol, type, typeFrame, arguments);
  }

  public object CreateInstance(SymbolConstructorInfo constructor, object?[] arguments)
  {
    SymbolNamedType type = constructor.DeclaringType;
    InterpreterFrame typeFrame = _interpreter.GetTypeFrame(type);

    return CreateInstance(constructor.Symbol, type, typeFrame, arguments);
  }

  private object CreateWithDefaultConstructor(SymbolNamedType type, InterpreterFrame typeFrame)
  {
    Type proxyType = type.RuntimeType.UnderlyingSystemType;
    ConstructorInfo proxyConstructor = FindConstructor(proxyType, Array.Empty<object>());

    IProxied instance = (IProxied)proxyConstructor.Invoke(Array.Empty<object>());
    InterpreterFrame instanceFrame = _interpreter.GetInstanceFrame(typeFrame, type, instance);
    IReadOnlyDictionary<int, IMethodSymbol> methods = GetMethods(type.Symbol);
    instance.Delegate = new OperationDelegate(_interpreter, type, instanceFrame, methods);

    return instance;
  }

  private object CreateAnonymousTypeInstance(SymbolNamedType type, InterpreterFrame typeFrame)
  {
    ObjectProxy instance = new ObjectProxy();
    InterpreterFrame instanceFrame = _interpreter.GetInstanceFrame(typeFrame, type, instance);
    instance.Delegate = new OperationDelegate(_interpreter, type, instanceFrame, s_noMethods);

    foreach (IPropertySymbol property in type.Symbol.GetMembers().Where(member => member.Kind is SymbolKind.Property))
    {
      instanceFrame.Declare(property);
    }

    return instance;
  }

  private object CreateInstance(IMethodSymbol constructor, SymbolNamedType type, InterpreterFrame typeFrame, object?[] arguments)
  {
    Type proxyType = type.RuntimeType.UnderlyingSystemType;
    object?[] proxyArguments = _interpreter.GetProxyArguments(constructor, typeFrame, arguments);
    ConstructorInfo proxyConstructor = FindConstructor(proxyType, proxyArguments);

    IProxied instance = (IProxied)proxyConstructor.Invoke(proxyArguments);
    InterpreterFrame instanceFrame = _interpreter.GetInstanceFrame(typeFrame, type, instance);
    IReadOnlyDictionary<int, IMethodSymbol> methods = GetMethods(type.Symbol);
    instance.Delegate = new OperationDelegate(_interpreter, type, instanceFrame, methods);
    _interpreter.InterpretConstructor(constructor, instanceFrame, arguments);

    return instance;
  }

  private IReadOnlyDictionary<int, IMethodSymbol> GetMethods(ITypeSymbol symbol)
  {
    if (_methods.TryGetValue(symbol, out IReadOnlyDictionary<int, IMethodSymbol>? methods))
      return methods;

    Dictionary<int, IMethodSymbol> result = new Dictionary<int, IMethodSymbol>();
    foreach (IMethodSymbol method in symbol.GetMembers().Where(member => member.Kind is SymbolKind.Method))
    {
      if (method.IsSource())
        continue;

      bool found = false;
      int operationId = -1;
      foreach (AttributeData attribute in method.GetAttributes()) // TODO: Use reflection when supported
      {
        if (attribute.AttributeClass is not { } attributeClass || attributeClass.ToDisplayString() != "GeneratorKit.Proxy.OperationAttribute")
          continue;
        if (attribute.ConstructorArguments.Length == 0)
          continue;
        TypedConstant constant = attribute.ConstructorArguments[0];

        if (constant.Value is int value)
        {
          found = true;
          operationId = value;
        }
      }

      if (found)
        continue;

      result.Add(operationId, method);
    }

    _methods.Add(symbol, result);
    return result;
  }

  private static SymbolConstructorInfo FindConstructor(SymbolNamedType type, object?[] arguments)
  {
    SymbolConstructorInfo[] constructors = type.GetConstructors(s_allDeclared);
    if (type.IsAnonymousType)
    {
      Debug.Assert(constructors.Length == 1, $"Expected anonymous type to have a single constructor, but found {constructors.Length} of them.");
      return constructors[0];
    }

    SymbolConstructorInfo? match = null;
    int length = arguments.Length;

    foreach (SymbolConstructorInfo constructor in constructors)
    {
      Type[] parameterTypes = constructor.ParameterTypes;
      if (parameterTypes.Length != length)
        continue;

      bool isMatch = true;
      for (int i = 0; i < length; i++)
      {
        if (!MatchesArgument(parameterTypes[i], arguments[i]))
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

    return match ?? throw new MissingMethodException($"Constructor on type '{type.FullName}' not found.");
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
