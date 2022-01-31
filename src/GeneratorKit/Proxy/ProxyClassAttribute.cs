using System;

namespace GeneratorKit.Proxy;

[AttributeUsage(AttributeTargets.Class)]
public class ProxyClassAttribute : Attribute
{
  public ProxyClassAttribute(Type type)
  {
    Type = type;
  }

  public Type Type { get; }
}
