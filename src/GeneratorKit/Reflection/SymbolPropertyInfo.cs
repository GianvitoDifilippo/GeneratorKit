using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System;
using System.Reflection;
using System.Globalization;

namespace GeneratorKit.Reflection;

public sealed class SymbolPropertyInfo : SymbolPropertyInfoBase
{
  private readonly IGeneratorRuntime _runtime;
  private readonly SymbolType? _reflectedType;

  internal SymbolPropertyInfo(IGeneratorRuntime runtime, IPropertySymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  internal SymbolPropertyInfo(IGeneratorRuntime runtime, IPropertySymbol symbol, SymbolType reflectedType)
    : this(runtime, symbol)
  {
    _reflectedType = reflectedType;
  }

  public IPropertySymbol Symbol { get; }


  // System.Reflection.PropertyInfo overrides

  public override PropertyAttributes Attributes => throw new NotImplementedException();

  public override bool CanRead => throw new NotImplementedException();

  public override bool CanWrite => throw new NotImplementedException();

  public override string Name => throw new NotImplementedException();

  public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
  {
    throw new NotImplementedException();
  }

  public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
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

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }


  // SymbolPropertyInfoBase overrides

  protected override SymbolType PropertyTypeCore => throw new NotImplementedException();

  protected override SymbolType DeclaringTypeCore => throw new NotImplementedException();

  protected override SymbolType ReflectedTypeCore => throw new NotImplementedException();

  protected override SymbolMethodInfo? GetMethodCore => throw new NotImplementedException();

  protected override SymbolModule ModuleCore => throw new NotImplementedException();

  protected override SymbolMethodInfo? SetMethodCore => throw new NotImplementedException();

  protected override SymbolMethodInfo[] GetAccessorsCore(bool nonPublic)
  {
    throw new NotImplementedException();
  }

  protected override SymbolMethodInfo? GetGetMethodCore(bool nonPublic)
  {
    throw new NotImplementedException();
  }

  protected override SymbolParameterInfo[] GetIndexParametersCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolMethodInfo? GetSetMethodCore(bool nonPublic)
  {
    throw new NotImplementedException();
  }
}

#region Base

public abstract class SymbolPropertyInfoBase : PropertyInfo
{
  private protected SymbolPropertyInfoBase() { }


  // System.Reflection.PropertyInfo overrides

  public sealed override Type PropertyType => PropertyTypeCore;

  public sealed override Type DeclaringType => DeclaringTypeCore;

  public sealed override Type ReflectedType => ReflectedTypeCore;

  public sealed override MethodInfo? GetMethod => GetMethodCore;

  public sealed override Module Module => ModuleCore;

  public sealed override MethodInfo? SetMethod => SetMethodCore;

  public sealed override MethodInfo[] GetAccessors(bool nonPublic) => GetAccessorsCore(nonPublic);

  public sealed override MethodInfo? GetGetMethod(bool nonPublic) => GetGetMethodCore(nonPublic);

  public sealed override ParameterInfo[] GetIndexParameters() => GetIndexParametersCore();

  public sealed override MethodInfo? GetSetMethod(bool nonPublic) => GetSetMethodCore(nonPublic);


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType PropertyTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType DeclaringTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ReflectedTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolMethodInfo? GetMethodCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolMethodInfo? SetMethodCore { get; }

  protected abstract SymbolMethodInfo[] GetAccessorsCore(bool nonPublic);

  protected abstract SymbolMethodInfo? GetGetMethodCore(bool nonPublic);

  protected abstract SymbolParameterInfo[] GetIndexParametersCore();

  protected abstract SymbolMethodInfo? GetSetMethodCore(bool nonPublic);
}

#endregion