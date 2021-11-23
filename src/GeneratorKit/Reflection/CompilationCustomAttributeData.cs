using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

namespace GeneratorKit.Reflection;

internal class CompilationCustomAttributeData : CustomAttributeData
{
  public CompilationCustomAttributeData(GeneratorRuntime runtime, AttributeData data)
  {
    Constructor = runtime.CreateConstructorInfoDelegator(data.AttributeConstructor!);
    ConstructorArguments = CreateConstructorArguments(runtime, data);
    NamedArguments = CreateNamedArguments(runtime, data);
  }

  public override ConstructorInfo Constructor { get; }

  public override IList<CustomAttributeTypedArgument> ConstructorArguments { get; }

  public override IList<CustomAttributeNamedArgument> NamedArguments { get; }

  private static IList<CustomAttributeTypedArgument> CreateConstructorArguments(GeneratorRuntime runtime, AttributeData data)
  {
    return data.ConstructorArguments
      .Select(x => CreateCustomAttributeTypedArgument(runtime, x))
      .ToList();
  }

  private static IList<CustomAttributeNamedArgument> CreateNamedArguments(GeneratorRuntime runtime, AttributeData data)
  {
    return data.NamedArguments
      .Select(x => new CustomAttributeNamedArgument(GetMember(runtime, data.AttributeClass!, x.Key), CreateCustomAttributeTypedArgument(runtime, x.Value)))
      .ToList();

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
  }

  private static CustomAttributeTypedArgument CreateCustomAttributeTypedArgument(GeneratorRuntime runtime, TypedConstant constant)
  {
    return new CustomAttributeTypedArgument(runtime.CreateTypeDelegator(constant.Type!), constant.Value);
  }
}