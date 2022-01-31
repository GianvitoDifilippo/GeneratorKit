namespace GeneratorKit.Proxy;

internal class ObjectProxy : IProxied
{
  public IOperationDelegate Delegate { get; set; } = default!;
}
