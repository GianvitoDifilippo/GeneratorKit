using GeneratorKit.Interpret;

namespace GeneratorKit;

internal class DependencyFactory : IDependencyFactory
{
  public void GetDependencies(GeneratorRuntime runtime, out IActivator activator, out IInterpreter interpreter, out IFrameProvider frameProvider)
  {
    OperationManager operationManager = new OperationManager(runtime);
    frameProvider = new FrameProvider();
    interpreter = new Interpreter(runtime, operationManager, frameProvider);
    activator = new Activator(interpreter, frameProvider);
  }
}
