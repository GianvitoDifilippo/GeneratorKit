using GeneratorKit.Reflection;

namespace GeneratorKit;

internal interface IActivator
{
  T CreateInstance<T>(IRuntimeType type, params object?[] arguments);
}
