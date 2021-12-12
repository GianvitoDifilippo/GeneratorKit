namespace GeneratorKit.Interpret;

internal partial class Interpreter
{
  public static readonly Interpreter s_instance = new Interpreter();

  public static T Interpret<T>(int operationId, object? target, object?[] arguments)
  {
    return s_instance.InterpretImpl<T>(operationId, target, arguments);
  }
}