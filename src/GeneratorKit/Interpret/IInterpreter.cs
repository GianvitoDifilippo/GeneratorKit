using GeneratorKit.Reflection;

namespace GeneratorKit.Interpret;

internal interface IInterpreter
{
  object? Interpret(IRuntimeMethod method, InterpreterFrame frame, object?[] arguments);
  void Interpret(IRuntimeConstructor constructor, InterpreterFrame frame, object?[] arguments);
  object?[] GetProxyArguments(IRuntimeConstructor constructor, InterpreterFrame classFrame, object?[] arguments);
}
