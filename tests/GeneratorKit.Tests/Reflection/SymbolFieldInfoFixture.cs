using GeneratorKit.Reflection.Context;
using GeneratorKit.TestHelpers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace GeneratorKit.Reflection;

public class SymbolFieldInfoFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName + ".SymbolFieldInfo";

  private const BindingFlags s_allFields = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

  private const string s_source =
@"

namespace " + Namespace + @"
{
  public abstract class BaseClass
  {
    public int BaseField;
  }

  public class DerivedClass : BaseClass
  {
    public int PublicField;
    internal int InternalField;
    private int PrivateField;
    protected int ProtectedField;
    private protected int PrivateProtectedField;
    protected internal int ProtectedInternalField;

    public static int PublicStaticField;
    internal static int InternalStaticField;
    private static int PrivateStaticField;
    protected static int ProtectedStaticField;
    private protected static int PrivateProtectedStaticField;
    protected internal static int ProtectedInternalStaticField;

    [System.Obsolete]
    public int FieldWithAttribute;
    [System.NonSerialized]
    public int NotSerializedField;
    public int FieldWithConstantValue = 4;
    public const int ConstField = 2;

    public readonly int ReadonlyField;

    public int ForBackingFieldProperty { get; set; }
  }
}

";

  private readonly FakeReflectionRuntime _runtime;
  private readonly Type _baseType;
  private readonly Type _derivedType;
  private readonly INamedTypeSymbol _baseSymbol;
  private readonly INamedTypeSymbol _derivedSymbol;

  public SymbolFieldInfoFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName);
    Assert.True(output.IsValid, $"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");

    _runtime = new FakeReflectionRuntime(output.Compilation);

    _baseType = output.Assembly!.GetType(Namespace + ".BaseClass")!;
    _derivedType = output.Assembly!.GetType(Namespace + ".DerivedClass")!;

    _baseSymbol = output.Compilation.GetTypeByMetadataName(Namespace + ".BaseClass")!;
    _derivedSymbol = output.Compilation.GetTypeByMetadataName(Namespace + ".DerivedClass")!;

    _runtime.AddType(_baseSymbol, _baseType);
    _runtime.AddType(_derivedSymbol, _derivedType);
  }

  public FieldInfo GetReference(FieldCategory category)
  {
    return category switch
    {
      FieldCategory.PublicField
        => GetFieldFromType(_derivedType, "PublicField"),

      FieldCategory.InternalField
        => GetFieldFromType(_derivedType, "InternalField"),

      FieldCategory.PrivateField
        => GetFieldFromType(_derivedType, "PrivateField"),

      FieldCategory.ProtectedField
        => GetFieldFromType(_derivedType, "ProtectedField"),

      FieldCategory.PrivateProtectedField
        => GetFieldFromType(_derivedType, "PrivateProtectedField"),

      FieldCategory.ProtectedInternalField
        => GetFieldFromType(_derivedType, "ProtectedInternalField"),

      FieldCategory.PublicStaticField
        => GetFieldFromType(_derivedType, "PublicStaticField"),

      FieldCategory.InternalStaticField
        => GetFieldFromType(_derivedType, "InternalStaticField"),

      FieldCategory.PrivateStaticField
        => GetFieldFromType(_derivedType, "PrivateStaticField"),

      FieldCategory.ProtectedStaticField
        => GetFieldFromType(_derivedType, "ProtectedStaticField"),

      FieldCategory.PrivateProtectedStaticField
        => GetFieldFromType(_derivedType, "PrivateProtectedStaticField"),

      FieldCategory.ProtectedInternalStaticField
        => GetFieldFromType(_derivedType, "ProtectedInternalStaticField"),

      FieldCategory.FieldWithAttribute
        => GetFieldFromType(_derivedType, "FieldWithAttribute"),

      FieldCategory.NotSerializedField
        => GetFieldFromType(_derivedType, "NotSerializedField"),

      FieldCategory.FieldWithConstantValue
        => GetFieldFromType(_derivedType, "FieldWithConstantValue"),

      FieldCategory.ConstField
        => GetFieldFromType(_derivedType, "ConstField"),

      FieldCategory.BaseField
        => GetFieldFromType(_baseType, "BaseField"),

      FieldCategory.BaseFieldReflectedFromDerived
        => GetFieldFromType(_derivedType, "BaseField"),

      FieldCategory.ReadonlyField
        => GetFieldFromType(_derivedType, "ReadonlyField"),

      FieldCategory.BackingField
        => GetFieldFromType(_derivedType, "<ForBackingFieldProperty>k__BackingField"),

      _
        => throw new InvalidOperationException()
    };

    static FieldInfo GetFieldFromType(Type type, string name)
    {
      FieldInfo? result = type.GetField(name, s_allFields);
      Assert.NotNull(result);
      return result!;
    }
  }

  internal SymbolFieldInfo GetDelegator(FieldCategory category)
  {
    IFieldSymbol symbol = category switch
    {
      FieldCategory.PublicField
        => GetFieldFromType(_derivedSymbol, "PublicField"),

      FieldCategory.InternalField
        => GetFieldFromType(_derivedSymbol, "InternalField"),

      FieldCategory.PrivateField
        => GetFieldFromType(_derivedSymbol, "PrivateField"),

      FieldCategory.ProtectedField
        => GetFieldFromType(_derivedSymbol, "ProtectedField"),

      FieldCategory.PrivateProtectedField
        => GetFieldFromType(_derivedSymbol, "PrivateProtectedField"),

      FieldCategory.ProtectedInternalField
        => GetFieldFromType(_derivedSymbol, "ProtectedInternalField"),

      FieldCategory.PublicStaticField
        => GetFieldFromType(_derivedSymbol, "PublicStaticField"),

      FieldCategory.InternalStaticField
        => GetFieldFromType(_derivedSymbol, "InternalStaticField"),

      FieldCategory.PrivateStaticField
        => GetFieldFromType(_derivedSymbol, "PrivateStaticField"),

      FieldCategory.ProtectedStaticField
        => GetFieldFromType(_derivedSymbol, "ProtectedStaticField"),

      FieldCategory.PrivateProtectedStaticField
        => GetFieldFromType(_derivedSymbol, "PrivateProtectedStaticField"),

      FieldCategory.ProtectedInternalStaticField
        => GetFieldFromType(_derivedSymbol, "ProtectedInternalStaticField"),

      FieldCategory.FieldWithAttribute
        => GetFieldFromType(_derivedSymbol, "FieldWithAttribute"),

      FieldCategory.NotSerializedField
        => GetFieldFromType(_derivedSymbol, "NotSerializedField"),

      FieldCategory.FieldWithConstantValue
        => GetFieldFromType(_derivedSymbol, "FieldWithConstantValue"),

      FieldCategory.ConstField
        => GetFieldFromType(_derivedSymbol, "ConstField"),

      FieldCategory.BaseField or
      FieldCategory.BaseFieldReflectedFromDerived
        => GetFieldFromType(_baseSymbol, "BaseField"),

      FieldCategory.ReadonlyField
        => GetFieldFromType(_derivedSymbol, "ReadonlyField"),

      FieldCategory.BackingField
        => GetFieldFromType(_derivedSymbol, "<ForBackingFieldProperty>k__BackingField"),

      _ => throw new InvalidOperationException()
    };

    DefaultGeneratorContext context = new DefaultGeneratorContext(_runtime);
    return NeedsReflectedType(category)
      ? new SymbolFieldInfo(_runtime, context, symbol, new SymbolNamedType(_runtime, context, _derivedSymbol))
      : new SymbolFieldInfo(_runtime, context, symbol, null);

    static IFieldSymbol GetFieldFromType(INamedTypeSymbol symbol, string name)
    {
      return (IFieldSymbol)symbol.GetMembers().Single(x => x.Kind is SymbolKind.Field && x.Name == name);
    }

    static bool NeedsReflectedType(FieldCategory category)
    {
      return category is FieldCategory.BaseFieldReflectedFromDerived;
    }
  }
}

public class FieldsDataAttribute : DataAttribute
{
  public FieldCategory[] Except { get; set; } = Array.Empty<FieldCategory>();

  public override IEnumerable<object[]> GetData(MethodInfo testMethod)
  {
    return Enum.GetValues<FieldCategory>().Where(x => !Except.Contains(x)).Select(x => new object[1] { x });
  }
}

public class FieldsDataExceptAttribute : FieldsDataAttribute
{
  public FieldsDataExceptAttribute(params FieldCategory[] except)
  {
    Except = except;
  }
}

public enum FieldCategory
{
  PublicField,
  InternalField,
  PrivateField,
  ProtectedField,
  PrivateProtectedField,
  ProtectedInternalField,
  PublicStaticField,
  InternalStaticField,
  PrivateStaticField,
  ProtectedStaticField,
  PrivateProtectedStaticField,
  ProtectedInternalStaticField,
  FieldWithAttribute,
  NotSerializedField,
  FieldWithConstantValue,
  ConstField,
  BaseField,
  BaseFieldReflectedFromDerived,
  ReadonlyField,
  BackingField
}