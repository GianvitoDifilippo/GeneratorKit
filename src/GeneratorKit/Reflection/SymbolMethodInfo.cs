using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolMethodInfo : SymbolMethodInfoBase
{
  private readonly GeneratorRuntime _runtime;

  public SymbolMethodInfo(GeneratorRuntime runtime, IMethodSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IMethodSymbol Symbol { get; }


  // System.Reflection.MethodInfo overrides

  public override MethodAttributes Attributes => throw new NotImplementedException();

  public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

  public override string Name => Symbol.Name;

  public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();

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

  public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
  {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }


  // SymbolMethodInfoBase overrides

  protected override SymbolType ReturnTypeCore => throw new NotImplementedException();

  protected override SymbolType DeclaringTypeCore => _runtime.CreateTypeDelegator(Symbol.ContainingType);

  protected override SymbolModule ModuleCore => throw new NotImplementedException();

  protected override SymbolType ReflectedTypeCore => throw new NotImplementedException();

  protected override SymbolParameterInfo ReturnParameterCore => throw new NotImplementedException();

  protected override SymbolMethodInfo GetBaseDefinitionCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolType[] GetGenericArgumentsCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolMethodInfo GetGenericMethodDefinitionCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolParameterInfo[] GetParametersCore()
  {
    return Symbol.Parameters.Select(x => _runtime.CreateParameterInfoDelegator(x)).ToArray();
  }
}

internal abstract class SymbolMethodInfoBase : MethodInfo
{
  private protected SymbolMethodInfoBase() { }


  // System.Reflection.MethodInfo overrides

  public sealed override Type DeclaringType => DeclaringTypeCore;

  public sealed override Module Module => ModuleCore;

  public sealed override Type ReturnType => ReturnTypeCore;

  public sealed override Type ReflectedType => ReflectedTypeCore;

  public sealed override ParameterInfo ReturnParameter => ReturnParameterCore;

  public sealed override MethodInfo GetBaseDefinition() => GetBaseDefinitionCore();

  public sealed override Type[] GetGenericArguments() => GetGenericArgumentsCore();

  public sealed override MethodInfo GetGenericMethodDefinition() => GetGenericMethodDefinitionCore();

  public sealed override ParameterInfo[] GetParameters() => GetParametersCore();


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType DeclaringTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ReturnTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ReflectedTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolParameterInfo ReturnParameterCore { get; }

  protected abstract SymbolMethodInfo GetBaseDefinitionCore();

  protected abstract SymbolType[] GetGenericArgumentsCore();

  protected abstract SymbolMethodInfo GetGenericMethodDefinitionCore();

  protected abstract SymbolParameterInfo[] GetParametersCore();
}