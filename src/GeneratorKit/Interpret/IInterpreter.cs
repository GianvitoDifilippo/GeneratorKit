using GeneratorKit.Interpret.Frame;
using GeneratorKit.Reflection;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Interpret;

internal interface IInterpreter
{
  object? InterpretMethod(IMethodSymbol method, InterpreterFrame frame, Type[] typeArguments, object?[] arguments);
  void InterpretConstructor(IMethodSymbol constructor, InterpreterFrame frame, object?[] arguments);
  object?[] GetProxyArguments(IMethodSymbol constructor, InterpreterFrame typeFrame, object?[] arguments);
  InterpreterFrame GetTypeFrame(SymbolType type);
  InterpreterFrame GetInstanceFrame(InterpreterFrame typeFrame, SymbolType type, object instance);
}
