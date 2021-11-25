using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolConstructorInfo : SymbolConstructorInfoBase
{
  private readonly GeneratorRuntime _runtime;

  public SymbolConstructorInfo(GeneratorRuntime runtime, IMethodSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IMethodSymbol Symbol { get; }


  // System.Reflection.ConstructorInfo overrides

  public override MethodAttributes Attributes
  {
    get
    {
      MethodAttributes result = default;
      switch (Symbol.DeclaredAccessibility)
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
      if (Symbol.IsStatic)
        result |= MethodAttributes.Static;

      return result;
    }
  }

  public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

  public override string Name => IsStatic ? TypeConstructorName : ConstructorName;

  public override object[] GetCustomAttributes(bool inherit)
  {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override MethodImplAttributes GetMethodImplementationFlags()
  {
    throw new NotImplementedException();
  }

  public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
  {
    throw new NotImplementedException();
  }

  public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
  {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }


  // SymbolConstructorInfoBase overrides

  protected override SymbolType DeclaringTypeCore => _runtime.CreateTypeDelegator(Symbol.ContainingType);

  protected override SymbolModule ModuleCore => throw new NotImplementedException();

  protected override SymbolType ReflectedTypeCore => DeclaringTypeCore;

  protected override SymbolType[] GetGenericArgumentsCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolParameterInfo[] GetParametersCore()
  {
    return Symbol.Parameters.Select(x => _runtime.CreateParameterInfoDelegator(x)).ToArray();
  }
}

internal abstract class SymbolConstructorInfoBase : ConstructorInfo
{
  private protected SymbolConstructorInfoBase() { }


  // System.Reflection.ConstructorInfo overrides

  public sealed override Type DeclaringType => DeclaringTypeCore;

  public sealed override Module Module => ModuleCore;

  public sealed override Type ReflectedType => ReflectedTypeCore;

  public sealed override Type[] GetGenericArguments() => GetGenericArgumentsCore();

  public sealed override ParameterInfo[] GetParameters() => GetParametersCore();


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType DeclaringTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ReflectedTypeCore { get; }

  protected abstract SymbolType[] GetGenericArgumentsCore();

  protected abstract SymbolParameterInfo[] GetParametersCore();
}