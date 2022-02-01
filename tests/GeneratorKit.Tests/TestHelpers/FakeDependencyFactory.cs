using GeneratorKit.Interpret;
using System;

namespace GeneratorKit.TestHelpers;

internal class FakeDependencyFactory : IDependencyFactory
{
  private readonly FrameProvider _frameProvider;
  private Interpreter? _interpreter;

  public FakeDependencyFactory(FrameProvider frameProvider)
  {
    _frameProvider = frameProvider;
  }

  public void GetDependencies(GeneratorRuntime runtime, out IActivator activator, out IInterpreter interpreter, out IFrameProvider frameProvider)
  {
    OperationManager operationManager = new OperationManager(runtime);
    _interpreter = new Interpreter(runtime, operationManager, _frameProvider);
    frameProvider = _frameProvider;
    interpreter = _interpreter;
    activator = new Activator(_interpreter, _frameProvider);
  }

  public Interpreter Interpreter => _interpreter ?? throw new InvalidOperationException("GetDependencies was not called");
}
