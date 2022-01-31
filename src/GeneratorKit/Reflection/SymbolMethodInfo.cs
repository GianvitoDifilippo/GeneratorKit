using GeneratorKit.Comparers;
using GeneratorKit.Interpret;
using GeneratorKit.Reflection.Binders;
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

internal class SymbolMethodInfo : SymbolMethodInfoBase, IRuntimeMethod
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

  // TODO: Assert not in source
  internal MethodInfo UnderlyingSystemMethod => _underlyingSystemMethod ??= DelegatorBinder.ResolveMethod(ReflectedType.UnderlyingSystemType, this);


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

  public override bool ContainsGenericParameters =>
    Symbol.IsGenericMethod && Symbol.TypeArguments.Any(t => t.TypeKind is TypeKind.TypeParameter) ||
    DeclaringTypeCore is not null && DeclaringTypeCore.ContainsGenericParameters;

  public override bool IsGenericMethod => Symbol.IsGenericMethod;

  public override bool IsGenericMethodDefinition => Symbol.IsGenericMethod && Symbol.TypeArguments.All(x => x.TypeKind is TypeKind.TypeParameter);

  public override bool IsSecurityCritical => true;

  public override bool IsSecuritySafeCritical => false;

  public override bool IsSecurityTransparent => false;

  public override int MetadataToken => throw new NotSupportedException();

  public override RuntimeMethodHandle MethodHandle => throw new NotSupportedException();

  public override string Name => Symbol.Name;

  public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();

  public override Delegate CreateDelegate(Type delegateType)
  {
    throw new NotSupportedException();
  }

  public override Delegate CreateDelegate(Type delegateType, object target)
  {
    throw new NotSupportedException();
  }

  public override MethodBody GetMethodBody()
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

  public override MethodImplAttributes GetMethodImplementationFlags()
  {
    return Symbol.MethodImplementationFlags;
  }

  public override object? Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
  {
    return Symbol.ContainingAssembly is ISourceAssemblySymbol
      ? _runtime.InvokeMethod(this, obj, parameters)
      : UnderlyingSystemMethod.Invoke(obj, invokeAttr, binder, parameters, culture);
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
    return Symbol.TypeArguments.Select(x => _runtime.CreateTypeDelegator(x)).ToArray();
  }

  protected override SymbolMethodInfo GetGenericMethodDefinitionCore()
  {
    if (!IsGenericMethod)
      throw new InvalidOperationException();

    return _runtime.CreateMethodInfoDelegator(Symbol.ConstructedFrom);
  }

  protected override SymbolArgumentParameter[] GetParametersCore()
  {
    return Symbol.Parameters.Select(x => new SymbolArgumentParameter(_runtime, x)).ToArray();
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

  public new SymbolMethodInfo GetGenericMethodDefinition() => GetGenericMethodDefinitionCore();

  public new SymbolArgumentParameter[] GetParameters() => GetParametersCore();


  // IRuntimeMethod members

  SymbolMethodInfo IRuntimeMethod.Definition => IsConstructedGenericMethod ? GetGenericMethodDefinitionCore() : this;

  Type[] IRuntimeMethod.TypeArguments => IsGenericMethod ? GetGenericArguments() : Type.EmptyTypes;

  IRuntimeType IRuntimeMethod.DeclaringType => DeclaringType;

  bool IRuntimeMethod.IsSource => Symbol.ContainingAssembly is ISourceAssemblySymbol;


  // Other members

  public bool IsConstructedGenericMethod => Symbol.IsGenericMethod && Symbol.TypeArguments.Any(x => x.TypeKind is not TypeKind.TypeParameter);

  public SymbolMethodInfo MakeGenericMethod(params SymbolType[] typeArguments)
  {
    ITypeSymbol[] typeSymbolArguments = typeArguments.Select(x => x.Symbol).ToArray();
    IMethodSymbol constructed = Symbol.Construct(typeSymbolArguments);

    return _reflectedType is null
      ? _runtime.CreateMethodInfoDelegator(constructed)
      : new SymbolMethodInfo(_runtime, constructed, _reflectedType);
  }

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
}

internal abstract class SymbolMethodInfoBase : MethodInfo
{
  // System.Reflection.MethodInfo overrides

  public sealed override Type DeclaringType => DeclaringTypeCore;

  public sealed override Module Module => ModuleCore;

  public sealed override Type ReflectedType => ReflectedTypeCore;

  public sealed override ParameterInfo ReturnParameter => ReturnParameterCore;

  public sealed override Type ReturnType => ReturnTypeCore;

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
  protected abstract SymbolType ReflectedTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ReturnTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolParameterInfo ReturnParameterCore { get; }

  protected abstract SymbolMethodInfo GetBaseDefinitionCore();

  protected abstract SymbolType[] GetGenericArgumentsCore();

  protected abstract SymbolMethodInfo GetGenericMethodDefinitionCore();

  protected abstract SymbolArgumentParameter[] GetParametersCore();
}