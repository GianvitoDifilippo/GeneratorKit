using GeneratorKit.Reflection;

namespace GeneratorKit.Interpret;

internal interface IFrameProvider
{
  InterpreterFrame GetClassFrame(IRuntimeType type);
  InterpreterFrame GetInstanceFrame(InterpreterFrame classFrame, IRuntimeType type, object instance);
  InterpreterFrame GetMethodFrame(InterpreterFrame parent, IRuntimeMethod method, object?[] arguments);
  InterpreterFrame GetConstructorFrame(InterpreterFrame classFrame, IRuntimeConstructor constructor, object?[] arguments);
  InterpreterFrame NewScopeFrame(InterpreterFrame parent);
}