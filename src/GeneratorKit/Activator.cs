#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Interpret;
using GeneratorKit.Interpret.Frame;
using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GeneratorKit;

internal class Activator : IActivator
{
  private const BindingFlags s_allDeclared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
  
  private readonly IInterpreter _interpreter;
  private readonly Dictionary<INamedTypeSymbol, IReadOnlyDictionary<int, IMethodSymbol>> _methods;

  public Activator(IInterpreter interpreter)
  {
    _interpreter = interpreter;
    _methods = new Dictionary<INamedTypeSymbol, IReadOnlyDictionary<int, IMethodSymbol>>(SymbolDefinitionEqualityComparer.Default);
  }

  public object CreateInstance(SymbolType type, object?[] arguments)
  {
    InterpreterFrame typeFrame = _interpreter.GetTypeFrame(type);
    SymbolConstructorInfo constructor = FindConstructor(type, arguments);

    return CreateInstance(constructor.OriginalSymbol, type, typeFrame, arguments);
  }

  public object CreateInstance(SymbolConstructorInfo constructor, object?[] arguments)
  {
    SymbolType type = constructor.DeclaringType;
    InterpreterFrame typeFrame = _interpreter.GetTypeFrame(type);

    return CreateInstance(constructor.OriginalSymbol, type, typeFrame, arguments);
  }

  private object CreateInstance(IMethodSymbol constructor, SymbolType type, InterpreterFrame typeFrame, object?[] arguments)
  {
    Type proxyType = type.RuntimeType.UnderlyingSystemType;
    object?[] proxyArguments = constructor.IsImplicitlyDeclared
      ? arguments
      : _interpreter.GetProxyArguments(constructor, typeFrame, arguments);
    ConstructorInfo proxyConstructor = FindConstructor(proxyType, proxyArguments);

    IProxied instance = (IProxied)proxyConstructor.Invoke(proxyArguments);
    InterpreterFrame instanceFrame = _interpreter.GetInstanceFrame(typeFrame, type, instance);
    IReadOnlyDictionary<int, IMethodSymbol> methods = GetMethods(type.OriginalSymbol);
    instance.Delegate = new OperationDelegate(_interpreter, instanceFrame, methods);
    if (!constructor.IsImplicitlyDeclared)
    {
      _interpreter.InterpretConstructor(constructor, instanceFrame, arguments);
    }

    return instance;
  }

  private IReadOnlyDictionary<int, IMethodSymbol> GetMethods(INamedTypeSymbol symbol)
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

  private static SymbolConstructorInfo FindConstructor(SymbolType type, object?[] arguments)
  {
    SymbolConstructorInfo[] constructors = type.GetConstructors(s_allDeclared);
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
