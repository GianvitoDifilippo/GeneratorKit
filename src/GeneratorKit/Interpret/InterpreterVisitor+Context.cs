using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GeneratorKit.Interpret;

internal partial class InterpreterVisitor
{
  private readonly GeneratorRuntime _runtime;
  private readonly Interpreter _interpreter;
  private readonly IFrameProvider _frameProvider;
  private readonly Stack<InterpreterFrame> _frames;
  private readonly Stack<object> _implicitReceivers;
  private readonly Stack<object> _conditionalAccessInstances;
  private readonly Stack<ArrayInitializerContext> _arrayContexes;
  private Optional<object?> _returnValue;
  private BranchKind _branchState;

  public InterpreterVisitor(GeneratorRuntime runtime, Interpreter interpreter, IFrameProvider frameProvider, InterpreterFrame frame)
  {
    _runtime = runtime;
    _interpreter = interpreter;
    _frameProvider = frameProvider;
    _frames = new Stack<InterpreterFrame>(1);
    _frames.Push(frame);
    _implicitReceivers = new Stack<object>();
    _conditionalAccessInstances = new Stack<object>();
    _arrayContexes = new Stack<ArrayInitializerContext>();
    _branchState = BranchKind.None;
  }

  private InterpreterFrame Frame => _frames.Peek();

  private object ImplicitReceiver => _implicitReceivers.Peek();

  private object ConditionalAccessInstance => _conditionalAccessInstances.Peek();

  private ArrayInitializerContext ArrayContext => _arrayContexes.Peek();

  private void BeginScope()
  {
    _frames.Push(_frameProvider.NewScopeFrame(Frame));
  }

  private void EndScope()
  {
    _frames.Pop();
  }

  private void BeginReceiver(object receiver)
  {
    _implicitReceivers.Push(receiver);
  }

  private void EndReceiver()
  {
    _implicitReceivers.Pop();
  }

  private void BeginConditionalAccess(object instance)
  {
    _conditionalAccessInstances.Push(instance);
  }

  private void EndConditionalAccess()
  {
    _conditionalAccessInstances.Pop();
  }

  private void BeginArrayInitializerContext(Array array, int[] indices, int dimension)
  {
    _arrayContexes.Push(new ArrayInitializerContext(array, indices, dimension));
  }

  private void EndArrayInitializerContext()
  {
    _arrayContexes.Pop();
  }

  private bool ResetBranchState()
  {
    bool result = _branchState is BranchKind.Break;
    _branchState = BranchKind.None;
    return result;
  }

  private object? InvokeMethod(IRuntimeMethod method, object? instance, object?[] arguments)
  {
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

  // public MethodInfo ResolveRuntimeMethod(SymbolMethodInfo method)
  // {
  //   MethodInfo result;
  //   if (method.DeclaringType.ContainsGenericParameters)
  //   {
  //     Type definitionType = method.DeclaringType.GetGenericTypeDefinition().UnderlyingSystemType;
  //     SymbolType[] genericArguments = method.DeclaringType.GetGenericArguments();
  //     Type[] typeArguments = genericArguments.Map(t =>
  //     {
  //       Type result = t.IsGenericParameter ? Frame.GetGenericTypeArgument(t.Symbol) : t;
  //       return t.UnderlyingSystemType;
  //     });
  //     Type type = definitionType.MakeGenericType(typeArguments);
  //     result = DelegatorBinder.ResolveMethod(type, method);
  //   }
  //   else
  //   {
  //     result = method.UnderlyingSystemMethod;
  //   }
  // 
  //   if (method.ContainsGenericParameters)
  //   {
  //     Type[] methodGenericArguments = method.GetGenericArguments().Map(t =>
  //     {
  //       Type result = t.IsGenericParameter ? Frame.GetGenericTypeArgument(t.Symbol) : t;
  //       return t.UnderlyingSystemType;
  //     });
  //     result = result.MakeGenericMethod(methodGenericArguments);
  //   }
  // 
  //   return result;
  // }
}