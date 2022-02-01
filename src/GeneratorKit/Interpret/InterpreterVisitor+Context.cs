﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;

namespace GeneratorKit.Interpret;

internal partial class InterpreterVisitor
{
  private readonly GeneratorRuntime _runtime;
  private readonly IFrameProvider _frameProvider;
  private readonly Stack<InterpreterFrame> _frames;
  private readonly Stack<object> _implicitReceivers;
  private readonly Stack<object> _conditionalAccessInstances;
  private readonly Stack<ArrayInitializerContext> _arrayContexes;
  private Optional<object?> _returnValue;
  private BranchKind _branchState;

  public InterpreterVisitor(GeneratorRuntime runtime, IFrameProvider frameProvider, InterpreterFrame frame)
  {
    _runtime = runtime;
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
}