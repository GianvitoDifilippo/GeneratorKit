using GeneratorKit.TestHelpers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Xunit;
using Xunit.Sdk;

namespace GeneratorKit.Reflection;

public class SymbolPropertyInfoFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName + ".SymbolPropertyInfo";

  private const BindingFlags s_allProperties = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

  private const string s_source =
@"

namespace " + Namespace + @"
{
  public abstract class BaseClass
  {
    public int BaseProperty => throw null;
    public abstract int AbstractProperty { get; set; }
    public abstract int AbstractThenSealedProperty { get; set; }
    public virtual int VirtualPropertyNotOverridden { get => throw null; set => throw null; }
    public virtual int VirtualPropertyOverridden { get => throw null; set => throw null; }
  }

  public class DerivedClass : BaseClass
  {
    public int PublicProperty { get => throw null; set => throw null; }
    internal int InternalProperty { get => throw null; set => throw null; }
    private int PrivateProperty { get => throw null; set => throw null; }
    protected int ProtectedProperty { get => throw null; set => throw null; }
    private protected int PrivateProtectedProperty { get => throw null; set => throw null; }
    protected internal int ProtectedInternalProperty { get => throw null; set => throw null; }

    public static int PublicStaticProperty { get => throw null; set => throw null; }
    internal static int InternalStaticProperty { get => throw null; set => throw null; }
    private static int PrivateStaticProperty { get => throw null; set => throw null; }
    protected static int ProtectedStaticProperty { get => throw null; set => throw null; }
    private protected static int PrivateProtectedStaticProperty { get => throw null; set => throw null; }
    protected internal static int ProtectedInternalStaticProperty { get => throw null; set => throw null; }

    [System.Obsolete]
    public int PropertyWithAttribute { get => throw null; set => throw null; }
    public int PropertyWithConstantValue { get; } = 4;

    public int this[int arg1] => throw null;
    public int this[int arg1, string arg2] => throw null;

    public override int AbstractProperty { get => throw null; set => throw null; }
    public override sealed int AbstractThenSealedProperty { get => throw null; set => throw null; }
    public override int VirtualPropertyOverridden { get => throw null; set => throw null; }

    // Below all possible combinations of accessor modifiers

    public int Public_ProtectedInternal { get => throw null; protected internal set => throw null; }
    public int Public_Protected { get => throw null; protected set => throw null; }
    public int Public_Internal { get => throw null; internal set => throw null; }
    public int Public_PrivateProtected { get => throw null; private protected set => throw null; }
    public int Public_Private { get => throw null; private set => throw null; }
    public int Public_None { get => throw null; }

    protected internal int ProtectedInternal_Protected { get => throw null; protected set => throw null; }
    protected internal int ProtectedInternal_Internal { get => throw null; internal set => throw null; }
    protected internal int ProtectedInternal_PrivateProtected { get => throw null; private protected set => throw null; }
    protected internal int ProtectedInternal_Private { get => throw null; private set => throw null; }
    protected internal int ProtectedInternal_None { get => throw null; }

    protected int Protected_PrivateProtected { get => throw null; private protected set => throw null; }
    protected int Protected_Private { get => throw null; private set => throw null; }
    protected int Protected_None { get => throw null; }

    internal int Internal_PrivateProtected { get => throw null; private protected set => throw null; }
    internal int Internal_Private { get => throw null; private set => throw null; }
    internal int Internal_None { get => throw null; }

    private protected int PrivateProtected_Private { get => throw null; private set => throw null; }
    private protected int PrivateProtected_None { get => throw null; }

    private int Private_None { get => throw null; }

    public int ProtectedInternal_Public { protected internal get => throw null; set => throw null; }
    public int Protected_Public { protected get => throw null; set => throw null; }
    public int Internal_Public { internal get => throw null; set => throw null; }
    public int PrivateProtected_Public { private protected get => throw null; set => throw null; }
    public int Private_Public { private get => throw null; set => throw null; }
    public int None_Public { set => throw null; }

    protected internal int Protected_ProtectedInternal { protected get => throw null; set => throw null; }
    protected internal int Internal_ProtectedInternal { internal get => throw null; set => throw null; }
    protected internal int PrivateProtected_ProtectedInternal { private protected get => throw null; set => throw null; }
    protected internal int Private_ProtectedInternal { private get => throw null; set => throw null; }
    protected internal int None_ProtectedInternal { set => throw null; }

    protected int PrivateProtected_Protected { private protected get => throw null; set => throw null; }
    protected int Private_Protected { private get => throw null; set => throw null; }
    protected int None_Protected { set => throw null; }

    internal int PrivateProtected_Internal { private protected get => throw null; set => throw null; }
    internal int Private_Internal { private get => throw null; set => throw null; }
    internal int None_Internal { set => throw null; }

    private protected int Private_PrivateProtected { private get => throw null; set => throw null; }
    private protected int None_PrivateProtected { set => throw null; }

    private int None_Private { set => throw null; }
  }
}

";

  private readonly FakeGeneratorRuntime _runtime;
  private readonly Type _baseType;
  private readonly Type _derivedType;
  private readonly INamedTypeSymbol _baseSymbol;
  private readonly INamedTypeSymbol _derivedSymbol;

  public SymbolPropertyInfoFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName);
    Assert.True(output.IsValid, $"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");

    _runtime = new FakeGeneratorRuntime(output.Compilation);

    _baseType = output.Assembly!.GetType(Namespace + ".BaseClass")!;
    _derivedType = output.Assembly!.GetType(Namespace + ".DerivedClass")!;

    _baseSymbol = output.Compilation.GetTypeByMetadataName(Namespace + ".BaseClass")!;
    _derivedSymbol = output.Compilation.GetTypeByMetadataName(Namespace + ".DerivedClass")!;

    _runtime.AddType(_baseSymbol, _baseType);
    _runtime.AddType(_derivedSymbol, _derivedType);
  }

  public PropertyInfo GetReference(PropertyCategory category)
  {
    return category switch
    {
      PropertyCategory.PublicProperty
        => GetPropertyFromType(_derivedType, "PublicProperty"),

      PropertyCategory.InternalProperty
        => GetPropertyFromType(_derivedType, "InternalProperty"),

      PropertyCategory.PrivateProperty
        => GetPropertyFromType(_derivedType, "PrivateProperty"),

      PropertyCategory.ProtectedProperty
        => GetPropertyFromType(_derivedType, "ProtectedProperty"),

      PropertyCategory.PrivateProtectedProperty
        => GetPropertyFromType(_derivedType, "PrivateProtectedProperty"),

      PropertyCategory.ProtectedInternalProperty
        => GetPropertyFromType(_derivedType, "ProtectedInternalProperty"),

      PropertyCategory.PublicStaticProperty
        => GetPropertyFromType(_derivedType, "PublicStaticProperty"),

      PropertyCategory.InternalStaticProperty
        => GetPropertyFromType(_derivedType, "InternalStaticProperty"),

      PropertyCategory.PrivateStaticProperty
        => GetPropertyFromType(_derivedType, "PrivateStaticProperty"),

      PropertyCategory.ProtectedStaticProperty
        => GetPropertyFromType(_derivedType, "ProtectedStaticProperty"),

      PropertyCategory.PrivateProtectedStaticProperty
        => GetPropertyFromType(_derivedType, "PrivateProtectedStaticProperty"),

      PropertyCategory.ProtectedInternalStaticProperty
        => GetPropertyFromType(_derivedType, "ProtectedInternalStaticProperty"),

      PropertyCategory.PropertyWithAttribute
        => GetPropertyFromType(_derivedType, "PropertyWithAttribute"),

      PropertyCategory.PropertyWithConstantValue
        => GetPropertyFromType(_derivedType, "PropertyWithConstantValue"),

      PropertyCategory.BaseProperty
        => GetPropertyFromType(_baseType, "BaseProperty"),

      PropertyCategory.BasePropertyReflectedFromDerived
        => GetPropertyFromType(_derivedType, "BaseProperty"),

      PropertyCategory.AbstractProperty
        => GetPropertyFromType(_baseType, "AbstractProperty"),

      PropertyCategory.Indexer1
        => GetPublicIndexerFromType(_derivedType, new Type[] { typeof(int) }),

      PropertyCategory.Indexer2
        => GetPublicIndexerFromType(_derivedType, new Type[] { typeof(int), typeof(string) }),

      PropertyCategory.AbstractPropertyReflectedFromDerived
        => GetPropertyFromType(_derivedType, "AbstractProperty"),

      PropertyCategory.AbstractThenSealedProperty
        => GetPropertyFromType(_baseType, "AbstractThenSealedProperty"),

      PropertyCategory.AbstractThenSealedPropertyReflectedFromDerived
        => GetPropertyFromType(_derivedType, "AbstractThenSealedProperty"),

      PropertyCategory.VirtualPropertyNotOverridden
        => GetPropertyFromType(_baseType, "VirtualPropertyNotOverridden"),

      PropertyCategory.VirtualPropertyNotOverriddenReflectedFromDerived
        => GetPropertyFromType(_derivedType, "VirtualPropertyNotOverridden"),

      PropertyCategory.VirtualPropertyOverridden
        => GetPropertyFromType(_baseType, "VirtualPropertyOverridden"),

      PropertyCategory.VirtualPropertyOverriddenReflectedFromDerived
        => GetPropertyFromType(_derivedType, "VirtualPropertyOverridden"),

      PropertyCategory.Public_ProtectedInternal
        => GetPropertyFromType(_derivedType, "Public_ProtectedInternal"),

      PropertyCategory.Public_Protected
        => GetPropertyFromType(_derivedType, "Public_Protected"),

      PropertyCategory.Public_Internal
        => GetPropertyFromType(_derivedType, "Public_Internal"),

      PropertyCategory.Public_PrivateProtected
        => GetPropertyFromType(_derivedType, "Public_PrivateProtected"),

      PropertyCategory.Public_Private
        => GetPropertyFromType(_derivedType, "Public_Private"),

      PropertyCategory.Public_None
        => GetPropertyFromType(_derivedType, "Public_None"),

      PropertyCategory.ProtectedInternal_Protected
        => GetPropertyFromType(_derivedType, "ProtectedInternal_Protected"),

      PropertyCategory.ProtectedInternal_Internal
        => GetPropertyFromType(_derivedType, "ProtectedInternal_Internal"),

      PropertyCategory.ProtectedInternal_PrivateProtected
        => GetPropertyFromType(_derivedType, "ProtectedInternal_PrivateProtected"),

      PropertyCategory.ProtectedInternal_Private
        => GetPropertyFromType(_derivedType, "ProtectedInternal_Private"),

      PropertyCategory.ProtectedInternal_None
        => GetPropertyFromType(_derivedType, "ProtectedInternal_None"),

      PropertyCategory.Protected_PrivateProtected
        => GetPropertyFromType(_derivedType, "Protected_PrivateProtected"),

      PropertyCategory.Protected_Private
        => GetPropertyFromType(_derivedType, "Protected_Private"),

      PropertyCategory.Protected_None
        => GetPropertyFromType(_derivedType, "Protected_None"),

      PropertyCategory.Internal_PrivateProtected
        => GetPropertyFromType(_derivedType, "Internal_PrivateProtected"),

      PropertyCategory.Internal_Private
        => GetPropertyFromType(_derivedType, "Internal_Private"),

      PropertyCategory.Internal_None
        => GetPropertyFromType(_derivedType, "Internal_None"),

      PropertyCategory.PrivateProtected_Private
        => GetPropertyFromType(_derivedType, "PrivateProtected_Private"),

      PropertyCategory.PrivateProtected_None
        => GetPropertyFromType(_derivedType, "PrivateProtected_None"),

      PropertyCategory.Private_None
        => GetPropertyFromType(_derivedType, "Private_None"),

      PropertyCategory.ProtectedInternal_Public
        => GetPropertyFromType(_derivedType, "ProtectedInternal_Public"),

      PropertyCategory.Protected_Public
        => GetPropertyFromType(_derivedType, "Protected_Public"),

      PropertyCategory.Internal_Public
        => GetPropertyFromType(_derivedType, "Internal_Public"),

      PropertyCategory.PrivateProtected_Public
        => GetPropertyFromType(_derivedType, "PrivateProtected_Public"),

      PropertyCategory.Private_Public
        => GetPropertyFromType(_derivedType, "Private_Public"),

      PropertyCategory.None_Public
        => GetPropertyFromType(_derivedType, "None_Public"),

      PropertyCategory.Protected_ProtectedInternal
        => GetPropertyFromType(_derivedType, "Protected_ProtectedInternal"),

      PropertyCategory.Internal_ProtectedInternal
        => GetPropertyFromType(_derivedType, "Internal_ProtectedInternal"),

      PropertyCategory.PrivateProtected_ProtectedInternal
        => GetPropertyFromType(_derivedType, "PrivateProtected_ProtectedInternal"),

      PropertyCategory.Private_ProtectedInternal
        => GetPropertyFromType(_derivedType, "Private_ProtectedInternal"),

      PropertyCategory.None_ProtectedInternal
        => GetPropertyFromType(_derivedType, "None_ProtectedInternal"),

      PropertyCategory.PrivateProtected_Protected
        => GetPropertyFromType(_derivedType, "PrivateProtected_Protected"),

      PropertyCategory.Private_Protected
        => GetPropertyFromType(_derivedType, "Private_Protected"),

      PropertyCategory.None_Protected
        => GetPropertyFromType(_derivedType, "None_Protected"),

      PropertyCategory.PrivateProtected_Internal
        => GetPropertyFromType(_derivedType, "PrivateProtected_Internal"),

      PropertyCategory.Private_Internal
        => GetPropertyFromType(_derivedType, "Private_Internal"),

      PropertyCategory.None_Internal
        => GetPropertyFromType(_derivedType, "None_Internal"),

      PropertyCategory.Private_PrivateProtected
        => GetPropertyFromType(_derivedType, "Private_PrivateProtected"),

      PropertyCategory.None_PrivateProtected
        => GetPropertyFromType(_derivedType, "None_PrivateProtected"),

      PropertyCategory.None_Private
        => GetPropertyFromType(_derivedType, "None_Private"),

      _
        => throw new InvalidOperationException()
    };

    static PropertyInfo GetPropertyFromType(Type type, string name)
    {
      PropertyInfo? result = type.GetProperty(name, s_allProperties);
      Assert.NotNull(result);
      return result!;
    }

    static PropertyInfo GetPublicIndexerFromType(Type type, Type[] types)
    {
      PropertyInfo? result = type.GetProperty("Item", null, types);
      Assert.NotNull(result);
      return result!;
    }
  }

  internal SymbolPropertyInfo GetDelegator(PropertyCategory category)
  {
    IPropertySymbol symbol = category switch
    {
      PropertyCategory.PublicProperty
        => GetPropertyFromType(_derivedSymbol, "PublicProperty"),

      PropertyCategory.InternalProperty
        => GetPropertyFromType(_derivedSymbol, "InternalProperty"),

      PropertyCategory.PrivateProperty
        => GetPropertyFromType(_derivedSymbol, "PrivateProperty"),

      PropertyCategory.ProtectedProperty
        => GetPropertyFromType(_derivedSymbol, "ProtectedProperty"),

      PropertyCategory.PrivateProtectedProperty
        => GetPropertyFromType(_derivedSymbol, "PrivateProtectedProperty"),

      PropertyCategory.ProtectedInternalProperty
        => GetPropertyFromType(_derivedSymbol, "ProtectedInternalProperty"),

      PropertyCategory.PublicStaticProperty
        => GetPropertyFromType(_derivedSymbol, "PublicStaticProperty"),

      PropertyCategory.InternalStaticProperty
        => GetPropertyFromType(_derivedSymbol, "InternalStaticProperty"),

      PropertyCategory.PrivateStaticProperty
        => GetPropertyFromType(_derivedSymbol, "PrivateStaticProperty"),

      PropertyCategory.ProtectedStaticProperty
        => GetPropertyFromType(_derivedSymbol, "ProtectedStaticProperty"),

      PropertyCategory.PrivateProtectedStaticProperty
        => GetPropertyFromType(_derivedSymbol, "PrivateProtectedStaticProperty"),

      PropertyCategory.ProtectedInternalStaticProperty
        => GetPropertyFromType(_derivedSymbol, "ProtectedInternalStaticProperty"),

      PropertyCategory.PropertyWithAttribute
        => GetPropertyFromType(_derivedSymbol, "PropertyWithAttribute"),

      PropertyCategory.PropertyWithConstantValue
        => GetPropertyFromType(_derivedSymbol, "PropertyWithConstantValue"),

      PropertyCategory.BaseProperty or
      PropertyCategory.BasePropertyReflectedFromDerived
        => GetPropertyFromType(_baseSymbol, "BaseProperty"),

      PropertyCategory.Indexer1
        => _derivedSymbol.GetMembers("this[]").Cast<IPropertySymbol>().Single(x => x.Parameters.Length == 1),

      PropertyCategory.Indexer2
        => _derivedSymbol.GetMembers("this[]").Cast<IPropertySymbol>().Single(x => x.Parameters.Length == 2),

      PropertyCategory.AbstractProperty
        => GetPropertyFromType(_baseSymbol, "AbstractProperty"),

      PropertyCategory.AbstractPropertyReflectedFromDerived
        => GetPropertyFromType(_derivedSymbol, "AbstractProperty"),

      PropertyCategory.AbstractThenSealedProperty
        => GetPropertyFromType(_baseSymbol, "AbstractThenSealedProperty"),

      PropertyCategory.AbstractThenSealedPropertyReflectedFromDerived
        => GetPropertyFromType(_derivedSymbol, "AbstractThenSealedProperty"),

      PropertyCategory.VirtualPropertyNotOverridden or
      PropertyCategory.VirtualPropertyNotOverriddenReflectedFromDerived
        => GetPropertyFromType(_baseSymbol, "VirtualPropertyNotOverridden"),

      PropertyCategory.VirtualPropertyOverridden
        => GetPropertyFromType(_baseSymbol, "VirtualPropertyOverridden"),

      PropertyCategory.VirtualPropertyOverriddenReflectedFromDerived
        => GetPropertyFromType(_derivedSymbol, "VirtualPropertyOverridden"),

      PropertyCategory.Public_ProtectedInternal
        => GetPropertyFromType(_derivedSymbol, "Public_ProtectedInternal"),

      PropertyCategory.Public_Protected
        => GetPropertyFromType(_derivedSymbol, "Public_Protected"),

      PropertyCategory.Public_Internal
        => GetPropertyFromType(_derivedSymbol, "Public_Internal"),

      PropertyCategory.Public_PrivateProtected
        => GetPropertyFromType(_derivedSymbol, "Public_PrivateProtected"),

      PropertyCategory.Public_Private
        => GetPropertyFromType(_derivedSymbol, "Public_Private"),

      PropertyCategory.Public_None
        => GetPropertyFromType(_derivedSymbol, "Public_None"),

      PropertyCategory.ProtectedInternal_Protected
        => GetPropertyFromType(_derivedSymbol, "ProtectedInternal_Protected"),

      PropertyCategory.ProtectedInternal_Internal
        => GetPropertyFromType(_derivedSymbol, "ProtectedInternal_Internal"),

      PropertyCategory.ProtectedInternal_PrivateProtected
        => GetPropertyFromType(_derivedSymbol, "ProtectedInternal_PrivateProtected"),

      PropertyCategory.ProtectedInternal_Private
        => GetPropertyFromType(_derivedSymbol, "ProtectedInternal_Private"),

      PropertyCategory.ProtectedInternal_None
        => GetPropertyFromType(_derivedSymbol, "ProtectedInternal_None"),

      PropertyCategory.Protected_PrivateProtected
        => GetPropertyFromType(_derivedSymbol, "Protected_PrivateProtected"),

      PropertyCategory.Protected_Private
        => GetPropertyFromType(_derivedSymbol, "Protected_Private"),

      PropertyCategory.Protected_None
        => GetPropertyFromType(_derivedSymbol, "Protected_None"),

      PropertyCategory.Internal_PrivateProtected
        => GetPropertyFromType(_derivedSymbol, "Internal_PrivateProtected"),

      PropertyCategory.Internal_Private
        => GetPropertyFromType(_derivedSymbol, "Internal_Private"),

      PropertyCategory.Internal_None
        => GetPropertyFromType(_derivedSymbol, "Internal_None"),

      PropertyCategory.PrivateProtected_Private
        => GetPropertyFromType(_derivedSymbol, "PrivateProtected_Private"),

      PropertyCategory.PrivateProtected_None
        => GetPropertyFromType(_derivedSymbol, "PrivateProtected_None"),

      PropertyCategory.Private_None
        => GetPropertyFromType(_derivedSymbol, "Private_None"),

      PropertyCategory.ProtectedInternal_Public
        => GetPropertyFromType(_derivedSymbol, "ProtectedInternal_Public"),

      PropertyCategory.Protected_Public
        => GetPropertyFromType(_derivedSymbol, "Protected_Public"),

      PropertyCategory.Internal_Public
        => GetPropertyFromType(_derivedSymbol, "Internal_Public"),

      PropertyCategory.PrivateProtected_Public
        => GetPropertyFromType(_derivedSymbol, "PrivateProtected_Public"),

      PropertyCategory.Private_Public
        => GetPropertyFromType(_derivedSymbol, "Private_Public"),

      PropertyCategory.None_Public
        => GetPropertyFromType(_derivedSymbol, "None_Public"),

      PropertyCategory.Protected_ProtectedInternal
        => GetPropertyFromType(_derivedSymbol, "Protected_ProtectedInternal"),

      PropertyCategory.Internal_ProtectedInternal
        => GetPropertyFromType(_derivedSymbol, "Internal_ProtectedInternal"),

      PropertyCategory.PrivateProtected_ProtectedInternal
        => GetPropertyFromType(_derivedSymbol, "PrivateProtected_ProtectedInternal"),

      PropertyCategory.Private_ProtectedInternal
        => GetPropertyFromType(_derivedSymbol, "Private_ProtectedInternal"),

      PropertyCategory.None_ProtectedInternal
        => GetPropertyFromType(_derivedSymbol, "None_ProtectedInternal"),

      PropertyCategory.PrivateProtected_Protected
        => GetPropertyFromType(_derivedSymbol, "PrivateProtected_Protected"),

      PropertyCategory.Private_Protected
        => GetPropertyFromType(_derivedSymbol, "Private_Protected"),

      PropertyCategory.None_Protected
        => GetPropertyFromType(_derivedSymbol, "None_Protected"),

      PropertyCategory.PrivateProtected_Internal
        => GetPropertyFromType(_derivedSymbol, "PrivateProtected_Internal"),

      PropertyCategory.Private_Internal
        => GetPropertyFromType(_derivedSymbol, "Private_Internal"),

      PropertyCategory.None_Internal
        => GetPropertyFromType(_derivedSymbol, "None_Internal"),

      PropertyCategory.Private_PrivateProtected
        => GetPropertyFromType(_derivedSymbol, "Private_PrivateProtected"),

      PropertyCategory.None_PrivateProtected
        => GetPropertyFromType(_derivedSymbol, "None_PrivateProtected"),

      PropertyCategory.None_Private
        => GetPropertyFromType(_derivedSymbol, "None_Private"),

      _ => throw new InvalidOperationException()
    };

    return NeedsReflectedType(category)
      ? new SymbolPropertyInfo(_runtime, symbol, _runtime.CreateTypeDelegator(_derivedSymbol))
      : new SymbolPropertyInfo(_runtime, symbol);

    static IPropertySymbol GetPropertyFromType(INamedTypeSymbol symbol, string name)
    {
      return (IPropertySymbol)symbol.GetMembers().Single(x => x.Kind is SymbolKind.Property && x.Name == name);
    }

    static bool NeedsReflectedType(PropertyCategory category)
    {
      return category is
        PropertyCategory.BasePropertyReflectedFromDerived or
        PropertyCategory.VirtualPropertyNotOverriddenReflectedFromDerived;
    }
  }
}

public class PropertiesDataAttribute : DataAttribute
{
  public PropertyCategory[] Except { get; set; } = Array.Empty<PropertyCategory>();

  public override IEnumerable<object[]> GetData(MethodInfo testMethod)
  {
    return Enum.GetValues<PropertyCategory>().Where(x => !Except.Contains(x)).Select(x => new object[1] { x });
  }
}

public class PropertiesDataExceptAttribute : PropertiesDataAttribute
{
  public PropertiesDataExceptAttribute(params PropertyCategory[] except)
  {
    Except = except;
  }
}

public enum PropertyCategory
{
  PublicProperty,
  InternalProperty,
  PrivateProperty,
  ProtectedProperty,
  PrivateProtectedProperty,
  ProtectedInternalProperty,
  PublicStaticProperty,
  InternalStaticProperty,
  PrivateStaticProperty,
  ProtectedStaticProperty,
  PrivateProtectedStaticProperty,
  ProtectedInternalStaticProperty,
  PropertyWithAttribute,
  PropertyWithConstantValue,
  BaseProperty,
  BasePropertyReflectedFromDerived,
  Indexer1,
  Indexer2,
  AbstractProperty,
  AbstractPropertyReflectedFromDerived,
  AbstractThenSealedProperty,
  AbstractThenSealedPropertyReflectedFromDerived,
  VirtualPropertyNotOverridden,
  VirtualPropertyNotOverriddenReflectedFromDerived,
  VirtualPropertyOverridden,
  VirtualPropertyOverriddenReflectedFromDerived,

  Public_ProtectedInternal,
  Public_Protected,
  Public_Internal,
  Public_PrivateProtected,
  Public_Private,
  Public_None,
  ProtectedInternal_Protected,
  ProtectedInternal_Internal,
  ProtectedInternal_PrivateProtected,
  ProtectedInternal_Private,
  ProtectedInternal_None,
  Protected_PrivateProtected,
  Protected_Private,
  Protected_None,
  Internal_PrivateProtected,
  Internal_Private,
  Internal_None,
  PrivateProtected_Private,
  PrivateProtected_None,
  Private_None,
  ProtectedInternal_Public,
  Protected_Public,
  Internal_Public,
  PrivateProtected_Public,
  Private_Public,
  None_Public,
  Protected_ProtectedInternal,
  Internal_ProtectedInternal,
  PrivateProtected_ProtectedInternal,
  Private_ProtectedInternal,
  None_ProtectedInternal,
  PrivateProtected_Protected,
  Private_Protected,
  None_Protected,
  PrivateProtected_Internal,
  Private_Internal,
  None_Internal,
  Private_PrivateProtected,
  None_PrivateProtected,
  None_Private,
}