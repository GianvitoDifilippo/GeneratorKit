using GeneratorKit.Comparers;
using GeneratorKit.Reflection.Binders;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GeneratorKit.Reflection;

internal class HybridGenericMethod : HybridGenericMethodBase, IRuntimeMethod
{
  private readonly SymbolMethodInfo _definition;
  private readonly Type[] _typeArguments;
  private MethodInfo? _underlyingSystemMethod;

  public HybridGenericMethod(SymbolMethodInfo definition, Type[] typeArguments)
  {
    _definition = definition;
    _typeArguments = typeArguments;
  }

  public MethodInfo UnderlyingSystemMethod => _underlyingSystemMethod ??= DelegatorBinder.ResolveMethod(ReflectedTypeCore.UnderlyingSystemType, this);


  // System.Reflection.MethodInfo overrides

  public override ICustomAttributeProvider ReturnTypeCustomAttributes => _definition.ReturnTypeCustomAttributes;

  public override MethodAttributes Attributes => _definition.Attributes;

  public override RuntimeMethodHandle MethodHandle => UnderlyingSystemMethod.MethodHandle;

  public override string Name => _definition.Name;

  public override object[] GetCustomAttributes(bool inherit)
  {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override Type[] GetGenericArguments()
  {
    return _typeArguments;
  }

  public override MethodImplAttributes GetMethodImplementationFlags()
  {
    throw new NotImplementedException();
  }

  public override ParameterInfo[] GetParameters()
  {
    throw new NotImplementedException();
  }

  public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
  {
    return UnderlyingSystemMethod.Invoke(obj, invokeAttr, binder, parameters, culture);
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }


  // HybridGenericMethodBase overrides

  protected override SymbolType DeclaringTypeCore => _definition.DeclaringType;

  protected override SymbolModule ModuleCore => _definition.Module;

  protected override SymbolType ReflectedTypeCore => _definition.ReflectedType;

  protected override SymbolMethodInfo GetBaseDefinitionCore()
  {
    return _definition.GetBaseDefinition();
  }

  protected override SymbolMethodInfo GetGenericMethodDefinitionCore()
  {
    return _definition;
  }


  // System.Object overrides

  public override bool Equals(object? obj)
  {
    if (obj is not MethodInfo methodInfo)
      return false;

    return MethodInfoEqualityComparer.Default.Equals(this, methodInfo);
  }

  public override int GetHashCode()
  {
    return MethodInfoEqualityComparer.Default.GetHashCode(this);
  }

  public override string ToString()
  {
    StringBuilder builder = new StringBuilder(ReturnType.Name);
    builder.Append(' ');
    builder.Append(Name);
    if (IsGenericMethod)
    {
      Type[] genericArguments = GetGenericArguments();
      builder.Append('[');
      builder.Append(genericArguments[0].Name);
      for (int i = 1; i < genericArguments.Length; i++)
      {
        builder.Append(',');
        builder.Append(genericArguments[i].Name);
      }
      builder.Append(']');
    }
    builder.Append('(');
    ParameterInfo[] parameters = GetParameters();
    if (parameters.Length > 0)
    {
      builder.Append(parameters[0].ParameterType.Name);
      for (int i = 1; i < parameters.Length; i++)
      {
        builder.Append(',');
        builder.Append(' ');
        builder.Append(parameters[i].ParameterType.Name);
      }
    }
    builder.Append(')');

    return builder.ToString();
  }


  // IRuntimeMethod members

  SymbolMethodInfo IRuntimeMethod.Definition => _definition;

  Type[] IRuntimeMethod.TypeArguments => _typeArguments;

  IRuntimeType IRuntimeMethod.DeclaringType => DeclaringType;

  bool IRuntimeMethod.IsOpenGeneric => _typeArguments.Any(t => t.IsGenericParameter); // TODO: Deep check


  // New members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType DeclaringType => DeclaringTypeCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolModule Module => ModuleCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType ReflectedType => ReflectedTypeCore;

  public new SymbolMethodInfo GetBaseDefinition() => GetBaseDefinitionCore();

  public new SymbolMethodInfo GetGenericMethodDefinition() => GetGenericMethodDefinitionCore();
}

internal abstract class HybridGenericMethodBase : MethodInfo
{
  // System.Reflection.MethodInfo overrides

  public sealed override Type DeclaringType => DeclaringTypeCore;

  public sealed override Module Module => ModuleCore;

  public sealed override Type ReflectedType => ReflectedTypeCore;

  public sealed override MethodInfo GetBaseDefinition() => GetBaseDefinitionCore();

  public sealed override MethodInfo GetGenericMethodDefinition() => GetGenericMethodDefinitionCore();


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType DeclaringTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ReflectedTypeCore { get; }

  protected abstract SymbolMethodInfo GetBaseDefinitionCore();

  protected abstract SymbolMethodInfo GetGenericMethodDefinitionCore();
}