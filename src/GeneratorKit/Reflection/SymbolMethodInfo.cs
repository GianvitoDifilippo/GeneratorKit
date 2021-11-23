using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolMethodInfo : MethodInfo
{
  private readonly GeneratorRuntime _runtime;

  public SymbolMethodInfo(GeneratorRuntime runtime, IMethodSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IMethodSymbol Symbol { get; }

  public override MethodAttributes Attributes => throw new NotImplementedException();

  public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

  public override string Name => Symbol.Name;

  public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();

  public override Type ReturnType => base.ReturnType;

  public override Type DeclaringType => _runtime.CreateTypeDelegator(Symbol.ContainingType);

  public override Module Module => throw new NotImplementedException();

  public override Type ReflectedType => throw new NotImplementedException();

  public override ParameterInfo ReturnParameter => throw new NotImplementedException();

  public override MethodInfo GetBaseDefinition()
  {
    throw new NotImplementedException();
  }

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

  public override MethodInfo GetGenericMethodDefinition()
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

  public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
  {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }
}