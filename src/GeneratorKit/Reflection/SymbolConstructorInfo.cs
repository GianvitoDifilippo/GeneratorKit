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

internal sealed class SymbolConstructorInfo : SymbolConstructorInfoBase
{
  private readonly IRuntime _runtime;
  private readonly IGeneratorContext _context;
  private ConstructorInfo? _underlyingSystemConstructor;

  public SymbolConstructorInfo(IRuntime runtime, IGeneratorContext context, IMethodSymbol symbol)
  {
    _runtime = runtime;
    _context = context;
    OriginalSymbol = symbol;
  }

  public IMethodSymbol OriginalSymbol { get; }

  public Type[] ParameterTypes => OriginalSymbol.Parameters.Map(parameter => _context.GetContextType(parameter.Type));

  public ConstructorInfo UnderlyingSystemConstructor
  {
    get
    {
      Debug.Assert(!OriginalSymbol.IsSource());
      return _underlyingSystemConstructor ??= DelegatorBinder.ResolveConstructor(DeclaringType.UnderlyingSystemType, this);
    }
  }


  // System.Reflection.ConstructorInfo overrides

  public override MethodAttributes Attributes
  {
    get
    {
      MethodAttributes result = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
      switch (OriginalSymbol.DeclaredAccessibility)
      {
        case Accessibility.Private:
          result |= MethodAttributes.Private;
          break;
        case Accessibility.ProtectedAndInternal:
          result |= MethodAttributes.FamANDAssem;
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
        case Accessibility.Public:
          result |= MethodAttributes.Public;
          break;
      }
      if (OriginalSymbol.IsStatic)
        result |= MethodAttributes.Static;

      return result;
    }
  }

  public override CallingConventions CallingConvention => OriginalSymbol.IsStatic
    ? CallingConventions.Standard
    : CallingConventions.Standard | CallingConventions.HasThis;

  public override RuntimeMethodHandle MethodHandle => UnderlyingSystemConstructor.MethodHandle;

  public override bool IsSecurityCritical => true;

  public override bool IsSecuritySafeCritical => false;

  public override bool IsSecurityTransparent => false;

  public override string Name => IsStatic ? TypeConstructorName : ConstructorName;

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
    throw new NotSupportedException();
  }

  public override MethodImplAttributes GetMethodImplementationFlags()
  {
    return OriginalSymbol.MethodImplementationFlags;
  }

  public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
  {
    return _runtime.InvokeConstructor(this, parameters);
  }

  public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
  {
    return _runtime.InvokeConstructor(this, parameters);
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }


  // SymbolConstructorInfoBase overrides

  protected override SymbolType DeclaringTypeCore => _context.CreateTypeDelegator(OriginalSymbol.ContainingType);

  protected override SymbolModule ModuleCore => _context.CreateModuleDelegator(OriginalSymbol.ContainingModule);

  protected override SymbolType ReflectedTypeCore => DeclaringTypeCore;

  protected override SymbolArgumentParameter[] GetParametersCore()
  {
    return OriginalSymbol.Parameters.Map(parameter => new SymbolArgumentParameter(_runtime, _context, this, parameter));
  }


  // System.Object overrides

  public override bool Equals(object? obj)
  {
    if (obj is not ConstructorInfo constructorInfo)
      return false;

    return ConstructorInfoEqualityComparer.Default.Equals(this, constructorInfo);
  }

  public override int GetHashCode()
  {
    return ConstructorInfoEqualityComparer.Default.GetHashCode(this);
  }

  public override string ToString()
  {
    if (OriginalSymbol.IsStatic)
    {
      return "Void .cctor()";
    }
    StringBuilder builder = new StringBuilder("Void .ctor(");
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

  public new SymbolArgumentParameter[] GetParameters() => GetParametersCore();
}

internal abstract class SymbolConstructorInfoBase : ConstructorInfo
{
  // System.Reflection.ConstructorInfo overrides

  public sealed override Type DeclaringType => DeclaringTypeCore;

  public sealed override Module Module => ModuleCore;

  public sealed override Type ReflectedType => ReflectedTypeCore;

  public sealed override ParameterInfo[] GetParameters() => GetParametersCore();


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType DeclaringTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ReflectedTypeCore { get; }

  protected abstract SymbolArgumentParameter[] GetParametersCore();
}