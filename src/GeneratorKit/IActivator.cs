using GeneratorKit.Reflection;

namespace GeneratorKit;

internal interface IActivator
{
  object CreateInstance(SymbolNamedType type, object?[] arguments);
  object CreateInstance(SymbolConstructorInfo constructor, object?[] arguments);
}
