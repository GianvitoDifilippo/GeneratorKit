using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolEventInfo : EventInfo
{
  private readonly GeneratorRuntime _runtime;

  public SymbolEventInfo(GeneratorRuntime runtime, IEventSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IEventSymbol Symbol { get; }

  public override EventAttributes Attributes => throw new NotImplementedException();

  public override string Name => Symbol.Name;

  public override Type DeclaringType => _runtime.CreateTypeDelegator(Symbol.ContainingType);

  public override Type EventHandlerType => throw new NotImplementedException();

  public override Module Module => throw new NotImplementedException();

  public override Type ReflectedType => throw new NotImplementedException();

  public override MethodInfo? GetAddMethod(bool nonPublic)
  {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(bool inherit)
  {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override MethodInfo? GetRaiseMethod(bool nonPublic)
  {
    throw new NotImplementedException();
  }

  public override MethodInfo? GetRemoveMethod(bool nonPublic)
  {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }
}