using GeneratorKit.Reflection;

namespace GeneratorKit;

internal interface IActivator
{
  object CreateInstance(SymbolType type, object?[] arguments);
  object CreateInstance(SymbolConstructorInfo constructor, object?[] arguments);
}
