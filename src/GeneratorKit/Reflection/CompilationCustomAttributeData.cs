using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

namespace GeneratorKit.Reflection;

internal class CompilationCustomAttributeData : CustomAttributeData
{
  private CompilationCustomAttributeData(ConstructorInfo constructor,
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

  public static CompilationCustomAttributeData FromSymbol(
    GeneratorRuntime runtime,
    IConstructorSymbol symbol,
    IList<CustomAttributeTypedArgument> constructorArguments,
    IList<CustomAttributeNamedArgument> namedArguments)
  {
    ConstructorInfo constructor = runtime.CreateConstructorDelegator(symbol);
    return new CompilationCustomAttributeData(constructor, constructorArguments, namedArguments);
  }

  public static ConstructedCustomAttributeData FromParameterlessAttribute(GeneratorRuntime runtime, INamedTypeSymbol attribute)
  {
    if (attribute.InstanceConstructors.Length != 1)
      throw new ArgumentException("Symbol must contain a single parameterless constructor.", nameof(attribute));

    return FromSymbol(runtime, attribute.InstanceConstructors[0], Array.Empty<CustomAttributeTypedArgument>(), Array.Empty<CustomAttributeNamedArgument>());
  }

  public static CompilationCustomAttributeData FromAttributeData(GeneratorRuntime runtime, AttributeData data)
  {
    ConstructorInfo constructor = runtime.CreateConstructorInfoDelegator(data.AttributeConstructor!);
    IList<CustomAttributeTypedArgument> constructorArguments = data.ConstructorArguments
      .Select(x => CreateCustomAttributeTypedArgument(runtime, x))
      .ToList();
    IList<CustomAttributeNamedArgument> namedArguments = data.NamedArguments
      .Select(x => new CustomAttributeNamedArgument(GetMember(runtime, data.AttributeClass!, x.Key), CreateCustomAttributeTypedArgument(runtime, x.Value)))
      .ToList();

    return new CompilationCustomAttributeData(constructor, constructorArguments, namedArguments);

    static MemberInfo GetMember(GeneratorRuntime runtime, INamedTypeSymbol attributeType, string memberName)
    {
      ISymbol member = attributeType.GetMembers(memberName)[0];
      return member.Kind switch
      {
        SymbolKind.Property => runtime.CreatePropertyInfoDelegator((IPropertySymbol)member),
        SymbolKind.Field    => runtime.CreateFieldInfoDelegator((IFieldSymbol)member),
        _                   => throw new InvalidOperationException()
      };
    }

    static CustomAttributeTypedArgument CreateCustomAttributeTypedArgument(GeneratorRuntime runtime, TypedConstant constant)
    {
      return new CustomAttributeTypedArgument(runtime.CreateTypeDelegator(constant.Type!), constant.Value);
    }
  }
}