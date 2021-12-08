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

internal sealed class SymbolMethodInfo : SymbolMethodInfoBase
{
  private readonly GeneratorRuntime _runtime;
  private readonly SymbolType? _reflectedType;
  private MethodInfo? _underlyingSystemMethod;

  public SymbolMethodInfo(GeneratorRuntime runtime, IMethodSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public SymbolMethodInfo(GeneratorRuntime runtime, IMethodSymbol symbol, SymbolType reflectedType)
    : this(runtime, symbol)
  {
    _reflectedType = reflectedType;
  }

  public IMethodSymbol Symbol { get; }

  internal MethodInfo UnderlyingSystemMethod
  {
    get
    {
      if (_underlyingSystemMethod is null)
      {
        if (!IsGenericMethodDefinition && IsGenericMethod)
        {
          Type[] genericArguments = GetGenericArguments().Map(x => x.UnderlyingSystemType);
          return GetGenericMethodDefinitionCore(true).UnderlyingSystemMethod.MakeGenericMethod(genericArguments);
        }

        _underlyingSystemMethod = MemberResolver.ResolveMethod(ReflectedTypeCore.UnderlyingSystemType, this);
      }
      return _underlyingSystemMethod;
    }
  }


  // System.Reflection.MethodInfo overrides

  public override MethodAttributes Attributes
  {
    get
    {
      MethodAttributes result = MethodAttributes.HideBySig;

      switch (Symbol.DeclaredAccessibility)
      {
        case Accessibility.Public:
          result |= MethodAttributes.Public;
          break;
        case Accessibility.ProtectedOrInternal:
          result |= MethodAttributes.FamORAssem;
          break;
        case Accessibility.Protected:
          result |= MethodAttributes.Family;
          break;
        case Accessibility.Internal:
          result |= MethodAttributes.Assembly;
          break;
        case Accessibility.ProtectedAndInternal:
          result |= MethodAttributes.FamANDAssem;
          break;
        case Accessibility.Private:
          result |= MethodAttributes.Private;
          break;
      }

      if (ImplementsAnyInterfaceMethod)
        result |= MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.NewSlot;
      if (Symbol.IsStatic)
        result |= MethodAttributes.Static;
      if (Symbol.IsSealed)
        result |= MethodAttributes.Final | MethodAttributes.Virtual;
      if (Symbol.IsOverride || Symbol.IsVirtual)
        result |= MethodAttributes.Virtual;
      if (Symbol.IsAbstract)
        result |= MethodAttributes.Abstract | MethodAttributes.Virtual;
      if (Symbol.DeclaredAccessibility is Accessibility.ProtectedAndInternal && (Symbol.IsAbstract || Symbol.IsVirtual))
        result |= MethodAttributes.CheckAccessOnOverride;
      if ((Symbol.IsVirtual || Symbol.IsAbstract) && !Symbol.IsOverride)
        result |= MethodAttributes.NewSlot;

      return result;
    }
  }

  public override CallingConventions CallingConvention => Symbol.IsStatic
    ? CallingConventions.Standard
    : CallingConventions.Standard | CallingConventions.HasThis;

  public override bool ContainsGenericParameters
  {
    get
    {
      if (!Symbol.IsGenericMethod) return false;

      for (int i = 0; i < Symbol.TypeArguments.Length; i++)
      {
        if (Symbol.TypeArguments[i].Equals(Symbol.TypeParameters[i], SymbolEqualityComparer.Default))
        {
          return true;
        }
      }

      return false;
    }
  }

  public override bool IsGenericMethod => Symbol.IsGenericMethod;

  public override bool IsGenericMethodDefinition => Symbol.IsDefinition && Symbol.IsGenericMethod;

  public override bool IsSecurityCritical => true;

  public override bool IsSecuritySafeCritical => false;

  public override bool IsSecurityTransparent => false;

  public override int MetadataToken => throw new NotSupportedException();

  public override RuntimeMethodHandle MethodHandle => UnderlyingSystemMethod.MethodHandle;

  public override string Name => Symbol.Name;

  public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();

  public override Delegate CreateDelegate(Type delegateType)
  {
    return UnderlyingSystemMethod.CreateDelegate(delegateType);
  }

  public override Delegate CreateDelegate(Type delegateType, object target)
  {
    return UnderlyingSystemMethod.CreateDelegate(delegateType, target);
  }

  public override MethodBody GetMethodBody()
  {
    return UnderlyingSystemMethod.GetMethodBody();
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

  public override MethodImplAttributes GetMethodImplementationFlags()
  {
    return Symbol.MethodImplementationFlags;
  }

  public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
  {
    return UnderlyingSystemMethod.Invoke(obj, invokeAttr, binder, parameters, culture);
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
  {
    if (!IsGenericMethodDefinition)
      throw new InvalidOperationException();

    if (typeArguments is SymbolType[] symbolTypeArguments)
      return MakeGenericMethod(symbolTypeArguments);

    return new HybridGenericMethod(this, typeArguments);
  }


  // SymbolMethodInfoBase overrides

  protected override SymbolType DeclaringTypeCore => _runtime.CreateTypeDelegator(Symbol.ContainingType);

  protected override SymbolModule ModuleCore => _runtime.CreateModuleDelegator(Symbol.ContainingModule);

  protected override SymbolType ReflectedTypeCore => _reflectedType ?? DeclaringTypeCore;

  protected override SymbolParameterInfo ReturnParameterCore => new SymbolReturnParameter(_runtime, Symbol);

  protected override SymbolType ReturnTypeCore => _runtime.CreateTypeDelegator(Symbol.ReturnType);

  protected override SymbolMethodInfo GetBaseDefinitionCore()
  {
    return Symbol.IsOverride
      ? IsGenericMethod && !IsGenericMethodDefinition
        ? _runtime.CreateMethodInfoDelegator(Symbol.OriginalDefinition.OverriddenMethod!).MakeGenericMethod(GetGenericArgumentsCore())
        : _runtime.CreateMethodInfoDelegator(Symbol.OverriddenMethod!)
      : Symbol.IsVirtual && _reflectedType is not null
        ? _runtime.CreateMethodInfoDelegator(Symbol)
        : this;
  }

  protected override SymbolType[] GetGenericArgumentsCore()
  {
    return Symbol.TypeArguments.Map(x => _runtime.CreateTypeDelegator(x));
  }

  // The 'preserveReflectedType' parameter is a hack to make the reflected types of 'RuntimeMethod' and 'GetGenericMethodDefinition'
  // correct. Not sure how the runtime decides when to preserve the reflected type - subject needs further investigation.
  protected override SymbolMethodInfo GetGenericMethodDefinitionCore(bool preserveReflectedType)
  {
    if (!IsGenericMethod)
    {
      throw new InvalidOperationException();
    }
    preserveReflectedType |= Symbol.IsOverride && Symbol.IsVirtual;
    return preserveReflectedType && _reflectedType is not null
      ? new SymbolMethodInfo(_runtime, Symbol.OriginalDefinition, _reflectedType)
      : _runtime.CreateMethodInfoDelegator(Symbol.OriginalDefinition);
  }

  protected override SymbolParameterInfo[] GetParametersCore()
  {
    return Symbol.Parameters.Map(x => new SymbolArgumentParameter(_runtime, x));
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


  // New members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType DeclaringType => DeclaringTypeCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolModule Module => ModuleCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType ReflectedType => ReflectedTypeCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolParameterInfo ReturnParameter => ReturnParameterCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType ReturnType => ReturnTypeCore;

  public new SymbolMethodInfo GetBaseDefinition() => GetBaseDefinitionCore();

  public new SymbolType[] GetGenericArguments() => GetGenericArgumentsCore();

  public new SymbolMethodInfo GetGenericMethodDefinition() => GetGenericMethodDefinitionCore(false);

  public new SymbolParameterInfo[] GetParameters() => GetParametersCore();


  // Other members

  private bool ImplementsAnyInterfaceMethod
  {
    get
    {
      if (Symbol.ExplicitInterfaceImplementations.Length != 0)
        return true;

      ITypeSymbol containingType = Symbol.ContainingType;
      foreach (INamedTypeSymbol @interface in containingType.AllInterfaces)
      {
        if (@interface.GetMembers().Any(x => SymbolEqualityComparer.Default.Equals(containingType.FindImplementationForInterfaceMember(x), Symbol)))
        {
          return true;
        }
      }

      return false;
    }
  }

  public SymbolMethodInfo MakeGenericMethod(params SymbolType[] typeArguments)
  {
    ITypeSymbol[] typeSymbolArguments = typeArguments.Map(x => x.Symbol);
    IMethodSymbol constructed = Symbol.Construct(typeSymbolArguments);

    return _reflectedType is null
      ? _runtime.CreateMethodInfoDelegator(constructed)
      : new SymbolMethodInfo(_runtime, constructed, _reflectedType);
  }
}

internal abstract class SymbolMethodInfoBase : MethodInfo
{
  private protected SymbolMethodInfoBase() { }


  // System.Reflection.MethodInfo overrides

  public sealed override Type DeclaringType => DeclaringTypeCore;

  public sealed override Module Module => ModuleCore;

  public sealed override Type ReflectedType => ReflectedTypeCore;

  public sealed override ParameterInfo ReturnParameter => ReturnParameterCore;

  public sealed override Type ReturnType => ReturnTypeCore;

  public sealed override MethodInfo GetBaseDefinition() => GetBaseDefinitionCore();

  public sealed override Type[] GetGenericArguments() => GetGenericArgumentsCore();

  public sealed override MethodInfo GetGenericMethodDefinition() => GetGenericMethodDefinitionCore(false);

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

  protected abstract SymbolMethodInfo GetGenericMethodDefinitionCore(bool preserveReflectedType);

  protected abstract SymbolParameterInfo[] GetParametersCore();
}