using GeneratorKit.Reflection;

namespace GeneratorKit.Interpret;

internal interface IInterpreter
{
  object? Interpret(IRuntimeMethod method, InterpreterFrame frame, object?[] arguments);
  void Interpret(IRuntimeConstructor constructor, InterpreterFrame frame, object?[] arguments);
  object?[] GetProxyArguments(IRuntimeConstructor constructor, InterpreterFrame classFrame, object?[] arguments);
  InterpreterFrame GetClassFrame(IRuntimeType type);
  InterpreterFrame GetInstanceFrame(InterpreterFrame classFrame, IRuntimeType type, object instance);
}
