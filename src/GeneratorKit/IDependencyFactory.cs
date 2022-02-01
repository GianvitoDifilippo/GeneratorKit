using GeneratorKit.Interpret;

namespace GeneratorKit;

internal interface IDependencyFactory
{
  void GetDependencies(GeneratorRuntime runtime, out IActivator activator, out IInterpreter interpreter);
}
