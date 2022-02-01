using GeneratorKit.Interpret;
using System;

namespace GeneratorKit.TestHelpers;

internal class FakeDependencyFactory : IDependencyFactory
{
  private readonly FakeFrameProvider _frameProvider;
  private Interpreter? _interpreter;

  public FakeDependencyFactory(FakeFrameProvider frameProvider)
  {
    _frameProvider = frameProvider;
  }

  public void GetDependencies(GeneratorRuntime runtime, out IActivator activator, out IInterpreter interpreter)
  {
    OperationManager operationManager = new OperationManager(runtime);
    _interpreter = new Interpreter(runtime, operationManager, _frameProvider);
    interpreter = _interpreter;
    activator = new Activator(_interpreter);
  }

  public Interpreter Interpreter => _interpreter ?? throw new InvalidOperationException("GetDependencies was not called");
}
