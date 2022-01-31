using GeneratorKit.Reflection;

namespace GeneratorKit.Interpret;

internal interface IFrameProvider
{
  InterpreterFrame GetClassFrame(IRuntimeType type);
  InterpreterFrame GetInstanceFrame(InterpreterFrame classFrame, object instance);
  InterpreterFrame GetMethodFrame(InterpreterFrame parent, IRuntimeMethod method, object?[] arguments);
  InterpreterFrame GetConstructorFrame(InterpreterFrame classFrame, SymbolConstructorInfo constructor, object?[] arguments);
  InterpreterFrame NewScopeFrame(InterpreterFrame parent);
}