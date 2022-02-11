using GeneratorKit.Comparers;
using GeneratorKit.Reflection.Binders;
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
  private readonly IReflectionRuntime _runtime;
  private readonly IGeneratorContext _context;
  private readonly SymbolType? _reflectedType;
  private PropertyInfo? _underlyingSystemProperty;

  public SymbolPropertyInfo(IReflectionRuntime runtime, IGeneratorContext context, IPropertySymbol symbol, SymbolType? reflectedType)
  {
    _runtime = runtime;
    _context = context;
    OriginalSymbol = symbol;
    _reflectedType = reflectedType;
  }

  public IPropertySymbol OriginalSymbol { get; }

  public bool IsSource => OriginalSymbol.IsSource();

  public bool IsStatic => OriginalSymbol.IsStatic;

  public bool IsIndexer => OriginalSymbol.IsIndexer;

  public Type[] ParameterTypes => OriginalSymbol.Parameters.Map(parameter => _context.GetContextType(parameter.Type));

  public PropertyInfo UnderlyingSystemProperty
  {
    get
    {
      Debug.Assert(!OriginalSymbol.IsSource());
      return _underlyingSystemProperty ??= DelegatorBinder.ResolveProperty(ReflectedType.UnderlyingSystemType, this);
    }
  }


  // System.Reflection.PropertyInfo overrides

  public override PropertyAttributes Attributes => PropertyAttributes.None;

  public override bool CanRead => !OriginalSymbol.IsWriteOnly;

  public override bool CanWrite => !OriginalSymbol.IsReadOnly;

  public override MemberTypes MemberType => MemberTypes.Property;

  public override int MetadataToken => throw new NotSupportedException();

  public override string Name => OriginalSymbol.Name is "this[]" ? "Item" : OriginalSymbol.Name;

  public override Type PropertyType => _context.GetContextType(OriginalSymbol.Type);

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
    List<CustomAttributeData> result = OriginalSymbol
      .GetAttributes()
      .Select(data => CompilationCustomAttributeData.FromAttributeData(_context, data) as CustomAttributeData)
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

  public override object? GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
  {
    if (OriginalSymbol.IsSource())
    {
      return _runtime.GetProperty(this, obj, index);
    }
    else
    {
      return UnderlyingSystemProperty.GetValue(obj, invokeAttr, binder, index, culture);
    }
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
  {
    if (OriginalSymbol.IsSource())
    {
      _runtime.SetProperty(this, obj, index, value);
    }
    else
    {
      UnderlyingSystemProperty.SetValue(obj, value, invokeAttr, binder, index, culture);
    }
  }


  // SymbolPropertyInfoBase overrides

  protected override SymbolType DeclaringTypeCore => _context.CreateTypeDelegator(OriginalSymbol.ContainingType);

  protected override SymbolModule ModuleCore => _context.CreateModuleDelegator(OriginalSymbol.ContainingModule);

  protected override SymbolType ReflectedTypeCore => _reflectedType ?? DeclaringTypeCore;

  protected override SymbolMethodInfo[] GetAccessorsCore(bool nonPublic)
  {
    bool includeGetMethod = CanRead && (OriginalSymbol.GetMethod!.DeclaredAccessibility == Accessibility.Public || nonPublic);
    bool includeSetMethod = CanWrite && (OriginalSymbol.SetMethod!.DeclaredAccessibility == Accessibility.Public || nonPublic);

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
    return CanRead && (OriginalSymbol.GetMethod!.DeclaredAccessibility == Accessibility.Public || nonPublic)
      ? _reflectedType is not null
        ? new SymbolMethodInfo(_runtime, _context, OriginalSymbol.GetMethod, _reflectedType)
        : _context.CreateMethodInfoDelegator(OriginalSymbol.GetMethod)
      : null;
  }

  protected override SymbolArgumentParameter[] GetIndexParametersCore()
  {
    return OriginalSymbol.Parameters.Map(parameter => new SymbolArgumentParameter(_runtime, _context, this, parameter));
  }

  protected override SymbolMethodInfo? GetSetMethodCore(bool nonPublic)
  {
    return CanWrite && (OriginalSymbol.SetMethod!.DeclaredAccessibility == Accessibility.Public || nonPublic)
      ? _reflectedType is not null
        ? new SymbolMethodInfo(_runtime, _context, OriginalSymbol.SetMethod, _reflectedType)
        : _context.CreateMethodInfoDelegator(OriginalSymbol.SetMethod)
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
  // System.Reflection.PropertyInfo overrides

  public sealed override Type DeclaringType => DeclaringTypeCore;

  public sealed override Type ReflectedType => ReflectedTypeCore;

  public sealed override Module Module => ModuleCore;

  public sealed override MethodInfo[] GetAccessors(bool nonPublic) => GetAccessorsCore(nonPublic);

  public sealed override MethodInfo? GetGetMethod(bool nonPublic) => GetGetMethodCore(nonPublic);

  public sealed override ParameterInfo[] GetIndexParameters() => GetIndexParametersCore();

  public sealed override MethodInfo? GetSetMethod(bool nonPublic) => GetSetMethodCore(nonPublic);


  // Abstract members

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