using GeneratorKit.Reflection;

namespace GeneratorKit.Interpret;

internal interface IInterpreter
{
  object? Interpret(IRuntimeMethod method, InterpreterFrame frame, object?[] arguments);
  object?[] GetProxyArguments(SymbolConstructorInfo constructor, InterpreterFrame classFrame, object?[] arguments);
}
