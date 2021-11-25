using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolFieldInfo : SymbolFieldInfoBase
{
  private readonly GeneratorRuntime _runtime;

  public SymbolFieldInfo(GeneratorRuntime runtime, IFieldSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IFieldSymbol Symbol { get; }


  // System.Reflection.FieldInfo overrides

  public override FieldAttributes Attributes => throw new NotImplementedException();

  public override RuntimeFieldHandle FieldHandle => throw new NotImplementedException();

  public override string Name => Symbol.Name;

  public override object[] GetCustomAttributes(bool inherit)
  {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override object GetValue(object obj)
  {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
  {
    throw new NotImplementedException();
  }


  // SymbolFieldInfoBase overrides

  protected override SymbolType DeclaringTypeCore => _runtime.CreateTypeDelegator(Symbol.ContainingType);

  protected override SymbolType FieldTypeCore => throw new NotImplementedException();

  protected override SymbolModule ModuleCore => throw new NotImplementedException();

  protected override SymbolType ReflectedTypeCore => throw new NotImplementedException();
}

internal abstract class SymbolFieldInfoBase : FieldInfo
{
  private protected SymbolFieldInfoBase() { }


  // System.Reflection.FieldInfo overrides

  public sealed override Type DeclaringType => DeclaringTypeCore;

  public sealed override Type FieldType => FieldTypeCore;

  public sealed override Module Module => ModuleCore;

  public sealed override Type ReflectedType => ReflectedTypeCore;


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType DeclaringTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType FieldTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ReflectedTypeCore { get; }
}