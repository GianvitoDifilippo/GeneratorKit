using GeneratorKit.Interpret.Context;
using GeneratorKit.Interpret.Frame;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

namespace GeneratorKit.Interpret;

internal class InterpretedMethodInvoker
{
  private const int s_maxParameters = 1;
  private static readonly DelegateEntry?[] s_actionEntries = new DelegateEntry?[s_maxParameters + 1];
  private static readonly DelegateEntry?[] s_funcEntries = new DelegateEntry?[s_maxParameters + 1];

  private readonly IInterpreterContext _context;
  private readonly InterpreterFrame _closure;
  private readonly IFrameProvider _frameProvider;
  private readonly IAnonymousFunctionOperation _operation;

  private InterpretedMethodInvoker(IInterpreterContext context, InterpreterFrame closure, IFrameProvider frameProvider, IAnonymousFunctionOperation operation)
  {
    _context = context;
    _closure = closure;
    _frameProvider = frameProvider;
    _operation = operation;
  }

  public static Delegate CreateDelegate(IInterpreterContext context, InterpreterFrame closure, IFrameProvider frameProvider, IAnonymousFunctionOperation operation)
  {
    return new InterpretedMethodInvoker(context, closure, frameProvider, operation).CreateDelegate();
  }

  private object? Invoke(object?[] arguments)
  {
    IMethodSymbol method = _operation.Symbol;
    ImmutableArray<IParameterSymbol> parameters = method.Parameters;
    Debug.Assert(parameters.Length == arguments.Length, $"Expected {parameters.Length} arguments but got {arguments.Length} instead.");
    
    InterpreterFrame frame = InterpreterFrame.NewScopeFrame(_closure, _frameProvider.GetFrame());
    for (int i = 0; i < parameters.Length; i++)
    {
      frame.Define(parameters[i], arguments[i]);
    }
    
    return new InterpreterVisitor(_context, _frameProvider, frame).VisitBlock(_operation.Body, default);
  }

  private void InvokeAction0()
  {
    Invoke(Array.Empty<object?>());
  }

  private void InvokeAction1<T1>(T1 arg)
  {
    Invoke(new object?[] { arg });
  }

  private TResult InvokeFunc0<TResult>()
  {
    return (TResult)Invoke(Array.Empty<object?>())!;
  }

  private TResult InvokeFunc1<T1, TResult>(T1 arg)
  {
    return (TResult)Invoke(new object?[] { arg })!;
  }

  private Delegate CreateDelegate()
  {
    IMethodSymbol symbol = _operation.Symbol;

    return symbol.ReturnsVoid
      ? CreateAction(symbol)
      : CreateFunc(symbol);
  }

  private Delegate CreateAction(IMethodSymbol symbol)
  {
    int length = symbol.Parameters.Length;
    Type[] parameterTypes = symbol.Parameters.Map(p => _context.GetType(p.Type).UnderlyingSystemType);
    if (length > s_maxParameters)
      throw new NotSupportedException($"Delegates with more than {s_maxParameters} parameters are not supported.");

    DelegateEntry? entry = s_actionEntries[length];
    Type delegateType;
    MethodInfo method;
    if (entry is null)
    {
      delegateType = DelegateHelper.GetActionType(parameterTypes);
      // TODO: Extract and check for null
      method = typeof(InterpretedMethodInvoker).GetMethod($"InvokeAction{length}", BindingFlags.NonPublic | BindingFlags.Instance);
      s_actionEntries[length] = new DelegateEntry(delegateType, method);
    }
    else
    {
      (delegateType, method) = entry;
    }

    method = method.MakeGenericMethod(parameterTypes);
    return Delegate.CreateDelegate(delegateType, this, method);
  }

  private Delegate CreateFunc(IMethodSymbol symbol)
  {
    int length = symbol.Parameters.Length;
    Type[] parameterTypes = new Type[length + 1];
    for (int i = 0; i < symbol.Parameters.Length; i++)
    {
      parameterTypes[i] = _context.GetType(symbol.Parameters[i].Type).UnderlyingSystemType;
    }
    parameterTypes[symbol.Parameters.Length] = _context.GetType(symbol.ReturnType).UnderlyingSystemType;

    if (length > s_maxParameters)
      throw new NotSupportedException($"Delegates with more than {s_maxParameters} parameters are not supported.");

    DelegateEntry? entry = s_funcEntries[length];
    Type delegateType;
    MethodInfo method;
    if (entry is null)
    {
      delegateType = DelegateHelper.GetFuncType(parameterTypes);
      method = typeof(InterpretedMethodInvoker).GetMethod($"InvokeFunc{length}", BindingFlags.NonPublic | BindingFlags.Instance);
      s_funcEntries[length] = new DelegateEntry(delegateType, method);
    }
    else
    {
      (delegateType, method) = entry;
    }

    method = method.MakeGenericMethod(parameterTypes);
    return Delegate.CreateDelegate(delegateType, this, method);
  }

  private record DelegateEntry(Type DelegateType, MethodInfo Method);
}
