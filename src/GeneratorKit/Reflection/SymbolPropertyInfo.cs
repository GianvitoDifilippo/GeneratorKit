using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolPropertyInfo : SymbolPropertyInfoBase
{
  private readonly GeneratorRuntime _runtime;
  private readonly SymbolType? _reflectedType;

  public SymbolPropertyInfo(GeneratorRuntime runtime, IPropertySymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public SymbolPropertyInfo(GeneratorRuntime runtime, IPropertySymbol symbol, SymbolType reflectedType)
    : this(runtime, symbol)
  {
    _reflectedType = reflectedType;
  }

  public IPropertySymbol Symbol { get; }


  // System.Reflection.PropertyInfo overrides

  public override PropertyAttributes Attributes => throw new NotImplementedException();

  public override bool CanRead => throw new NotImplementedException();

  public override bool CanWrite => throw new NotImplementedException();

  public override Type DeclaringType => _runtime.CreateTypeDelegator(Symbol.ContainingType);

  public override MethodInfo? GetMethod => throw new NotImplementedException();

  public override Module Module => throw new NotImplementedException();

  public override string Name => Symbol.Name is "this[]" ? "Item" : Symbol.Name;

  public override Type PropertyType => throw new NotImplementedException();

  public override Type ReflectedType => throw new NotImplementedException();

  public override MethodInfo? SetMethod => throw new NotImplementedException();

  public override MethodInfo[] GetAccessors(bool nonPublic)
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

  public override MethodInfo? GetGetMethod(bool nonPublic)
  {
    throw new NotImplementedException();
  }

  public override ParameterInfo[] GetIndexParameters()
  {
    return Symbol.Parameters.Select(x => _runtime.CreateParameterInfoDelegator(x)).ToArray();
  }

  public override MethodInfo? GetSetMethod(bool nonPublic)
  {
    throw new NotImplementedException();
  }

  public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
  {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}

internal abstract class SymbolPropertyInfoBase : PropertyInfo
{

}