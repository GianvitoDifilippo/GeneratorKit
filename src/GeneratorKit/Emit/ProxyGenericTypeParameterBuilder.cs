using GeneratorKit.Reflection;
using System.Reflection.Emit;

namespace GeneratorKit.Emit;

internal class ProxyGenericTypeParameterBuilder
{
  public static void BuildGenericTypeParameter(GenericTypeParameterBuilder genericTypeParameterBuilder, SymbolType genericArgument)
  {
    genericTypeParameterBuilder.SetGenericParameterAttributes(genericArgument.GenericParameterAttributes);
    foreach (SymbolType constraint in genericArgument.GetGenericParameterConstraints())
    {
      if (constraint.IsClass)
      {
        genericTypeParameterBuilder.SetBaseTypeConstraint(constraint.RuntimeType);
      }
      else
      {
        genericTypeParameterBuilder.SetInterfaceConstraints(constraint.RuntimeType);
      }
    }
  }
}