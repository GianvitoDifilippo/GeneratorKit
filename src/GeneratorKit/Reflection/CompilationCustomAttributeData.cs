using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using GeneratorKit.Utils;

namespace GeneratorKit.Reflection;

internal class CompilationCustomAttributeData : CustomAttributeData
{
  private CompilationCustomAttributeData(ConstructorInfo constructor, IList<CustomAttributeTypedArgument> constructorArguments, IList<CustomAttributeNamedArgument> namedArguments)
  {
    Constructor = constructor;
    ConstructorArguments = constructorArguments;
    NamedArguments = namedArguments;
  }

  public override ConstructorInfo Constructor { get; }

  public override IList<CustomAttributeTypedArgument> ConstructorArguments { get; }

  public override IList<CustomAttributeNamedArgument> NamedArguments { get; }

  public static CompilationCustomAttributeData FromSymbol(
    IGeneratorContext context,
    IMethodSymbol symbol,
    IList<CustomAttributeTypedArgument> constructorArguments,
    IList<CustomAttributeNamedArgument> namedArguments)
  {
    ConstructorInfo constructor = context.CreateConstructorInfoDelegator(symbol);
    return new CompilationCustomAttributeData(constructor, constructorArguments, namedArguments);
  }

  public static CompilationCustomAttributeData FromParameterlessAttribute(IGeneratorContext context, INamedTypeSymbol attribute)
  {
    if (attribute.InstanceConstructors.Length != 1)
      throw new ArgumentException("Symbol must contain a single parameterless constructor.", nameof(attribute));

    return FromSymbol(context, attribute.InstanceConstructors[0], Array.Empty<CustomAttributeTypedArgument>(), Array.Empty<CustomAttributeNamedArgument>());
  }

  public static CompilationCustomAttributeData FromAttributeData(IGeneratorContext context, AttributeData data)
  {
    ConstructorInfo constructor = context.CreateConstructorInfoDelegator(data.AttributeConstructor!);
    IList<CustomAttributeTypedArgument> constructorArguments = data.ConstructorArguments
      .Select(c => CreateCustomAttributeTypedArgument(context, c))
      .ToList();
    IList<CustomAttributeNamedArgument> namedArguments = data.NamedArguments
      .Select(arg => new CustomAttributeNamedArgument(GetMember(context, data.AttributeClass!, arg.Key), CreateCustomAttributeTypedArgument(context, arg.Value)))
      .ToList();

    return new CompilationCustomAttributeData(constructor, constructorArguments, namedArguments);

    static MemberInfo GetMember(IGeneratorContext context, INamedTypeSymbol attributeType, string memberName)
    {
      ISymbol member = attributeType.GetMembers(memberName)[0];
      return member.Kind switch
      {
        SymbolKind.Property => context.CreatePropertyInfoDelegator((IPropertySymbol)member),
        SymbolKind.Field    => context.CreateFieldInfoDelegator((IFieldSymbol)member),
        _                   => throw Errors.Unreacheable
      };
    }

    static CustomAttributeTypedArgument CreateCustomAttributeTypedArgument(IGeneratorContext context, TypedConstant constant)
    {
      return new CustomAttributeTypedArgument(context.CreateTypeDelegator(constant.Type!), constant.Value);
    }
  }
}