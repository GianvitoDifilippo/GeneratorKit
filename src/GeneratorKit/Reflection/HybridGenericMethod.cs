using System;
using System.Globalization;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal class HybridGenericMethod : MethodInfo
{
  private readonly SymbolMethodInfo _definition;
  private readonly Type[] _typeArguments;

  public HybridGenericMethod(SymbolMethodInfo definition, Type[] typeArguments)
  {
    _definition = definition;
    _typeArguments = typeArguments;
  }

  public override ICustomAttributeProvider ReturnTypeCustomAttributes => _definition.ReturnTypeCustomAttributes;

  public override MethodAttributes Attributes => _definition.Attributes;

  public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

  public override Type DeclaringType => _definition.DeclaringType;

  public override string Name => _definition.Name;

  public override Type ReflectedType => _definition.ReflectedType;

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
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }
}