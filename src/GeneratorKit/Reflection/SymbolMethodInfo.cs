using GeneratorKit.Comparers;
using GeneratorKit.Reflection.Binders;
using GeneratorKit.Reflection.Context;
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
  private readonly IReflectionContext _context;
  private readonly SymbolType? _reflectedType;
  private MethodInfo? _underlyingSystemMethod;

  public SymbolMethodInfo(IReflectionContext context, IMethodSymbol symbol, SymbolType? reflectedType)
  {
    _context = context;
    Symbol = symbol;
    _reflectedType = reflectedType;
  }

  public IMethodSymbol Symbol { get; }

  public bool IsSource => Symbol.IsSource();

  public Type[] ParameterTypes => Symbol.Parameters.Map(parameter => _context.GetContextType(parameter.Type));

  public MethodInfo UnderlyingSystemMethod
  {
    get
    {
      Debug.Assert(!Symbol.IsSource(), "Method must not be source.");
      return _underlyingSystemMethod ??= DelegatorBinder.ResolveMethod(ReflectedType.UnderlyingSystemType, this);
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

  public override bool ContainsGenericParameters => _context.ContainsGenericParameters(Symbol);

  public override bool IsGenericMethod => Symbol.IsGenericMethod;

  public override bool IsGenericMethodDefinition => _context.IsGenericMethodDefinition(Symbol);

  public override bool IsSecurityCritical => true;

  public override bool IsSecuritySafeCritical => false;

  public override bool IsSecurityTransparent => false;

  public override int MetadataToken => throw new NotSupportedException();

  public override RuntimeMethodHandle MethodHandle => throw new NotSupportedException();

  public override string Name => Symbol.Name;

  public override Type ReturnType => _context.GetContextType(Symbol.ReturnType);

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
      .Select(data => CompilationCustomAttributeData.FromAttributeData(_context, data) as CustomAttributeData)
      .ToList();

    return new ReadOnlyCollection<CustomAttributeData>(result);
  }

  public override Type[] GetGenericArguments()
  {
    return Symbol.TypeArguments.Map(_context.GetContextType);
  }

  public override MethodImplAttributes GetMethodImplementationFlags()
  {
    return Symbol.MethodImplementationFlags;
  }

  public override object? Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
  {
    return _context.InvokeMethod(this, obj, parameters);
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

  protected override SymbolNamedType DeclaringTypeCore => _context.GetDeclaringType(this);

  protected override SymbolModule ModuleCore => _context.CreateModuleDelegator(Symbol.ContainingModule);

  protected override SymbolType ReflectedTypeCore => _reflectedType ?? DeclaringTypeCore;

  protected override SymbolParameterInfo ReturnParameterCore => new SymbolReturnParameter(_context, this);

  protected override SymbolMethodInfo GetBaseDefinitionCore()
  {
    return _context.GetBaseDefinition(this, _reflectedType);
  }

  protected override SymbolMethodInfo GetGenericMethodDefinitionCore()
  {
    if (!IsGenericMethod)
      throw new InvalidOperationException();

    return _context.GetGenericMethodDefinition(this);
  }

  protected override SymbolArgumentParameter[] GetParametersCore()
  {
    return Symbol.Parameters.Map(parameter => new SymbolArgumentParameter(_context, this, parameter));
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
  public new SymbolNamedType DeclaringType => DeclaringTypeCore;

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

  public bool IsConstructedGenericMethod => Symbol.IsGenericMethod && Symbol.TypeArguments.Any(x => x.TypeKind is not TypeKind.TypeParameter);

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

  public sealed override MethodInfo GetBaseDefinition() => GetBaseDefinitionCore();

  public sealed override MethodInfo GetGenericMethodDefinition() => GetGenericMethodDefinitionCore();

  public sealed override ParameterInfo[] GetParameters() => GetParametersCore();

  public sealed override MethodInfo MakeGenericMethod(params Type[] typeArguments) => MakeGenericMethodCore(typeArguments);


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolNamedType DeclaringTypeCore { get; }

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