using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System;
using System.Reflection;
using System.Globalization;

namespace GeneratorKit.Reflection;

public sealed class SymbolConstructorInfo : SymbolConstructorInfoBase
{
  private readonly IGeneratorRuntime _runtime;

  internal SymbolConstructorInfo(IGeneratorRuntime runtime, IMethodSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IMethodSymbol Symbol { get; }


  // System.Reflection.ConstructorInfo overrides

  public override MethodAttributes Attributes => throw new NotImplementedException();

  public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

  public override string Name => throw new NotImplementedException();

  public override object[] GetCustomAttributes(bool inherit)
  {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override MethodImplAttributes GetMethodImplementationFlags()
  {
    throw new NotImplementedException();
  }

  public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
  {
    throw new NotImplementedException();
  }

  public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
  {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }


  // SymbolConstructorInfoBase overrides

  protected override SymbolType DeclaringTypeCore => throw new NotImplementedException();

  protected override SymbolModule ModuleCore => throw new NotImplementedException();

  protected override SymbolType ReflectedTypeCore => throw new NotImplementedException();

  protected override SymbolType[] GetGenericArgumentsCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolParameterInfo[] GetParametersCore()
  {
    throw new NotImplementedException();
  }
}

#region Base

public abstract class SymbolConstructorInfoBase : ConstructorInfo
{
  private protected SymbolConstructorInfoBase() { }


  // System.Reflection.ConstructorInfo overrides

  public sealed override Type DeclaringType => DeclaringTypeCore;

  public sealed override Type[] GetGenericArguments() => GetGenericArgumentsCore();

  public sealed override ParameterInfo[] GetParameters() => GetParametersCore();

  public sealed override Module Module => ModuleCore;

  public sealed override Type ReflectedType => ReflectedTypeCore;


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType DeclaringTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ReflectedTypeCore { get; }

  protected abstract SymbolType[] GetGenericArgumentsCore();

  protected abstract SymbolParameterInfo[] GetParametersCore();
}

#endregion
