using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolFieldInfo : FieldInfo
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

  public override Type DeclaringType => _runtime.CreateTypeDelegator(Symbol.ContainingType);

  public override RuntimeFieldHandle FieldHandle => throw new NotImplementedException();

  public override Type FieldType => throw new NotImplementedException();

  public override Module Module => throw new NotImplementedException();

  public override string Name => Symbol.Name;

  public override Type ReflectedType => throw new NotImplementedException();

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
}