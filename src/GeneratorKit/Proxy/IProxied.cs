namespace GeneratorKit.Proxy;

public interface IProxied
{
  IOperationDelegate Delegate { get; set; }
}