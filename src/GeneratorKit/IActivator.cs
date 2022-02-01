using GeneratorKit.Reflection;

namespace GeneratorKit;

internal interface IActivator
{
  object CreateInstance(IRuntimeType type, object?[] arguments);
  object CreateInstance(IRuntimeConstructor constructor, object?[] arguments);
}
