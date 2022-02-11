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

internal class SymbolMethodInfo : SymbolMethodInfoBase
{
  private readonly IReflectionRuntime _runtime;
  private readonly IGeneratorContext _context;
  private readonly SymbolType? _reflectedType;
  private MethodInfo? _underlyingSystemMethod;

  public SymbolMethodInfo(IReflectionRuntime runtime, IGeneratorContext context, IMethodSymbol symbol, SymbolType? reflectedType)
  {
    _runtime = runtime;
    _context = context;
    OriginalSymbol = symbol;
    _reflectedType = reflectedType;
  }

  public IMethodSymbol OriginalSymbol { get; }

  public bool IsSource => OriginalSymbol.IsSource();

  public Type[] ParameterTypes => OriginalSymbol.Parameters.Map(parameter => _context.GetContextType(parameter.Type));

  public MethodInfo UnderlyingSystemMethod
  {
    get
    {
      Debug.Assert(!OriginalSymbol.IsSource());
      return _underlyingSystemMethod ??= DelegatorBinder.ResolveMethod(ReflectedType.UnderlyingSystemType, this);
    }
  }


  // System.Reflection.MethodInfo overrides

  public override MethodAttributes Attributes
  {
    get
    {
      MethodAttributes result = MethodAttributes.HideBySig;

      switch (OriginalSymbol.DeclaredAccessibility)
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
      if (OriginalSymbol.IsStatic)
        result |= MethodAttributes.Static;
      if (OriginalSymbol.IsSealed)
        result |= MethodAttributes.Final | MethodAttributes.Virtual;
      if (OriginalSymbol.IsOverride || OriginalSymbol.IsVirtual)
        result |= MethodAttributes.Virtual;
      if (OriginalSymbol.IsAbstract)
        result |= MethodAttributes.Abstract | MethodAttributes.Virtual;
      if (OriginalSymbol.DeclaredAccessibility is Accessibility.ProtectedAndInternal && (OriginalSymbol.IsAbstract || OriginalSymbol.IsVirtual))
        result |= MethodAttributes.CheckAccessOnOverride;
      if ((OriginalSymbol.IsVirtual || OriginalSymbol.IsAbstract) && !OriginalSymbol.IsOverride)
        result |= MethodAttributes.NewSlot;

      return result;
    }
  }

  public override CallingConventions CallingConvention => OriginalSymbol.IsStatic
    ? CallingConventions.Standard
    : CallingConventions.Standard | CallingConventions.HasThis;

  public override bool ContainsGenericParameters => _context.ContainsGenericParameters(OriginalSymbol);

  public override bool IsGenericMethod => OriginalSymbol.IsGenericMethod;

  public override bool IsGenericMethodDefinition => _context.IsGenericMethodDefinition(OriginalSymbol);

  public override bool IsSecurityCritical => true;

  public override bool IsSecuritySafeCritical => false;

  public override bool IsSecurityTransparent => false;

  public override int MetadataToken => throw new NotSupportedException();

  public override RuntimeMethodHandle MethodHandle => throw new NotSupportedException();

  public override string Name => OriginalSymbol.Name;

  public override Type ReturnType => _context.GetContextType(OriginalSymbol.ReturnType);

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
    List<CustomAttributeData> result = OriginalSymbol
      .GetAttributes()
      .Select(data => CompilationCustomAttributeData.FromAttributeData(_context, data) as CustomAttributeData)
      .ToList();

    return new ReadOnlyCollection<CustomAttributeData>(result);
  }

  public override Type[] GetGenericArguments()
  {
    return OriginalSymbol.TypeArguments.Map(_context.GetContextType);
  }

  public override MethodImplAttributes GetMethodImplementationFlags()
  {
    return OriginalSymbol.MethodImplementationFlags;
  }

  public override object? Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
  {
    return _runtime.InvokeMethod(this, obj, parameters);
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  protected override SymbolMethodInfo MakeGenericMethodCore(Type[] typeArguments)
  {
    if (!IsGenericMethodDefinition)
      throw new InvalidOperationException(); // TODO: Message

    return _context.MakeGenericMethod(this, typeArguments, _reflectedType);
  }


  // SymbolMethodInfoBase overrides

  protected override SymbolType DeclaringTypeCore => _context.GetDeclaringType(this);

  protected override SymbolModule ModuleCore => _context.CreateModuleDelegator(OriginalSymbol.ContainingModule);

  protected override SymbolType ReflectedTypeCore => _reflectedType ?? DeclaringTypeCore;

  protected override SymbolParameterInfo ReturnParameterCore => new SymbolReturnParameter(_context, this);

  protected override SymbolMethodInfo GetBaseDefinitionCore()
  {
    return OriginalSymbol.IsOverride
      ? IsGenericMethod && !IsGenericMethodDefinition
        ? _context.CreateMethodInfoDelegator(OriginalSymbol.ConstructedFrom.OverriddenMethod!)
        : _context.CreateMethodInfoDelegator(OriginalSymbol.OverriddenMethod!)
      : OriginalSymbol.IsVirtual && _reflectedType is not null
        ? _context.CreateMethodInfoDelegator(OriginalSymbol)
        : this;
  }

  protected override SymbolMethodInfo GetGenericMethodDefinitionCore()
  {
    if (!IsGenericMethod)
      throw new InvalidOperationException();

    return _context.GetGenericMethodDefinition(this);
  }

  protected override SymbolArgumentParameter[] GetParametersCore()
  {
    return OriginalSymbol.Parameters.Map(parameter => new SymbolArgumentParameter(_runtime, _context, this, parameter));
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

  public new SymbolMethodInfo GetBaseDefinition() => GetBaseDefinitionCore();

  public new SymbolMethodInfo GetGenericMethodDefinition() => GetGenericMethodDefinitionCore();

  public new SymbolArgumentParameter[] GetParameters() => GetParametersCore();

  public new SymbolMethodInfo MakeGenericMethod(params Type[] typeArguments) => MakeGenericMethodCore(typeArguments);


  // Other members

  public bool IsConstructedGenericMethod => OriginalSymbol.IsGenericMethod && OriginalSymbol.TypeArguments.Any(x => x.TypeKind is not TypeKind.TypeParameter);

  private bool ImplementsAnyInterfaceMethod
  {
    get
    {
      if (OriginalSymbol.ExplicitInterfaceImplementations.Length != 0)
        return true;

      ITypeSymbol containingType = OriginalSymbol.ContainingType;
      foreach (INamedTypeSymbol @interface in containingType.AllInterfaces)
      {
        if (@interface.GetMembers().Any(x => SymbolEqualityComparer.Default.Equals(containingType.FindImplementationForInterfaceMember(x), OriginalSymbol)))
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

  public sealed override MethodInfo GetBaseDefinition() => GetBaseDefinitionCore();

  public sealed override MethodInfo GetGenericMethodDefinition() => GetGenericMethodDefinitionCore();

  public sealed override ParameterInfo[] GetParameters() => GetParametersCore();

  public sealed override MethodInfo MakeGenericMethod(params Type[] typeArguments) => MakeGenericMethodCore(typeArguments);


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType DeclaringTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ReflectedTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolParameterInfo ReturnParameterCore { get; }

  protected abstract SymbolMethodInfo GetBaseDefinitionCore();

  protected abstract SymbolMethodInfo GetGenericMethodDefinitionCore();

  protected abstract SymbolArgumentParameter[] GetParametersCore();

  protected abstract SymbolMethodInfo MakeGenericMethodCore(Type[] typeArguments);
}