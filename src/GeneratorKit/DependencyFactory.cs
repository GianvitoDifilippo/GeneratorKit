using GeneratorKit.Interpret;

namespace GeneratorKit;

internal class DependencyFactory : IDependencyFactory
{
  public void GetDependencies(GeneratorRuntime runtime, out IActivator activator, out IInterpreter interpreter, out IFrameProvider frameProvider)
  {
    OperationManager operationManager = new OperationManager(runtime);
    FrameDictionaryProvider dictionaryProvider = new FrameDictionaryProvider();
    frameProvider = new FrameProvider(dictionaryProvider);
    interpreter = new Interpreter(runtime, operationManager, frameProvider);
    activator = new Activator(interpreter, frameProvider);
  }
}
