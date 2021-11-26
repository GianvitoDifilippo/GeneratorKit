using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace GeneratorKit.Reflection;

public class SymbolTypeFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName + ".SymbolType";

  private const string s_source = @"

namespace " + Namespace + @"
{
  public class GenericClass<T1, T2> { }

  public class TypeArgument1 { }

  public class TypeArgument2 { }

  [System.Serializable]
  [System.Obsolete(""message"", false, DiagnosticId = ""diagnosticId"")]
  public class ClassWithAttributes { }

  public class ClassWithStaticConstructor
  {
    static ClassWithStaticConstructor() { }
  }

  public abstract class AbstractClass { }

  public sealed class SealedClass { }

  public interface IBaseInterface
  {
    int BasePublicProperty { get; set; }
  }

  public interface IDerivedInterface : IBaseInterface
  {
    int PublicProperty { get; set; }
    int PublicMethod(int arg1, string arg2);
  }

  public struct Struct { }

  public enum Enumeration
  {
    Enum1,
    Enum2
  }

  internal class InternalClass { }

  public class ClassWithAllMembersRoot
  {
    static ClassWithAllMembersRoot() { }
    public ClassWithAllMembersRoot(int param) { }
    internal ClassWithAllMembersRoot(int param1, int param2) { }
    private ClassWithAllMembersRoot(string param) { }
    protected ClassWithAllMembersRoot(string param1, string param2) { }
    private protected ClassWithAllMembersRoot(float param) { }
    protected internal ClassWithAllMembersRoot(float param1, float param2) { }

    public int RootPublicField;
    internal int RootInternalField;
    private int RootPrivateField;
    protected int RootProtectedField;
    private protected int RootPrivateProtectedField;
    protected internal int RootProtectedInternalField;

    public static int RootPublicStaticField;
    internal static int RootInternalStaticField;
    private static int RootPrivateStaticField;
    protected static int RootProtectedStaticField;
    private protected static int RootPrivateProtectedStaticField;
    protected internal static int RootProtectedInternalStaticField;

    public int RootPublicProperty { get; set; }
    internal int RootInternalProperty { get; set; }
    private int RootPrivateProperty { get; set; }
    protected int RootProtectedProperty { get; set; }
    private protected int RootPrivateProtectedProperty { get; set; }
    protected internal int RootProtectedInternalProperty { get; set; }

    public static int RootPublicStaticProperty { get; set; }
    internal static int RootInternalStaticProperty { get; set; }
    private static int RootPrivateStaticProperty { get; set; }
    protected static int RootProtectedStaticProperty { get; set; }
    private protected static int RootPrivateProtectedStaticProperty { get; set; }
    protected internal static int RootProtectedInternalStaticProperty { get; set; }

    public int RootPublicMethod(int arg1, string arg2) => default;
    internal int RootInternalMethod(int arg1, string arg2) => default;
    private int RootPrivateMethod(int arg1, string arg2) => default;
    protected int RootProtectedMethod(int arg1, string arg2) => default;
    private protected int RootPrivateProtectedMethod(int arg1, string arg2) => default;
    protected internal int RootProtectedInternalMethod(int arg1, string arg2) => default;

    public static int RootPublicStaticMethod(int arg1, string arg2) => default;
    internal static int RootInternalStaticMethod(int arg1, string arg2) => default;
    private static int RootPrivateStaticMethod(int arg1, string arg2) => default;
    protected static int RootProtectedStaticMethod(int arg1, string arg2) => default;
    private protected static int RootPrivateProtectedStaticMethod(int arg1, string arg2) => default;
    protected internal static int RootProtectedInternalStaticMethod(int arg1, string arg2) => default;

    public class RootNestedPublicClass { }
    internal class RootNestedInternalClass { }
    private class RootNestedPrivateClass { }
    protected class RootNestedProtectedClass { }
    private protected class RootNestedPrivateProtectedClass { }
    protected internal class RootNestedProtectedInternalClass { }
  }

  public class ClassWithAllMembersBase : ClassWithAllMembersRoot, IBaseInterface
  {
    static ClassWithAllMembersBase() { }
    public ClassWithAllMembersBase(int param) : base(0) { }
    internal ClassWithAllMembersBase(int param1, int param2) : base(0) { }
    private ClassWithAllMembersBase(string param) : base(0) { }
    protected ClassWithAllMembersBase(string param1, string param2) : base(0) { }
    private protected ClassWithAllMembersBase(float param) : base(0) { }
    protected internal ClassWithAllMembersBase(float param1, float param2) : base(0) { }

    public int BasePublicField;
    internal int BaseInternalField;
    private int BasePrivateField;
    protected int BaseProtectedField;
    private protected int BasePrivateProtectedField;
    protected internal int BaseProtectedInternalField;

    public static int BasePublicStaticField;
    internal static int BaseInternalStaticField;
    private static int BasePrivateStaticField;
    protected static int BaseProtectedStaticField;
    private protected static int BasePrivateProtectedStaticField;
    protected internal static int BaseProtectedInternalStaticField;

    public int BasePublicProperty { get; set; }
    internal int BaseInternalProperty { get; set; }
    private int BasePrivateProperty { get; set; }
    protected int BaseProtectedProperty { get; set; }
    private protected int BasePrivateProtectedProperty { get; set; }
    protected internal int BaseProtectedInternalProperty { get; set; }

    public static int BasePublicStaticProperty { get; set; }
    internal static int BaseInternalStaticProperty { get; set; }
    private static int BasePrivateStaticProperty { get; set; }
    protected static int BaseProtectedStaticProperty { get; set; }
    private protected static int BasePrivateProtectedStaticProperty { get; set; }
    protected internal static int BaseProtectedInternalStaticProperty { get; set; }

    public int BasePublicMethod(int arg1, string arg2) => default;
    internal int BaseInternalMethod(int arg1, string arg2) => default;
    private int BasePrivateMethod(int arg1, string arg2) => default;
    protected int BaseProtectedMethod(int arg1, string arg2) => default;
    private protected int BasePrivateProtectedMethod(int arg1, string arg2) => default;
    protected internal int BaseProtectedInternalMethod(int arg1, string arg2) => default;

    public static int BasePublicStaticMethod(int arg1, string arg2) => default;
    internal static int BaseInternalStaticMethod(int arg1, string arg2) => default;
    private static int BasePrivateStaticMethod(int arg1, string arg2) => default;
    protected static int BaseProtectedStaticMethod(int arg1, string arg2) => default;
    private protected static int BasePrivateProtectedStaticMethod(int arg1, string arg2) => default;
    protected internal static int BaseProtectedInternalStaticMethod(int arg1, string arg2) => default;

    public class BaseNestedPublicClass { }
    internal class BaseNestedInternalClass { }
    private class BaseNestedPrivateClass { }
    protected class BaseNestedProtectedClass { }
    private protected class BaseNestedPrivateProtectedClass { }
    protected internal class BaseNestedProtectedInternalClass { }
  }

  public class ClassWithAllMembers : ClassWithAllMembersBase, IDerivedInterface
  {
    static ClassWithAllMembers() { }
    public ClassWithAllMembers(int param) : base(0) { }
    internal ClassWithAllMembers(int param1, int param2) : base(0) { }
    private ClassWithAllMembers(string param) : base(0) { }
    protected ClassWithAllMembers(string param1, string param2) : base(0) { }
    private protected ClassWithAllMembers(float param) : base(0) { }
    protected internal ClassWithAllMembers(float param1, float param2) : base(0) { }

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

    public int PublicProperty { get; set; }
    internal int InternalProperty { get; set; }
    private int PrivateProperty { get; set; }
    protected int ProtectedProperty { get; set; }
    private protected int PrivateProtectedProperty { get; set; }
    protected internal int ProtectedInternalProperty { get; set; }

    public static int PublicStaticProperty { get; set; }
    internal static int InternalStaticProperty { get; set; }
    private static int PrivateStaticProperty { get; set; }
    protected static int ProtectedStaticProperty { get; set; }
    private protected static int PrivateProtectedStaticProperty { get; set; }
    protected internal static int ProtectedInternalStaticProperty { get; set; }

    public int PublicMethod(int arg1, string arg2) => default;
    internal int InternalMethod(int arg1, string arg2) => default;
    private int PrivateMethod(int arg1, string arg2) => default;
    protected int ProtectedMethod(int arg1, string arg2) => default;
    private protected int PrivateProtectedMethod(int arg1, string arg2) => default;
    protected internal int ProtectedInternalMethod(int arg1, string arg2) => default;

    public int PublicGenericMethod<T1, T2>(T1 arg1, T2 arg2) => default;
    internal int InternalGenericMethod<T1, T2>(T1 arg1, T2 arg2) => default;
    private int PrivateGenericMethod<T1, T2>(T1 arg1, T2 arg2) => default;
    protected int ProtectedGenericMethod<T1, T2>(T1 arg1, T2 arg2) => default;
    private protected int PrivateProtectedGenericMethod<T1, T2>(T1 arg1, T2 arg2) => default;
    protected internal int ProtectedInternalGenericMethod<T1, T2>(T1 arg1, T2 arg2) => default;

    public static int PublicStaticMethod(int arg1, string arg2) => default;
    internal static int InternalStaticMethod(int arg1, string arg2) => default;
    private static int PrivateStaticMethod(int arg1, string arg2) => default;
    protected static int ProtectedStaticMethod(int arg1, string arg2) => default;
    private protected static int PrivateProtectedStaticMethod(int arg1, string arg2) => default;
    protected internal static int ProtectedInternalStaticMethod(int arg1, string arg2) => default;

    public int this[int i1] => default;
    public int this[int i1, int i2] => default;
    private int this[int i1, int i2, int i3] => default;

    public class NestedPublicClass { }
    internal class NestedInternalClass { }
    private class NestedPrivateClass { }
    protected class NestedProtectedClass { }
    private protected class NestedPrivateProtectedClass { }
    protected internal class NestedProtectedInternalClass { }
  }

  [System.Reflection.DefaultMember(""DefaultMember"")]
  public class ClassWithDefaultMember
  {
    public int DefaultMember;
  }

  [System.Serializable]
  public class SerializableClass { }

  public interface IWithCovariantParameter<out T1>
  {
    T1 MethodWithTypeParameter();
  }

  public interface IWithContravariantParameter<in T1>
  {
    void MethodWithTwoGenericParameters<T2>(T1 arg1, T2 arg2);
  }

  public interface IWithConstrainedParameter<T1> where T1 : System.Type, IDerivedInterface
  {
    void MethodWithTwoGenericParameters<T2>(T1 arg1, T2 arg2) where T2 : class;
  }
}

";

  private readonly FakeGeneratorRuntime _runtime;
  private readonly Compilation _compilation;
  private readonly Assembly _assembly;
  private readonly IReadOnlyDictionary<TypeCategory, DataPair> _data;

  public SymbolTypeFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName);
    if (!output.IsValid)
    {
      throw new Exception($"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");
    }

    _runtime = new FakeGeneratorRuntime(output.Compilation);
    _compilation = output.Compilation;
    _assembly = output.Assembly!;
    _data = new Dictionary<TypeCategory, DataPair>(CreateData());
  }

  internal SymbolType GetDelegator(TypeCategory category)
  {
    return _data[category].Sut;
  }

  public Type GetReference(TypeCategory category)
  {
    return _data[category].Reference;
  }

  private IEnumerable<KeyValuePair<TypeCategory, DataPair>> CreateData()
  {
    return Enum.GetValues<TypeCategory>().Select(MakeData);

    KeyValuePair<TypeCategory, DataPair> MakeData(TypeCategory category)
    {
      SymbolType symbolType = GetSymbolType(category);
      Type type = GetType(category);
      _runtime.AddType(symbolType.Symbol, type);
      return KeyValuePair.Create(category, new DataPair(symbolType, type));
    }
  }

  private SymbolType GetSymbolType(TypeCategory category)
  {
    ITypeSymbol symbol;
    INamedTypeSymbol namedSymbol;
    INamedTypeSymbol typeArg1Symbol;
    INamedTypeSymbol typeArg2Symbol;
    IMethodSymbol method;
    IParameterSymbol parameter;

    switch (category)
    {
      case TypeCategory.Object:
        symbol = _compilation.GetSpecialType(SpecialType.System_Object);
        break;
      case TypeCategory.Int:
        symbol = _compilation.GetSpecialType(SpecialType.System_Int32);
        break;
      case TypeCategory.Short:
        symbol = _compilation.GetSpecialType(SpecialType.System_Int16);
        break;
      case TypeCategory.Long:
        symbol = _compilation.GetSpecialType(SpecialType.System_Int64);
        break;
      case TypeCategory.Float:
        symbol = _compilation.GetSpecialType(SpecialType.System_Single);
        break;
      case TypeCategory.Double:
        symbol = _compilation.GetSpecialType(SpecialType.System_Double);
        break;
      case TypeCategory.Decimal:
        symbol = _compilation.GetSpecialType(SpecialType.System_Decimal);
        break;
      case TypeCategory.Bool:
        symbol = _compilation.GetSpecialType(SpecialType.System_Boolean);
        break;
      case TypeCategory.IntPtr:
        symbol = _compilation.GetSpecialType(SpecialType.System_IntPtr);
        break;
      case TypeCategory.String:
        symbol = _compilation.GetSpecialType(SpecialType.System_String);
        break;
      case TypeCategory.EnumBase:
        symbol = _compilation.GetSpecialType(SpecialType.System_Enum);
        break;

      case TypeCategory.OpenGeneric:
        symbol = GetSymbolTypeFromCompilation("GenericClass`2");
        break;
      case TypeCategory.ClosedGeneric:
        namedSymbol = GetSymbolTypeFromCompilation("GenericClass`2");
        typeArg1Symbol = GetSymbolTypeFromCompilation("TypeArgument1");
        typeArg2Symbol = GetSymbolTypeFromCompilation("TypeArgument2");
        symbol = namedSymbol.Construct(typeArg1Symbol, typeArg2Symbol);
        break;
      case TypeCategory.ClosedGenericWithGenericTypeArguments:
        namedSymbol = GetSymbolTypeFromCompilation("GenericClass`2");
        typeArg1Symbol = GetSymbolTypeFromCompilation("TypeArgument1");
        typeArg2Symbol = GetSymbolTypeFromCompilation("TypeArgument2");
        symbol = namedSymbol.Construct(namedSymbol.Construct(typeArg1Symbol, typeArg2Symbol), typeArg2Symbol);
        break;
      case TypeCategory.WithAttributes:
        symbol = GetSymbolTypeFromCompilation("ClassWithAttributes");
        break;
      case TypeCategory.WithStaticConstructor:
        symbol = GetSymbolTypeFromCompilation("ClassWithStaticConstructor");
        break;
      case TypeCategory.Internal:
        symbol = GetSymbolTypeFromCompilation("InternalClass");
        break;
      case TypeCategory.NestedPublic:
        symbol = GetSymbolTypeFromCompilation("ClassWithAllMembers");
        symbol = symbol.GetTypeMembers("NestedPublicClass")[0];
        break;
      case TypeCategory.NestedProtectedInternal:
        symbol = GetSymbolTypeFromCompilation("ClassWithAllMembers");
        symbol = symbol.GetTypeMembers("NestedProtectedInternalClass")[0];
        break;
      case TypeCategory.NestedProtected:
        symbol = GetSymbolTypeFromCompilation("ClassWithAllMembers");
        symbol = symbol.GetTypeMembers("NestedProtectedClass")[0];
        break;
      case TypeCategory.NestedInternal:
        symbol = GetSymbolTypeFromCompilation("ClassWithAllMembers");
        symbol = symbol.GetTypeMembers("NestedInternalClass")[0];
        break;
      case TypeCategory.NestedPrivateProtected:
        symbol = GetSymbolTypeFromCompilation("ClassWithAllMembers");
        symbol = symbol.GetTypeMembers("NestedPrivateProtectedClass")[0];
        break;
      case TypeCategory.NestedPrivate:
        symbol = GetSymbolTypeFromCompilation("ClassWithAllMembers");
        symbol = symbol.GetTypeMembers("NestedPrivateClass")[0];
        break;
      case TypeCategory.WithAllMembers:
        symbol = GetSymbolTypeFromCompilation("ClassWithAllMembers");
        break;
      case TypeCategory.WithAllMembersBase:
        symbol = GetSymbolTypeFromCompilation("ClassWithAllMembersBase");
        break;
      case TypeCategory.WithAllMembersRoot:
        symbol = GetSymbolTypeFromCompilation("ClassWithAllMembersRoot");
        break;
      case TypeCategory.WithDefaultMember:
        symbol = GetSymbolTypeFromCompilation("ClassWithDefaultMember");
        break;
      case TypeCategory.BaseInterface:
        symbol = GetSymbolTypeFromCompilation("IBaseInterface");
        break;
      case TypeCategory.DerivedInterface:
        symbol = GetSymbolTypeFromCompilation("IDerivedInterface");
        break;
      case TypeCategory.Struct:
        symbol = GetSymbolTypeFromCompilation("Struct");
        break;
      case TypeCategory.Enum:
        symbol = GetSymbolTypeFromCompilation("Enumeration");
        break;
      case TypeCategory.Abstract:
        symbol = GetSymbolTypeFromCompilation("AbstractClass");
        break;
      case TypeCategory.Sealed:
        symbol = GetSymbolTypeFromCompilation("SealedClass");
        break;
      case TypeCategory.Serializable:
        symbol = GetSymbolTypeFromCompilation("SerializableClass");
        break;

      case TypeCategory.ObjectArray1:
        symbol = _compilation.CreateArrayTypeSymbol(_compilation.GetSpecialType(SpecialType.System_Object));
        break;
      case TypeCategory.ObjectArray2:
        symbol = _compilation.CreateArrayTypeSymbol(_compilation.GetSpecialType(SpecialType.System_Object), 2);
        break;
      case TypeCategory.IntArray1:
        symbol = _compilation.CreateArrayTypeSymbol(_compilation.GetSpecialType(SpecialType.System_Int32));
        break;
      case TypeCategory.IntArray2:
        symbol = _compilation.CreateArrayTypeSymbol(_compilation.GetSpecialType(SpecialType.System_Int32), 2);
        break;
      case TypeCategory.OpenGenericArray1:
        symbol = _compilation.CreateArrayTypeSymbol(GetSymbolTypeFromCompilation("GenericClass`2"));
        break;
      case TypeCategory.OpenGenericArray2:
        symbol = _compilation.CreateArrayTypeSymbol(GetSymbolTypeFromCompilation("GenericClass`2"), 2);
        break;
      case TypeCategory.ClosedGenericArray1:
        namedSymbol = GetSymbolTypeFromCompilation("GenericClass`2");
        typeArg1Symbol = GetSymbolTypeFromCompilation("TypeArgument1");
        typeArg2Symbol = GetSymbolTypeFromCompilation("TypeArgument2");
        symbol = _compilation.CreateArrayTypeSymbol(namedSymbol.Construct(typeArg1Symbol, typeArg2Symbol));
        break;
      case TypeCategory.ClosedGenericArray2:
        namedSymbol = GetSymbolTypeFromCompilation("GenericClass`2");
        typeArg1Symbol = GetSymbolTypeFromCompilation("TypeArgument1");
        typeArg2Symbol = GetSymbolTypeFromCompilation("TypeArgument2");
        symbol = _compilation.CreateArrayTypeSymbol(namedSymbol.Construct(typeArg1Symbol, typeArg2Symbol), 2);
        break;
      case TypeCategory.ClassArray1:
        symbol = _compilation.CreateArrayTypeSymbol(GetSymbolTypeFromCompilation("InternalClass"));
        break;
      case TypeCategory.ClassArray2:
        symbol = _compilation.CreateArrayTypeSymbol(GetSymbolTypeFromCompilation("InternalClass"), 2);
        break;
      case TypeCategory.InterfaceArray1:
        symbol = _compilation.CreateArrayTypeSymbol(GetSymbolTypeFromCompilation("IDerivedInterface"));
        break;
      case TypeCategory.InterfaceArray2:
        symbol = _compilation.CreateArrayTypeSymbol(GetSymbolTypeFromCompilation("IDerivedInterface"), 2);
        break;
      case TypeCategory.StructArray1:
        symbol = _compilation.CreateArrayTypeSymbol(GetSymbolTypeFromCompilation("Struct"));
        break;
      case TypeCategory.StructArray2:
        symbol = _compilation.CreateArrayTypeSymbol(GetSymbolTypeFromCompilation("Struct"), 2);
        break;
      case TypeCategory.EnumArray1:
        symbol = _compilation.CreateArrayTypeSymbol(GetSymbolTypeFromCompilation("Enumeration"));
        break;
      case TypeCategory.EnumArray2:
        symbol = _compilation.CreateArrayTypeSymbol(GetSymbolTypeFromCompilation("Enumeration"), 2);
        break;

      case TypeCategory.CovariantTypeParameter:
        symbol = GetSymbolTypeFromCompilation("IWithCovariantParameter`1").TypeArguments[0];
        break;
      case TypeCategory.ContravariantTypeParameter:
        symbol = GetSymbolTypeFromCompilation("IWithContravariantParameter`1").TypeArguments[0];
        break;
      case TypeCategory.ConstrainedTypeParameter:
        symbol = GetSymbolTypeFromCompilation("IWithConstrainedParameter`1").TypeArguments[0];
        break;
      case TypeCategory.MethodParameter:
        method = (IMethodSymbol)GetSymbolTypeFromCompilation("IWithContravariantParameter`1").GetMembers("MethodWithTwoGenericParameters").Single();
        symbol = method.TypeArguments[0];
        break;
      case TypeCategory.ConstrainedMethodParameter:
        method = (IMethodSymbol)GetSymbolTypeFromCompilation("IWithConstrainedParameter`1").GetMembers("MethodWithTwoGenericParameters").Single();
        symbol = method.TypeArguments[0];
        break;

      default:
        throw new Exception($"Invalid {nameof(TypeCategory)}: {category}.");
    }

    return symbol switch
    {
      INamedTypeSymbol     => new SymbolNamedType(_runtime, _compilation, (INamedTypeSymbol)symbol),
      IArrayTypeSymbol     => new SymbolArrayType(_runtime, _compilation, (IArrayTypeSymbol)symbol),
      ITypeParameterSymbol => new SymbolTypeParameter(_runtime, _compilation, (ITypeParameterSymbol)symbol),
      _                    => throw new InvalidOperationException()
    };

    INamedTypeSymbol GetSymbolTypeFromCompilation(string name)
    {
      return _compilation.GetTypeByMetadataName($"{Namespace}.{name}") ?? throw new Exception($"Could not find type {name} on compilation.");
    }
  }

  private Type GetType(TypeCategory category)
  {
    Type type;
    Type typeArg1;
    Type typeArg2;

    switch (category)
    {
      case TypeCategory.Object:
        type = typeof(object);
        break;
      case TypeCategory.Int:
        type = typeof(int);
        break;
      case TypeCategory.Short:
        type = typeof(short);
        break;
      case TypeCategory.Long:
        type = typeof(long);
        break;
      case TypeCategory.Float:
        type = typeof(float);
        break;
      case TypeCategory.Double:
        type = typeof(double);
        break;
      case TypeCategory.Decimal:
        type = typeof(decimal);
        break;
      case TypeCategory.Bool:
        type = typeof(bool);
        break;
      case TypeCategory.IntPtr:
        type = typeof(IntPtr);
        break;
      case TypeCategory.String:
        type = typeof(string);
        break;
      case TypeCategory.EnumBase:
        type = typeof(Enum);
        break;

      case TypeCategory.OpenGeneric:
        type = GetTypeFromAssembly("GenericClass`2");
        break;
      case TypeCategory.ClosedGeneric:
        type = GetTypeFromAssembly("GenericClass`2");
        typeArg1 = GetTypeFromAssembly("TypeArgument1");
        typeArg2 = GetTypeFromAssembly("TypeArgument2");
        type = type.MakeGenericType(typeArg1, typeArg2);
        break;
      case TypeCategory.ClosedGenericWithGenericTypeArguments:
        type = GetTypeFromAssembly("GenericClass`2");
        typeArg1 = GetTypeFromAssembly("TypeArgument1");
        typeArg2 = GetTypeFromAssembly("TypeArgument2");
        type = type.MakeGenericType(type.MakeGenericType(typeArg1, typeArg2), typeArg2);
        break;
      case TypeCategory.WithAttributes:
        type = GetTypeFromAssembly("ClassWithAttributes");
        break;
      case TypeCategory.WithStaticConstructor:
        type = GetTypeFromAssembly("ClassWithStaticConstructor");
        break;
      case TypeCategory.Internal:
        type = GetTypeFromAssembly("InternalClass");
        break;
      case TypeCategory.NestedPublic:
        type = GetTypeFromAssembly("ClassWithAllMembers");
        type = type.GetNestedType("NestedPublicClass", BindingFlags.Public)!;
        break;
      case TypeCategory.NestedProtectedInternal:
        type = GetTypeFromAssembly("ClassWithAllMembers");
        type = type.GetNestedType("NestedProtectedInternalClass", BindingFlags.NonPublic)!;
        break;
      case TypeCategory.NestedProtected:
        type = GetTypeFromAssembly("ClassWithAllMembers");
        type = type.GetNestedType("NestedProtectedClass", BindingFlags.NonPublic)!;
        break;
      case TypeCategory.NestedInternal:
        type = GetTypeFromAssembly("ClassWithAllMembers");
        type = type.GetNestedType("NestedInternalClass", BindingFlags.NonPublic)!;
        break;
      case TypeCategory.NestedPrivateProtected:
        type = GetTypeFromAssembly("ClassWithAllMembers");
        type = type.GetNestedType("NestedPrivateProtectedClass", BindingFlags.NonPublic)!;
        break;
      case TypeCategory.NestedPrivate:
        type = GetTypeFromAssembly("ClassWithAllMembers");
        type = type.GetNestedType("NestedPrivateClass", BindingFlags.NonPublic)!;
        break;
      case TypeCategory.WithAllMembers:
        type = GetTypeFromAssembly("ClassWithAllMembers");
        break;
      case TypeCategory.WithAllMembersBase:
        type = GetTypeFromAssembly("ClassWithAllMembersBase");
        break;
      case TypeCategory.WithAllMembersRoot:
        type = GetTypeFromAssembly("ClassWithAllMembersRoot");
        break;
      case TypeCategory.WithDefaultMember:
        type = GetTypeFromAssembly("ClassWithDefaultMember");
        break;
      case TypeCategory.BaseInterface:
        type = GetTypeFromAssembly("IBaseInterface");
        break;
      case TypeCategory.DerivedInterface:
        type = GetTypeFromAssembly("IDerivedInterface");
        break;
      case TypeCategory.Struct:
        type = GetTypeFromAssembly("Struct");
        break;
      case TypeCategory.Enum:
        type = GetTypeFromAssembly("Enumeration");
        break;
      case TypeCategory.Abstract:
        type = GetTypeFromAssembly("AbstractClass");
        break;
      case TypeCategory.Sealed:
        type = GetTypeFromAssembly("SealedClass");
        break;
      case TypeCategory.Serializable:
        type = GetTypeFromAssembly("SerializableClass");
        break;

      case TypeCategory.ObjectArray1:
        type = typeof(object[]);
        break;
      case TypeCategory.ObjectArray2:
        type = typeof(object[,]);
        break;
      case TypeCategory.IntArray1:
        type = typeof(int[]);
        break;
      case TypeCategory.IntArray2:
        type = typeof(int[,]);
        break;
      case TypeCategory.OpenGenericArray1:
        type = GetTypeFromAssembly("GenericClass`2").MakeArrayType();
        break;
      case TypeCategory.OpenGenericArray2:
        type = GetTypeFromAssembly("GenericClass`2").MakeArrayType(2);
        break;
      case TypeCategory.ClosedGenericArray1:
        type = GetTypeFromAssembly("GenericClass`2");
        typeArg1 = GetTypeFromAssembly("TypeArgument1");
        typeArg2 = GetTypeFromAssembly("TypeArgument2");
        type = type.MakeGenericType(typeArg1, typeArg2).MakeArrayType();
        break;
      case TypeCategory.ClosedGenericArray2:
        type = GetTypeFromAssembly("GenericClass`2");
        typeArg1 = GetTypeFromAssembly("TypeArgument1");
        typeArg2 = GetTypeFromAssembly("TypeArgument2");
        type = type.MakeGenericType(typeArg1, typeArg2).MakeArrayType(2);
        break;
      case TypeCategory.ClassArray1:
        type = GetTypeFromAssembly("InternalClass").MakeArrayType();
        break;
      case TypeCategory.ClassArray2:
        type = GetTypeFromAssembly("InternalClass").MakeArrayType(2);
        break;
      case TypeCategory.InterfaceArray1:
        type = GetTypeFromAssembly("IDerivedInterface").MakeArrayType();
        break;
      case TypeCategory.InterfaceArray2:
        type = GetTypeFromAssembly("IDerivedInterface").MakeArrayType(2);
        break;
      case TypeCategory.StructArray1:
        type = GetTypeFromAssembly("Struct").MakeArrayType();
        break;
      case TypeCategory.StructArray2:
        type = GetTypeFromAssembly("Struct").MakeArrayType(2);
        break;
      case TypeCategory.EnumArray1:
        type = GetTypeFromAssembly("Enumeration").MakeArrayType();
        break;
      case TypeCategory.EnumArray2:
        type = GetTypeFromAssembly("Enumeration").MakeArrayType(2);
        break;

      case TypeCategory.CovariantTypeParameter:
        type = GetTypeFromAssembly("IWithCovariantParameter`1").GetGenericArguments()[0];
        break;
      case TypeCategory.ContravariantTypeParameter:
        type = GetTypeFromAssembly("IWithContravariantParameter`1").GetGenericArguments()[0];
        break;
      case TypeCategory.ConstrainedTypeParameter:
        type = GetTypeFromAssembly("IWithConstrainedParameter`1").GetGenericArguments()[0];
        break;
      case TypeCategory.MethodParameter:
        type = GetTypeFromAssembly("IWithContravariantParameter`1").GetMethod("MethodWithTwoGenericParameters")!.GetGenericArguments()[0];
        break;
      case TypeCategory.ConstrainedMethodParameter:
        type = GetTypeFromAssembly("IWithConstrainedParameter`1").GetMethod("MethodWithTwoGenericParameters")!.GetGenericArguments()[0];
        break;

      default:
        throw new Exception($"Invalid {nameof(TypeCategory)}: {category}.");
    }

    return type;

    Type GetTypeFromAssembly(string name)
    {
      return _assembly.GetType($"{Namespace}.{name}") ?? throw new Exception($"Could not find type {name} on compiled assembly.");
    }
  }

  private record struct DataPair(SymbolType Sut, Type Reference);
}

public enum TypeCategory
{
  Object = 0,
  Int,
  Short,
  Long,
  Float,
  Double,
  Decimal,
  Bool,
  IntPtr,
  String,
  EnumBase,

  OpenGeneric = 64,
  ClosedGeneric,
  ClosedGenericWithGenericTypeArguments,
  WithAttributes,
  WithStaticConstructor,
  Internal,
  NestedPublic,
  NestedPrivate,
  NestedProtected,
  NestedInternal,
  NestedPrivateProtected,
  NestedProtectedInternal,
  WithAllMembers,
  WithAllMembersBase,
  WithAllMembersRoot,
  WithDefaultMember,
  BaseInterface,
  DerivedInterface,
  Struct,
  Enum,
  Abstract,
  Sealed,
  Serializable,

  ObjectArray1 = 128,
  IntArray1,
  OpenGenericArray1,
  ClosedGenericArray1,
  ClassArray1,
  InterfaceArray1,
  StructArray1,
  EnumArray1,
  ObjectArray2,
  IntArray2,
  OpenGenericArray2,
  ClosedGenericArray2,
  ClassArray2,
  InterfaceArray2,
  StructArray2,
  EnumArray2,

  CovariantTypeParameter = 192,
  ContravariantTypeParameter,
  ConstrainedTypeParameter,
  MethodParameter,
  ConstrainedMethodParameter
}

public class SpecialTypesDataAttribute : DataAttribute
{
  public TypeCategory[] Except { get; set; } = Array.Empty<TypeCategory>();

  public override IEnumerable<object[]> GetData(MethodInfo testMethod)
  {
    return Enum.GetValues<TypeCategory>()
      .Where(x => (int)x >= 0 && (int)x < 64 && !Except.Contains(x))
      .Select(x => new object[1] { x });
  }
}

public class NamedTypesDataAttribute : DataAttribute
{
  public TypeCategory[] Except { get; set; } = Array.Empty<TypeCategory>();

  public override IEnumerable<object[]> GetData(MethodInfo testMethod)
  {
    return Enum.GetValues<TypeCategory>()
      .Where(x => (int)x >= 64 && (int)x < 128 && !Except.Contains(x))
      .Select(x => new object[1] { x });
  }
}

public class ArrayTypesDataAttribute : DataAttribute
{
  public TypeCategory[] Except { get; set; } = Array.Empty<TypeCategory>();

  public override IEnumerable<object[]> GetData(MethodInfo testMethod)
  {
    return Enum.GetValues<TypeCategory>()
      .Where(x => (int)x >= 128 && (int)x < 192 && !Except.Contains(x))
      .Select(x => new object[1] { x });
  }
}

public class GenericTypesDataAttribute : DataAttribute
{
  public TypeCategory[] Except { get; set; } = Array.Empty<TypeCategory>();

  public override IEnumerable<object[]> GetData(MethodInfo testMethod)
  {
    return Enum.GetValues<TypeCategory>()
      .Where(x => (int)x >= 192 && !Except.Contains(x))
      .Select(x => new object[1] { x });
  }
}

public class GetMembersDataAttribute : DataAttribute
{
  public override IEnumerable<object[]> GetData(MethodInfo testMethod)
  {
    for (int i = 0x00; i < 0x80; i++)
    {
      yield return new object[] { (BindingFlags)i };
    }
  }
}