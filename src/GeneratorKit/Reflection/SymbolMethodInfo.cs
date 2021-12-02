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

namespace GeneratorKit.Reflection;

internal sealed class SymbolMethodInfo : SymbolMethodInfoBase
{
  private readonly GeneratorRuntime _runtime;
  private readonly SymbolType? _reflectedType;
  private MethodInfo? _runtimeMethod;

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

  public MethodInfo RuntimeMethod
  {
    get
    {
      if (_runtimeMethod is null)
      {
        BindingFlags bindingAttr =
          (Symbol.DeclaredAccessibility is Accessibility.Public ? BindingFlags.Public : BindingFlags.NonPublic) |
          (Symbol.IsStatic ? BindingFlags.Static : BindingFlags.Instance);

        if (!IsGenericMethodDefinition && IsGenericMethod)
        {
          Type[] genericArguments = GetGenericArguments().Select(x => x.RuntimeType).ToArray();
          return GetGenericMethodDefinitionCore(true).RuntimeMethod.MakeGenericMethod(genericArguments);
        }

        int genericParameterCount = IsGenericMethod ? GetGenericArgumentsCore().Length : 0;
        _runtimeMethod = ReflectedTypeCore.RuntimeType.GetMethod(Name, bindingAttr, new DelegatorBinder(genericParameterCount), CallingConvention, GetParametersCore().Select(x => x.ParameterType).ToArray(), null);
      }
      return _runtimeMethod;
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

  public override RuntimeMethodHandle MethodHandle => RuntimeMethod.MethodHandle;

  public override string Name => Symbol.Name;

  public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();

  public override Delegate CreateDelegate(Type delegateType)
  {
    return RuntimeMethod.CreateDelegate(delegateType);
  }

  public override Delegate CreateDelegate(Type delegateType, object target)
  {
    return RuntimeMethod.CreateDelegate(delegateType, target);
  }

  public override MethodBody GetMethodBody()
  {
    return RuntimeMethod.GetMethodBody();
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
    return RuntimeMethod.Invoke(obj, invokeAttr, binder, parameters, culture);
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }


  // SymbolMethodInfoBase overrides

  protected override SymbolType DeclaringTypeCore => _runtime.CreateTypeDelegator(Symbol.ContainingType);

  protected override SymbolModule ModuleCore => _runtime.CreateModuleDelegator(Symbol.ContainingModule);

  protected override SymbolType ReflectedTypeCore => _reflectedType ?? DeclaringTypeCore;

  protected override SymbolArgumentParameter ReturnParameterCore => throw new NotImplementedException();

  protected override SymbolType ReturnTypeCore => _runtime.CreateTypeDelegator(Symbol.ReturnType);

  protected override SymbolMethodInfo GetBaseDefinitionCore()
  {
    return Symbol.IsOverride
      ? IsGenericMethod && !IsGenericMethodDefinition
        ? _runtime.CreateMethodInfoDelegator(Symbol.OriginalDefinition.OverriddenMethod!).MakeGenericMethodCore(GetGenericArgumentsCore())
        : _runtime.CreateMethodInfoDelegator(Symbol.OverriddenMethod!)
      : Symbol.IsVirtual && _reflectedType is not null
        ? _runtime.CreateMethodInfoDelegator(Symbol)
        : this;
  }

  protected override SymbolType[] GetGenericArgumentsCore()
  {
    return Symbol.TypeArguments.Select(x => _runtime.CreateTypeDelegator(x)).ToArray();
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

  protected override SymbolArgumentParameter[] GetParametersCore()
  {
    return Symbol.Parameters.Select(x => new SymbolArgumentParameter(_runtime, x)).ToArray();
  }

  protected override SymbolMethodInfo MakeGenericMethodCore(params Type[] typeArguments)
  {
    if (!IsGenericMethodDefinition)
    {
      throw new InvalidOperationException();
    }
    ITypeSymbol[] typeSymbolArguments = typeArguments.Select(x => _runtime.GetTypeSymbol(x) ?? throw new InvalidOperationException()).ToArray();
    IMethodSymbol constructed = Symbol.Construct(typeSymbolArguments);
    return _reflectedType is null
      ? _runtime.CreateMethodInfoDelegator(constructed)
      : new SymbolMethodInfo(_runtime, constructed, _reflectedType);
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


  // New members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType DeclaringType => DeclaringTypeCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolModule Module => ModuleCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType ReflectedType => ReflectedTypeCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolArgumentParameter ReturnParameter => ReturnParameterCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType ReturnType => ReturnTypeCore;

  public new SymbolMethodInfo GetBaseDefinition() => GetBaseDefinitionCore();

  public new SymbolType[] GetGenericArguments() => GetGenericArgumentsCore();

  public new SymbolMethodInfo GetGenericMethodDefinition() => GetGenericMethodDefinitionCore(false);

  public new SymbolArgumentParameter[] GetParameters() => GetParametersCore();

  public new SymbolMethodInfo MakeGenericMethod(params Type[] typeArguments) => MakeGenericMethodCore(typeArguments);
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

  public sealed override MethodInfo MakeGenericMethod(params Type[] typeArguments) => MakeGenericMethodCore(typeArguments);


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
  protected abstract SymbolArgumentParameter ReturnParameterCore { get; }

  protected abstract SymbolMethodInfo GetBaseDefinitionCore();

  protected abstract SymbolType[] GetGenericArgumentsCore();

  protected abstract SymbolMethodInfo GetGenericMethodDefinitionCore(bool preserveReflectedType);

  protected abstract SymbolArgumentParameter[] GetParametersCore();

  protected abstract SymbolMethodInfo MakeGenericMethodCore(params Type[] typeArguments);
}