using GeneratorKit.Proxy;
using System;

namespace GeneratorKit.TestHelpers;

public static class ProxyTypes
{
  public class NonGenericClass { }

  [ProxyClass(typeof(NonGenericClass))]
  public class NonGenericClassProxy : NonGenericClass, IProxied
  {
    IOperationDelegate IProxied.Delegate { get; set; } = default!;
  }

  public class GenericClass<T> { }

  [ProxyClass(typeof(GenericClass<>))]
  public class GenericClassProxy<T> : GenericClass<T>, IProxied
  {
    IOperationDelegate IProxied.Delegate { get; set; } = default!;
  }

  public class GenericClass<T1, T2> { }

  [ProxyClass(typeof(GenericClass<,>))]
  public class GenericClassProxy<T1, T2> : GenericClass<T1, T2>, IProxied
  {
    IOperationDelegate IProxied.Delegate { get; set; } = default!;
  }

  public interface IInterface { }

  [ProxyClass(typeof(IInterface))]
  public class InterfaceProxy : IInterface, IProxied
  {
    IOperationDelegate IProxied.Delegate { get; set; } = default!;
  }

  public class NonGenericClassWithMembers
  {
    public NonGenericClassWithMembers(int value1)
    {
      Value1 = value1;
    }

    public NonGenericClassWithMembers(string value2)
    {
      Value2 = value2;
    }

    public int Value1 { get; }
    public string? Value2 { get; }

    public int Prop1 { get; set; } = default!;
    public string Prop2 { get; set; } = default!;
    public int this[int arg1] { get => throw null!; set => throw null!; }
    public int this[int arg1, string arg2] { get => throw null!; set => throw null!; }

    public void Method1() => throw null!;
    public void Method1(int arg1, string arg2) => throw null!;
    public int Method2() => throw null!;
    public int Method2(int arg1, string arg2) => throw null!;
  }

  public class NonGenericClassWithMembersProxy : NonGenericClassWithMembers, IProxied
  {
    public NonGenericClassWithMembersProxy(int value1) : base(value1) { }

    public NonGenericClassWithMembersProxy(string value2) : base(value2) { }

    IOperationDelegate IProxied.Delegate { get; set; } = default!;
  }

  public abstract class GenericClassWithMembers<T>
    where T : class
  {
    public GenericClassWithMembers(int value1)
    {
      Value1 = value1;
    }

    public GenericClassWithMembers(T value2)
    {
      Value2 = value2;
    }

    public int Value1 { get; }
    public T? Value2 { get; }
  }

  public class GenericClassWithMembersProxy<T> : GenericClassWithMembers<T>, IProxied
    where T : class
  {
    public GenericClassWithMembersProxy(int value1) : base(value1) { }

    public GenericClassWithMembersProxy(T value2) : base(value2) { }

    public IOperationDelegate Delegate { get; set; } = default!;
  }
}