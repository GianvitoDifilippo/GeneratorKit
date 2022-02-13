using GeneratorKit.Reflection.Context;
using GeneratorKit.TestHelpers;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
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
    int ExplicitMethod();
  }

  public struct Struct { }

  public enum Enumeration
  {
    Enum1,
    Enum2
  }

  internal class InternalClass { }

  [System.CLSCompliant(true)]
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

    public int RootPublicMethod(int arg1, string arg2) => throw null;
    internal int RootInternalMethod(int arg1, string arg2) => throw null;
    private int RootPrivateMethod(int arg1, string arg2) => throw null;
    protected int RootProtectedMethod(int arg1, string arg2) => throw null;
    private protected int RootPrivateProtectedMethod(int arg1, string arg2) => throw null;
    protected internal int RootProtectedInternalMethod(int arg1, string arg2) => throw null;

    public static int RootPublicStaticMethod(int arg1, string arg2) => throw null;
    internal static int RootInternalStaticMethod(int arg1, string arg2) => throw null;
    private static int RootPrivateStaticMethod(int arg1, string arg2) => throw null;
    protected static int RootProtectedStaticMethod(int arg1, string arg2) => throw null;
    private protected static int RootPrivateProtectedStaticMethod(int arg1, string arg2) => throw null;
    protected internal static int RootProtectedInternalStaticMethod(int arg1, string arg2) => throw null;

    public class RootNestedPublicClass { }
    internal class RootNestedInternalClass { }
    private class RootNestedPrivateClass { }
    protected class RootNestedProtectedClass { }
    private protected class RootNestedPrivateProtectedClass { }
    protected internal class RootNestedProtectedInternalClass { }
  }

  [System.Obsolete]
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

    public int BasePublicMethod(int arg1, string arg2) => throw null;
    internal int BaseInternalMethod(int arg1, string arg2) => throw null;
    private int BasePrivateMethod(int arg1, string arg2) => throw null;
    protected int BaseProtectedMethod(int arg1, string arg2) => throw null;
    private protected int BasePrivateProtectedMethod(int arg1, string arg2) => throw null;
    protected internal int BaseProtectedInternalMethod(int arg1, string arg2) => throw null;

    public static int BasePublicStaticMethod(int arg1, string arg2) => throw null;
    internal static int BaseInternalStaticMethod(int arg1, string arg2) => throw null;
    private static int BasePrivateStaticMethod(int arg1, string arg2) => throw null;
    protected static int BaseProtectedStaticMethod(int arg1, string arg2) => throw null;
    private protected static int BasePrivateProtectedStaticMethod(int arg1, string arg2) => throw null;
    protected internal static int BaseProtectedInternalStaticMethod(int arg1, string arg2) => throw null;

    public class BaseNestedPublicClass { }
    internal class BaseNestedInternalClass { }
    private class BaseNestedPrivateClass { }
    protected class BaseNestedProtectedClass { }
    private protected class BaseNestedPrivateProtectedClass { }
    protected internal class BaseNestedProtectedInternalClass { }
  }

  [System.Serializable]
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

    public int PublicMethod(int arg1, string arg2) => throw null;
    internal int InternalMethod(int arg1, string arg2) => throw null;
    private int PrivateMethod(int arg1, string arg2) => throw null;
    protected int ProtectedMethod(int arg1, string arg2) => throw null;
    private protected int PrivateProtectedMethod(int arg1, string arg2) => throw null;
    protected internal int ProtectedInternalMethod(int arg1, string arg2) => throw null;

    public int PublicGenericMethod<T1, T2>(T1 arg1, T2 arg2) => throw null;
    internal int InternalGenericMethod<T1, T2>(T1 arg1, T2 arg2) => throw null;
    private int PrivateGenericMethod<T1, T2>(T1 arg1, T2 arg2) => throw null;
    protected int ProtectedGenericMethod<T1, T2>(T1 arg1, T2 arg2) => throw null;
    private protected int PrivateProtectedGenericMethod<T1, T2>(T1 arg1, T2 arg2) => throw null;
    protected internal int ProtectedInternalGenericMethod<T1, T2>(T1 arg1, T2 arg2) => throw null;

    public static int PublicStaticMethod(int arg1, string arg2) => throw null;
    internal static int InternalStaticMethod(int arg1, string arg2) => throw null;
    private static int PrivateStaticMethod(int arg1, string arg2) => throw null;
    protected static int ProtectedStaticMethod(int arg1, string arg2) => throw null;
    private protected static int PrivateProtectedStaticMethod(int arg1, string arg2) => throw null;
    protected internal static int ProtectedInternalStaticMethod(int arg1, string arg2) => throw null;

    public int this[int i1] => throw null;
    public int this[int i1, int i2] => throw null;
    private int this[int i1, int i2, int i3] => throw null;

    public class NestedPublicClass { }
    internal class NestedInternalClass { }
    private class NestedPrivateClass { }
    protected class NestedProtectedClass { }
    private protected class NestedPrivateProtectedClass { }
    protected internal class NestedProtectedInternalClass { }

    int IDerivedInterface.ExplicitMethod() => throw null;
  }

  [System.Reflection.DefaultMember(""DefaultMember"")]
  public class ClassWithDefaultMember
  {
    public int DefaultMember;
  }

  public interface IWithCovariantParameter<out T1>
  {
    T1 MethodWithTypeParameter();
  }

  public interface IWithContravariantParameter<in T1>
  {
    void MethodWithTwoGenericParameters<T2, T3>(T1 arg1, T2 arg2, T3 arg3);
  }

  public interface IWithConstrainedParameter<T1> where T1 : System.Type, IDerivedInterface
  {
    void MethodWithTwoGenericParameters<T2, T3>(T1 arg1, T2 arg2, T3 arg3) where T2 : class where T3 : System.Enum;
  }

  public class ClassForSymbolParameterType<T1>
  {
    public ref int MethodWithRefReturnParameter() => throw null;
    public void MethodWithRefArgument(ref int arg) => throw null;
    public void MethodWithOutArgument(out int arg) => throw null;
    public void MethodWithInArgument(in int arg) => throw null;
  }

  public class GenericOpenConstructedClass<T1, T2> { }

  public class GenericOpenConstructedDerivedClass<T1> : GenericOpenConstructedClass<T1, int> { }
}

";

  private readonly FakeReflectionContext _context;
  private readonly Compilation _compilation;
  private readonly Assembly _assembly;
  private readonly IReadOnlyDictionary<TypeCategory, DataPair> _data;

  public SymbolTypeFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName);
    Assert.True(output.IsValid, $"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");

    _context = new FakeReflectionContext(output.Compilation);
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
    return Enum.GetValues<TypeCategory>().Select(category => 
    {
      SymbolType symbolType = GetSymbolType(category);
      Type type = GetType(category);
      _context.AddType(symbolType, type);
      return KeyValuePair.Create(category, new DataPair(symbolType, type));
    });
  }

  private SymbolType GetSymbolType(TypeCategory category)
  {
    ITypeSymbol symbol;
    INamedTypeSymbol namedSymbol;
    INamedTypeSymbol typeArg1Symbol;
    INamedTypeSymbol typeArg2Symbol;
    IMethodSymbol method;

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
        symbol = GetTypeSymbolFromCompilation("GenericClass`2");
        break;
      case TypeCategory.ClosedGeneric:
        namedSymbol = GetTypeSymbolFromCompilation("GenericClass`2");
        typeArg1Symbol = GetTypeSymbolFromCompilation("TypeArgument1");
        typeArg2Symbol = GetTypeSymbolFromCompilation("TypeArgument2");
        symbol = namedSymbol.Construct(typeArg1Symbol, typeArg2Symbol);
        break;
      case TypeCategory.ClosedGenericWithGenericTypeArguments:
        namedSymbol = GetTypeSymbolFromCompilation("GenericClass`2");
        typeArg1Symbol = GetTypeSymbolFromCompilation("TypeArgument1");
        typeArg2Symbol = GetTypeSymbolFromCompilation("TypeArgument2");
        symbol = namedSymbol.Construct(namedSymbol.Construct(typeArg1Symbol, typeArg2Symbol), typeArg2Symbol);
        break;
      case TypeCategory.GenericOpenConstructed:
        symbol = GetTypeSymbolFromCompilation("GenericOpenConstructedDerivedClass`1").BaseType!;
        break;
      case TypeCategory.WithAttributes:
        symbol = GetTypeSymbolFromCompilation("ClassWithAttributes");
        break;
      case TypeCategory.WithStaticConstructor:
        symbol = GetTypeSymbolFromCompilation("ClassWithStaticConstructor");
        break;
      case TypeCategory.Internal:
        symbol = GetTypeSymbolFromCompilation("InternalClass");
        break;
      case TypeCategory.NestedPublic:
        symbol = GetTypeSymbolFromCompilation("ClassWithAllMembers");
        symbol = symbol.GetTypeMembers("NestedPublicClass")[0];
        break;
      case TypeCategory.NestedProtectedInternal:
        symbol = GetTypeSymbolFromCompilation("ClassWithAllMembers");
        symbol = symbol.GetTypeMembers("NestedProtectedInternalClass")[0];
        break;
      case TypeCategory.NestedProtected:
        symbol = GetTypeSymbolFromCompilation("ClassWithAllMembers");
        symbol = symbol.GetTypeMembers("NestedProtectedClass")[0];
        break;
      case TypeCategory.NestedInternal:
        symbol = GetTypeSymbolFromCompilation("ClassWithAllMembers");
        symbol = symbol.GetTypeMembers("NestedInternalClass")[0];
        break;
      case TypeCategory.NestedPrivateProtected:
        symbol = GetTypeSymbolFromCompilation("ClassWithAllMembers");
        symbol = symbol.GetTypeMembers("NestedPrivateProtectedClass")[0];
        break;
      case TypeCategory.NestedPrivate:
        symbol = GetTypeSymbolFromCompilation("ClassWithAllMembers");
        symbol = symbol.GetTypeMembers("NestedPrivateClass")[0];
        break;
      case TypeCategory.WithAllMembers:
        symbol = GetTypeSymbolFromCompilation("ClassWithAllMembers");
        break;
      case TypeCategory.WithAllMembersBase:
        symbol = GetTypeSymbolFromCompilation("ClassWithAllMembersBase");
        break;
      case TypeCategory.WithAllMembersRoot:
        symbol = GetTypeSymbolFromCompilation("ClassWithAllMembersRoot");
        break;
      case TypeCategory.WithDefaultMember:
        symbol = GetTypeSymbolFromCompilation("ClassWithDefaultMember");
        break;
      case TypeCategory.BaseInterface:
        symbol = GetTypeSymbolFromCompilation("IBaseInterface");
        break;
      case TypeCategory.DerivedInterface:
        symbol = GetTypeSymbolFromCompilation("IDerivedInterface");
        break;
      case TypeCategory.Struct:
        symbol = GetTypeSymbolFromCompilation("Struct");
        break;
      case TypeCategory.Enum:
        symbol = GetTypeSymbolFromCompilation("Enumeration");
        break;
      case TypeCategory.Abstract:
        symbol = GetTypeSymbolFromCompilation("AbstractClass");
        break;
      case TypeCategory.Sealed:
        symbol = GetTypeSymbolFromCompilation("SealedClass");
        break;

      case TypeCategory.ObjectSZArray:
        symbol = _compilation.CreateArrayTypeSymbol(_compilation.GetSpecialType(SpecialType.System_Object));
        break;
      case TypeCategory.ObjectMDArray2:
        symbol = _compilation.CreateArrayTypeSymbol(_compilation.GetSpecialType(SpecialType.System_Object), 2);
        break;
      case TypeCategory.IntSZArray:
        symbol = _compilation.CreateArrayTypeSymbol(_compilation.GetSpecialType(SpecialType.System_Int32));
        break;
      case TypeCategory.IntMDArray2:
        symbol = _compilation.CreateArrayTypeSymbol(_compilation.GetSpecialType(SpecialType.System_Int32), 2);
        break;
      case TypeCategory.OpenGenericSZArray:
        symbol = _compilation.CreateArrayTypeSymbol(GetTypeSymbolFromCompilation("GenericClass`2"));
        break;
      case TypeCategory.OpenGenericMDArray2:
        symbol = _compilation.CreateArrayTypeSymbol(GetTypeSymbolFromCompilation("GenericClass`2"), 2);
        break;
      case TypeCategory.ClosedGenericSZArray:
        namedSymbol = GetTypeSymbolFromCompilation("GenericClass`2");
        typeArg1Symbol = GetTypeSymbolFromCompilation("TypeArgument1");
        typeArg2Symbol = GetTypeSymbolFromCompilation("TypeArgument2");
        symbol = _compilation.CreateArrayTypeSymbol(namedSymbol.Construct(typeArg1Symbol, typeArg2Symbol));
        break;
      case TypeCategory.ClosedGenericMDArray2:
        namedSymbol = GetTypeSymbolFromCompilation("GenericClass`2");
        typeArg1Symbol = GetTypeSymbolFromCompilation("TypeArgument1");
        typeArg2Symbol = GetTypeSymbolFromCompilation("TypeArgument2");
        symbol = _compilation.CreateArrayTypeSymbol(namedSymbol.Construct(typeArg1Symbol, typeArg2Symbol), 2);
        break;
      case TypeCategory.ClassSZArray:
        symbol = _compilation.CreateArrayTypeSymbol(GetTypeSymbolFromCompilation("InternalClass"));
        break;
      case TypeCategory.ClassMDArray2:
        symbol = _compilation.CreateArrayTypeSymbol(GetTypeSymbolFromCompilation("InternalClass"), 2);
        break;
      case TypeCategory.InterfaceSZArray:
        symbol = _compilation.CreateArrayTypeSymbol(GetTypeSymbolFromCompilation("IDerivedInterface"));
        break;
      case TypeCategory.InterfaceMDArray2:
        symbol = _compilation.CreateArrayTypeSymbol(GetTypeSymbolFromCompilation("IDerivedInterface"), 2);
        break;
      case TypeCategory.StructSZArray:
        symbol = _compilation.CreateArrayTypeSymbol(GetTypeSymbolFromCompilation("Struct"));
        break;
      case TypeCategory.StructMDArray2:
        symbol = _compilation.CreateArrayTypeSymbol(GetTypeSymbolFromCompilation("Struct"), 2);
        break;
      case TypeCategory.EnumSZArray:
        symbol = _compilation.CreateArrayTypeSymbol(GetTypeSymbolFromCompilation("Enumeration"));
        break;
      case TypeCategory.EnumMDArray2:
        symbol = _compilation.CreateArrayTypeSymbol(GetTypeSymbolFromCompilation("Enumeration"), 2);
        break;

      case TypeCategory.CovariantTypeParameter:
        symbol = GetTypeSymbolFromCompilation("IWithCovariantParameter`1").TypeArguments[0];
        break;
      case TypeCategory.ContravariantTypeParameter:
        symbol = GetTypeSymbolFromCompilation("IWithContravariantParameter`1").TypeArguments[0];
        break;
      case TypeCategory.ConstrainedTypeParameter:
        symbol = GetTypeSymbolFromCompilation("IWithConstrainedParameter`1").TypeArguments[0];
        break;
      case TypeCategory.FirstMethodParameter:
        method = (IMethodSymbol)GetTypeSymbolFromCompilation("IWithContravariantParameter`1").GetMembers("MethodWithTwoGenericParameters").Single();
        symbol = method.TypeArguments[0];
        break;
      case TypeCategory.FirstConstrainedMethodParameter:
        method = (IMethodSymbol)GetTypeSymbolFromCompilation("IWithConstrainedParameter`1").GetMembers("MethodWithTwoGenericParameters").Single();
        symbol = method.TypeArguments[0];
        break;
      case TypeCategory.SecondMethodParameter:
        method = (IMethodSymbol)GetTypeSymbolFromCompilation("IWithContravariantParameter`1").GetMembers("MethodWithTwoGenericParameters").Single();
        symbol = method.TypeArguments[1];
        break;
      case TypeCategory.SecondConstrainedMethodParameter:
        method = (IMethodSymbol)GetTypeSymbolFromCompilation("IWithConstrainedParameter`1").GetMembers("MethodWithTwoGenericParameters").Single();
        symbol = method.TypeArguments[1];
        break;

      default:
        throw Errors.Unreacheable;
    }

    return symbol switch
    {
      INamedTypeSymbol     => new SymbolNamedType(_context, (INamedTypeSymbol)symbol),
      IArrayTypeSymbol     => new SymbolArrayType(_context, (IArrayTypeSymbol)symbol),
      ITypeParameterSymbol => new SymbolTypeParameter(_context, (ITypeParameterSymbol)symbol),
      _                    => throw new InvalidOperationException()
    };

    INamedTypeSymbol GetTypeSymbolFromCompilation(string name)
    {
      INamedTypeSymbol? result = _compilation.GetTypeByMetadataName($"{Namespace}.{name}");
      Assert.NotNull(result);
      return result!;
    }
  }

  private Type GetType(TypeCategory category)
  {
    Type type;

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
        type = GetType(TypeCategory.OpenGeneric).MakeGenericType(
          GetTypeFromAssembly("TypeArgument1"),
          GetTypeFromAssembly("TypeArgument2"));
        break;
      case TypeCategory.ClosedGenericWithGenericTypeArguments:
        type = GetType(TypeCategory.OpenGeneric).MakeGenericType(
          GetType(TypeCategory.ClosedGeneric),
          GetTypeFromAssembly("TypeArgument2"));
        break;
      case TypeCategory.GenericOpenConstructed:
        type = GetTypeFromAssembly("GenericOpenConstructedDerivedClass`1").BaseType!;
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
        type = GetType(TypeCategory.WithAllMembers)
          .GetNestedType("NestedProtectedInternalClass", BindingFlags.NonPublic)!;
        break;
      case TypeCategory.NestedProtected:
        type = GetType(TypeCategory.WithAllMembers)
          .GetNestedType("NestedProtectedClass", BindingFlags.NonPublic)!;
        break;
      case TypeCategory.NestedInternal:
        type = GetType(TypeCategory.WithAllMembers)
          .GetNestedType("NestedInternalClass", BindingFlags.NonPublic)!;
        break;
      case TypeCategory.NestedPrivateProtected:
        type = GetType(TypeCategory.WithAllMembers)
          .GetNestedType("NestedPrivateProtectedClass", BindingFlags.NonPublic)!;
        break;
      case TypeCategory.NestedPrivate:
        type = GetType(TypeCategory.WithAllMembers)
          .GetNestedType("NestedPrivateClass", BindingFlags.NonPublic)!;
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

      case TypeCategory.ObjectSZArray:
        type = GetType(TypeCategory.Object).MakeArrayType();
        break;
      case TypeCategory.ObjectMDArray2:
        type = GetType(TypeCategory.Object).MakeArrayType(2);
        break;
      case TypeCategory.IntSZArray:
        type = GetType(TypeCategory.Int).MakeArrayType();
        break;
      case TypeCategory.IntMDArray2:
        type = GetType(TypeCategory.Int).MakeArrayType(2);
        break;
      case TypeCategory.OpenGenericSZArray:
        type = GetType(TypeCategory.OpenGeneric).MakeArrayType();
        break;
      case TypeCategory.OpenGenericMDArray2:
        type = GetType(TypeCategory.OpenGeneric).MakeArrayType(2);
        break;
      case TypeCategory.ClosedGenericSZArray:
        type = GetType(TypeCategory.ClosedGeneric).MakeArrayType();
        break;
      case TypeCategory.ClosedGenericMDArray2:
        type = GetType(TypeCategory.ClosedGeneric).MakeArrayType(2);
        break;
      case TypeCategory.ClassSZArray:
        type = GetType(TypeCategory.Internal).MakeArrayType();
        break;
      case TypeCategory.ClassMDArray2:
        type = GetType(TypeCategory.Internal).MakeArrayType(2);
        break;
      case TypeCategory.InterfaceSZArray:
        type = GetType(TypeCategory.DerivedInterface).MakeArrayType();
        break;
      case TypeCategory.InterfaceMDArray2:
        type = GetType(TypeCategory.DerivedInterface).MakeArrayType(2);
        break;
      case TypeCategory.StructSZArray:
        type = GetType(TypeCategory.Struct).MakeArrayType();
        break;
      case TypeCategory.StructMDArray2:
        type = GetType(TypeCategory.Struct).MakeArrayType(2);
        break;
      case TypeCategory.EnumSZArray:
        type = GetType(TypeCategory.Enum).MakeArrayType();
        break;
      case TypeCategory.EnumMDArray2:
        type = GetType(TypeCategory.Enum).MakeArrayType(2);
        break;

      case TypeCategory.CovariantTypeParameter:
        type = GetTypeFromAssembly("IWithCovariantParameter`1")
          .GetGenericArguments()[0];
        break;
      case TypeCategory.ContravariantTypeParameter:
        type = GetTypeFromAssembly("IWithContravariantParameter`1")
          .GetGenericArguments()[0];
        break;
      case TypeCategory.ConstrainedTypeParameter:
        type = GetTypeFromAssembly("IWithConstrainedParameter`1")
          .GetGenericArguments()[0];
        break;
      case TypeCategory.FirstMethodParameter:
        type = GetTypeFromAssembly("IWithContravariantParameter`1")
          .GetMethod("MethodWithTwoGenericParameters")!
          .GetGenericArguments()[0];
        break;
      case TypeCategory.FirstConstrainedMethodParameter:
        type = GetTypeFromAssembly("IWithConstrainedParameter`1")
          .GetMethod("MethodWithTwoGenericParameters")!
          .GetGenericArguments()[0];
        break;
      case TypeCategory.SecondMethodParameter:
        type = GetTypeFromAssembly("IWithContravariantParameter`1")
          .GetMethod("MethodWithTwoGenericParameters")!
          .GetGenericArguments()[1];
        break;
      case TypeCategory.SecondConstrainedMethodParameter:
        type = GetTypeFromAssembly("IWithConstrainedParameter`1")
          .GetMethod("MethodWithTwoGenericParameters")!
          .GetGenericArguments()[1];
        break;

      default:
        throw Errors.Unreacheable;
    }

    return type;

    Type GetTypeFromAssembly(string name)
    {
      Type? result = _assembly.GetType($"{Namespace}.{name}");
      Assert.NotNull(result);
      return result!;
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
  GenericOpenConstructed,
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

  ObjectSZArray = 128,
  IntSZArray,
  OpenGenericSZArray,
  ClosedGenericSZArray,
  ClassSZArray,
  InterfaceSZArray,
  StructSZArray,
  EnumSZArray,
  ObjectMDArray2,
  IntMDArray2,
  OpenGenericMDArray2,
  ClosedGenericMDArray2,
  ClassMDArray2,
  InterfaceMDArray2,
  StructMDArray2,
  EnumMDArray2,

  CovariantTypeParameter = 192,
  ContravariantTypeParameter,
  ConstrainedTypeParameter,
  FirstMethodParameter,
  FirstConstrainedMethodParameter,
  SecondMethodParameter,
  SecondConstrainedMethodParameter,
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

public class SpecialTypesDataExceptAttribute : SpecialTypesDataAttribute
{
  public SpecialTypesDataExceptAttribute(params TypeCategory[] except)
  {
    Except = except;
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

public class NamedTypesDataExceptAttribute : NamedTypesDataAttribute
{
  public NamedTypesDataExceptAttribute(params TypeCategory[] except)
  {
    Except = except;
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

public class ArrayTypesDataExceptAttribute : ArrayTypesDataAttribute
{
  public ArrayTypesDataExceptAttribute(params TypeCategory[] except)
  {
    Except = except;
  }
}

public class TypeParametersDataAttribute : DataAttribute
{
  public TypeCategory[] Except { get; set; } = Array.Empty<TypeCategory>();

  public override IEnumerable<object[]> GetData(MethodInfo testMethod)
  {
    return Enum.GetValues<TypeCategory>()
      .Where(x => (int)x >= 192 && (int)x < 256 && !Except.Contains(x))
      .Select(x => new object[1] { x });
  }
}

public class TypeParametersDataExceptAttribute : TypeParametersDataAttribute
{
  public TypeParametersDataExceptAttribute(params TypeCategory[] except)
  {
    Except = except;
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