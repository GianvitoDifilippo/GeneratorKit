using GeneratorKit.Comparers;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GeneratorKit.Reflection;

internal sealed class SymbolPropertyInfo : SymbolPropertyInfoBase
{
  private readonly GeneratorRuntime _runtime;
  private readonly SymbolType? _reflectedType;
  private PropertyInfo? _runtimeProperty;

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

  public PropertyInfo RuntimeProperty => _runtimeProperty ??= MemberResolver.ResolveProperty(ReflectedTypeCore.RuntimeType, this);


  // System.Reflection.PropertyInfo overrides

  public override PropertyAttributes Attributes => PropertyAttributes.None;

  public override bool CanRead => !Symbol.IsWriteOnly;

  public override bool CanWrite => !Symbol.IsReadOnly;

  public override MemberTypes MemberType => MemberTypes.Property;

  public override int MetadataToken => throw new NotSupportedException();

  public override string Name => Symbol.Name is "this[]" ? "Item" : Symbol.Name;

  public override object GetConstantValue()
  {
    throw new NotSupportedException();
  }

  public override object[] GetCustomAttributes(bool inherit)
  {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override IList<CustomAttributeData> GetCustomAttributesData()
  {
    List<CustomAttributeData> result = Symbol
      .GetAttributes()
      .Select(x => (CustomAttributeData)CompilationCustomAttributeData.FromAttributeData(_runtime, x))
      .ToList();
    return new ReadOnlyCollection<CustomAttributeData>(result);
  }

  public override Type[] GetOptionalCustomModifiers()
  {
    throw new NotSupportedException();
  }

  public override Type[] GetRequiredCustomModifiers()
  {
    throw new NotSupportedException();
  }

  public override object GetRawConstantValue()
  {
    throw new NotSupportedException();
  }

  public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
  {
    return RuntimeProperty.GetValue(obj, invokeAttr, binder, index, culture);
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
  {
    SetValue(obj, value, invokeAttr, binder, index, culture);
  }


  // SymbolPropertyInfoBase overrides

  protected override SymbolType DeclaringTypeCore => _runtime.CreateTypeDelegator(Symbol.ContainingType);

  protected override SymbolModule ModuleCore => _runtime.CreateModuleDelegator(Symbol.ContainingModule);

  protected override SymbolType PropertyTypeCore => _runtime.CreateTypeDelegator(Symbol.Type);

  protected override SymbolType ReflectedTypeCore => _reflectedType ?? DeclaringTypeCore;

  protected override SymbolMethodInfo[] GetAccessorsCore(bool nonPublic)
  {
    bool includeGetMethod = CanRead && (Symbol.GetMethod!.DeclaredAccessibility == Accessibility.Public || nonPublic);
    bool includeSetMethod = CanWrite && (Symbol.SetMethod!.DeclaredAccessibility == Accessibility.Public || nonPublic);

    return includeGetMethod
      ? includeSetMethod
        ? new SymbolMethodInfo[] { GetMethod!, SetMethod! }
        : new SymbolMethodInfo[] { GetMethod! }
      : includeSetMethod
        ? new SymbolMethodInfo[] { SetMethod! }
        : Array.Empty<SymbolMethodInfo>();
  }

  protected override SymbolMethodInfo? GetGetMethodCore(bool nonPublic)
  {
    return CanRead && (Symbol.GetMethod!.DeclaredAccessibility == Accessibility.Public || nonPublic)
      ? _reflectedType is not null
        ? new SymbolMethodInfo(_runtime, Symbol.GetMethod, _reflectedType)
        : _runtime.CreateMethodInfoDelegator(Symbol.GetMethod)
      : null;
  }

  protected override SymbolArgumentParameter[] GetIndexParametersCore()
  {
    return Symbol.Parameters.Select(x => new SymbolArgumentParameter(_runtime, x)).ToArray();
  }

  protected override SymbolMethodInfo? GetSetMethodCore(bool nonPublic)
  {
    return CanWrite && (Symbol.SetMethod!.DeclaredAccessibility == Accessibility.Public || nonPublic)
      ? _reflectedType is not null
        ? new SymbolMethodInfo(_runtime, Symbol.SetMethod, _reflectedType)
        : _runtime.CreateMethodInfoDelegator(Symbol.SetMethod)
      : null;
  }


  // System.Object overrides

  public override bool Equals(object? obj)
  {
    if (obj is not PropertyInfo propertyInfo)
      return false;

    return PropertyInfoEqualityComparer.Default.Equals(this, propertyInfo);
  }

  public override int GetHashCode()
  {
    return PropertyInfoEqualityComparer.Default.GetHashCode(this);
  }

  public override string ToString()
  {
    StringBuilder builder = new StringBuilder(PropertyType.Name);
    builder.Append(" ");
    builder.Append(Name);
    ParameterInfo[] parameters = GetIndexParameters();
    if (parameters.Length > 0)
    {
      builder.Append(" [");
      builder.Append(parameters[0].ParameterType.Name);
      for (int i = 1; i < parameters.Length; i++)
      {
        builder.Append(',');
        builder.Append(parameters[i].ParameterType.Name);
      }
      builder.Append("]");
    }
    return builder.ToString();
  }


  // New members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType PropertyType => PropertyTypeCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType DeclaringType => DeclaringTypeCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType ReflectedType => ReflectedTypeCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolMethodInfo? GetMethod => (SymbolMethodInfo?)base.GetMethod;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolModule Module => ModuleCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolMethodInfo? SetMethod => (SymbolMethodInfo?)base.SetMethod;

  public new SymbolMethodInfo[] GetAccessors(bool nonPublic) => GetAccessorsCore(nonPublic);

  public new SymbolMethodInfo? GetGetMethod(bool nonPublic) => GetGetMethodCore(nonPublic);

  public new SymbolArgumentParameter[] GetIndexParameters() => GetIndexParametersCore();

  public new SymbolMethodInfo? GetSetMethod(bool nonPublic) => GetSetMethodCore(nonPublic);
}

internal abstract class SymbolPropertyInfoBase : PropertyInfo
{
  private protected SymbolPropertyInfoBase() { }


  // System.Reflection.PropertyInfo overrides

  public sealed override Type PropertyType => PropertyTypeCore;

  public sealed override Type DeclaringType => DeclaringTypeCore;

  public sealed override Type ReflectedType => ReflectedTypeCore;

  public sealed override Module Module => ModuleCore;

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
  protected abstract SymbolModule ModuleCore { get; }

  protected abstract SymbolMethodInfo[] GetAccessorsCore(bool nonPublic);

  protected abstract SymbolMethodInfo? GetGetMethodCore(bool nonPublic);

  protected abstract SymbolArgumentParameter[] GetIndexParametersCore();

  protected abstract SymbolMethodInfo? GetSetMethodCore(bool nonPublic);
}