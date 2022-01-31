namespace GeneratorKit.Proxy;

internal interface IProxied
{
  IOperationDelegate Delegate { get; set; }
}