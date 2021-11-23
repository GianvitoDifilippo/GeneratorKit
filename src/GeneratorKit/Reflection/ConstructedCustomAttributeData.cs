using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal class ConstructedCustomAttributeData : CustomAttributeData
{
  public ConstructedCustomAttributeData(
    ConstructorInfo constructor,
    IList<CustomAttributeTypedArgument> constructorArguments,
    IList<CustomAttributeNamedArgument> namedArguments)
  {
    Constructor = constructor;
    ConstructorArguments = constructorArguments;
    NamedArguments = namedArguments;
  }

  public override ConstructorInfo Constructor { get; }

  public override IList<CustomAttributeTypedArgument> ConstructorArguments { get; }

  public override IList<CustomAttributeNamedArgument> NamedArguments { get; }

  public static ConstructedCustomAttributeData CreateParameterlessAttribute(GeneratorRuntime runtime, INamedTypeSymbol attribute)
  {
    if (attribute.InstanceConstructors.Length != 1)
    {
      throw new ArgumentException("Symbol must contain a single parameterless constructor.", nameof(attribute));
    }

    return new ConstructedCustomAttributeData(
      runtime.CreateConstructorInfoDelegator(attribute.InstanceConstructors[0]),
      Array.Empty<CustomAttributeTypedArgument>(),
      Array.Empty<CustomAttributeNamedArgument>());
  }
}