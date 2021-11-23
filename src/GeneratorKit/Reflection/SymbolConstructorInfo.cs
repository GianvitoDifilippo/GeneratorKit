using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolConstructorInfo : ConstructorInfo
{
  private readonly GeneratorRuntime _runtime;

  public SymbolConstructorInfo(GeneratorRuntime runtime, IMethodSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IMethodSymbol Symbol { get; }

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

  public override Type DeclaringType => _runtime.CreateTypeDelegator(Symbol.ContainingType);

  public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

  public override Module Module => throw new NotImplementedException();

  public override string Name => IsStatic ? TypeConstructorName : ConstructorName;

  public override Type ReflectedType => DeclaringType;

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
    throw new NotImplementedException();
  }

  public override MethodImplAttributes GetMethodImplementationFlags()
  {
    throw new NotImplementedException();
  }

  public override ParameterInfo[] GetParameters()
  {
    return Symbol.Parameters.Select(x => _runtime.CreateParameterInfoDelegator(x)).ToArray();
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
}