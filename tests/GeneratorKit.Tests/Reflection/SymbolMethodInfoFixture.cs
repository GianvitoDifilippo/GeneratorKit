using GeneratorKit.TestHelpers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace GeneratorKit.Reflection;

public class SymbolMethodInfoFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName + ".SymbolMethodInfo";

  private const BindingFlags s_allMethods = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

  private const string s_source =
@"

namespace " + Namespace + @"
{
  public interface IInterface
  {
    void InterfaceMethod();
    int ExplicitMethod();
  }

  public abstract class BaseClass
  {
    public void BaseMethod() => throw null;
    public abstract void AbstractMethod();
    public abstract void AbstractThenSealedMethod();
    public virtual void VirtualMethodNotOverridden() => throw null;
    public virtual void VirtualMethodOverridden() => throw null;
    public virtual T1 GenericMethodNotOverridden<T1, T2>() => throw null;
    public virtual T1 GenericMethodOverridden<T1, T2>() => throw null;
  }

  public class DerivedClass : BaseClass, IInterface
  {
    public int PublicMethod(int arg1, string arg2) => throw null;
    internal void InternalMethod() => throw null;
    private int PrivateMethod(int arg1) => throw null;
    protected int ProtectedMethod(int arg1, string arg2) => throw null;
    private protected int PrivateProtectedMethod(int arg1, string arg2) => throw null;
    protected internal int ProtectedInternalMethod(int arg1, string arg2) => throw null;

    public T2 GenericMethod<T1, T2>(T1 arg1, T2 arg2, int arg3) => throw null;

    public static int PublicStaticMethod(int arg1, string arg2) => throw null;
    internal static int InternalStaticMethod(int arg1, string arg2) => throw null;
    private static int PrivateStaticMethod(int arg1, string arg2) => throw null;
    protected static int ProtectedStaticMethod(int arg1, string arg2) => throw null;
    private protected static int PrivateProtectedStaticMethod(int arg1, string arg2) => throw null;
    protected internal static int ProtectedInternalStaticMethod(int arg1, string arg2) => throw null;

    [System.Obsolete]
    public void MethodWithAttribute() => throw null;

    public override void AbstractMethod() => throw null;
    public override sealed void AbstractThenSealedMethod() => throw null;
    public override void VirtualMethodOverridden() => throw null;
    public override T1 GenericMethodOverridden<T1, T2>() => throw null;

    public void InterfaceMethod() => throw null;
    int IInterface.ExplicitMethod() => throw null;
  }
}

";

  private readonly FakeGeneratorRuntime _runtime;
  private readonly Type _baseType;
  private readonly Type _derivedType;
  private readonly INamedTypeSymbol _baseSymbol;
  private readonly INamedTypeSymbol _derivedSymbol;
  private readonly INamedTypeSymbol _intSymbol;
  private readonly INamedTypeSymbol _stringSymbol;

  public SymbolMethodInfoFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName);
    if (!output.IsValid)
    {
      throw new Exception($"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");
    }

    _runtime = new FakeGeneratorRuntime(output.Compilation);

    _baseType = output.Assembly!.GetType(Namespace + ".BaseClass")!;
    _derivedType = output.Assembly!.GetType(Namespace + ".DerivedClass")!;

    _baseSymbol = output.Compilation.GetTypeByMetadataName(Namespace + ".BaseClass")!;
    _derivedSymbol = output.Compilation.GetTypeByMetadataName(Namespace + ".DerivedClass")!;

    _intSymbol = output.Compilation.GetSpecialType(SpecialType.System_Int32);
    _stringSymbol = output.Compilation.GetSpecialType(SpecialType.System_String);

    _runtime.AddType(_baseSymbol, _baseType);
    _runtime.AddType(_derivedSymbol, _derivedType);
    _runtime.AddType(_intSymbol, typeof(int));
    _runtime.AddType(_stringSymbol, typeof(string));

    IntSymbolType = new SymbolNamedType(_runtime, _intSymbol);
    StringSymbolType = new SymbolNamedType(_runtime, _stringSymbol);
  }

  internal SymbolType IntSymbolType { get; }
  internal SymbolType StringSymbolType { get; }

  public MethodInfo GetReference(MethodCategory category)
  {
    return category switch
    {
      MethodCategory.PublicMethod
        => GetMethodFromType(_derivedType, "PublicMethod"),

      MethodCategory.InternalMethod
        => GetMethodFromType(_derivedType, "InternalMethod"),

      MethodCategory.PrivateMethod
        => GetMethodFromType(_derivedType, "PrivateMethod"),

      MethodCategory.ProtectedMethod
        => GetMethodFromType(_derivedType, "ProtectedMethod"),

      MethodCategory.PrivateProtectedMethod
        => GetMethodFromType(_derivedType, "PrivateProtectedMethod"),

      MethodCategory.ProtectedInternalMethod
        => GetMethodFromType(_derivedType, "ProtectedInternalMethod"),

      MethodCategory.GenericMethod
        => GetMethodFromType(_derivedType, "GenericMethod"),

      MethodCategory.ConstructedGenericMethod
        => GetMethodFromType(_derivedType, "GenericMethod").MakeGenericMethod(typeof(int), typeof(string)),

      MethodCategory.PublicStaticMethod
        => GetMethodFromType(_derivedType, "PublicStaticMethod"),

      MethodCategory.InternalStaticMethod
        => GetMethodFromType(_derivedType, "InternalStaticMethod"),

      MethodCategory.PrivateStaticMethod
        => GetMethodFromType(_derivedType, "PrivateStaticMethod"),

      MethodCategory.ProtectedStaticMethod
        => GetMethodFromType(_derivedType, "ProtectedStaticMethod"),

      MethodCategory.PrivateProtectedStaticMethod
        => GetMethodFromType(_derivedType, "PrivateProtectedStaticMethod"),

      MethodCategory.ProtectedInternalStaticMethod
        => GetMethodFromType(_derivedType, "ProtectedInternalStaticMethod"),

      MethodCategory.MethodWithAttribute
        => GetMethodFromType(_derivedType, "MethodWithAttribute"),

      MethodCategory.BaseMethod
        => GetMethodFromType(_baseType, "BaseMethod"),

      MethodCategory.BaseMethodReflectedFromDerived
        => GetMethodFromType(_derivedType, "BaseMethod"),

      MethodCategory.AbstractMethod
        => GetMethodFromType(_baseType, "AbstractMethod"),

      MethodCategory.AbstractMethodReflectedFromDerived
        => GetMethodFromType(_derivedType, "AbstractMethod"),

      MethodCategory.AbstractThenSealedMethod
        => GetMethodFromType(_baseType, "AbstractThenSealedMethod"),

      MethodCategory.AbstractThenSealedMethodReflectedFromDerived
        => GetMethodFromType(_derivedType, "AbstractThenSealedMethod"),

      MethodCategory.VirtualMethodNotOverridden
        => GetMethodFromType(_baseType, "VirtualMethodNotOverridden"),

      MethodCategory.VirtualMethodNotOverriddenReflectedFromDerived
        => GetMethodFromType(_derivedType, "VirtualMethodNotOverridden"),

      MethodCategory.VirtualMethodOverridden
        => GetMethodFromType(_baseType, "VirtualMethodOverridden"),

      MethodCategory.VirtualMethodOverriddenReflectedFromDerived
        => GetMethodFromType(_derivedType, "VirtualMethodOverridden"),

      MethodCategory.GenericMethodNotOverridden
        => GetMethodFromType(_baseType, "GenericMethodNotOverridden"),

      MethodCategory.GenericMethodNotOverriddenReflectedFromDerived
        => GetMethodFromType(_derivedType, "GenericMethodNotOverridden"),

      MethodCategory.GenericMethodOverridden
        => GetMethodFromType(_baseType, "GenericMethodOverridden"),

      MethodCategory.GenericMethodOverriddenReflectedFromDerived
        => GetMethodFromType(_derivedType, "GenericMethodOverridden"),

      MethodCategory.ConstructedGenericMethodNotOverridden
        => GetMethodFromType(_baseType, "GenericMethodNotOverridden").MakeGenericMethod(typeof(int), typeof(string)),

      MethodCategory.ConstructedGenericMethodNotOverriddenReflectedFromDerived
        => GetMethodFromType(_derivedType, "GenericMethodNotOverridden").MakeGenericMethod(typeof(int), typeof(string)),

      MethodCategory.ConstructedGenericMethodOverridden
        => GetMethodFromType(_baseType, "GenericMethodOverridden").MakeGenericMethod(typeof(int), typeof(string)),

      MethodCategory.ConstructedGenericMethodOverriddenReflectedFromDerived
        => GetMethodFromType(_derivedType, "GenericMethodOverridden").MakeGenericMethod(typeof(int), typeof(string)),

      MethodCategory.ExplicitMethod
        => GetMethodFromType(_derivedType, "GeneratorKit.Reflection.Tests.SymbolMethodInfo.IInterface.ExplicitMethod"),

      MethodCategory.InterfaceMethod
        => GetMethodFromType(_derivedType, "InterfaceMethod"),

      _ => throw new InvalidOperationException()
    };

    static MethodInfo GetMethodFromType(Type type, string name)
    {
      return type.GetMethod(name, s_allMethods) ?? throw new Exception($"Could not find method {name} on type {type.Name}.");
    }
  }

  internal SymbolMethodInfo GetDelegator(MethodCategory category)
  {
    IMethodSymbol symbol = category switch
    {
      MethodCategory.PublicMethod
        => GetMethodFromType(_derivedSymbol, "PublicMethod"),

      MethodCategory.InternalMethod
        => GetMethodFromType(_derivedSymbol, "InternalMethod"),

      MethodCategory.PrivateMethod
        => GetMethodFromType(_derivedSymbol, "PrivateMethod"),

      MethodCategory.ProtectedMethod
        => GetMethodFromType(_derivedSymbol, "ProtectedMethod"),

      MethodCategory.PrivateProtectedMethod
        => GetMethodFromType(_derivedSymbol, "PrivateProtectedMethod"),

      MethodCategory.ProtectedInternalMethod
        => GetMethodFromType(_derivedSymbol, "ProtectedInternalMethod"),

      MethodCategory.GenericMethod or
      MethodCategory.ConstructedGenericMethod
        => GetMethodFromType(_derivedSymbol, "GenericMethod"),

      MethodCategory.PublicStaticMethod
        => GetMethodFromType(_derivedSymbol, "PublicStaticMethod"),

      MethodCategory.InternalStaticMethod
        => GetMethodFromType(_derivedSymbol, "InternalStaticMethod"),

      MethodCategory.PrivateStaticMethod
        => GetMethodFromType(_derivedSymbol, "PrivateStaticMethod"),

      MethodCategory.ProtectedStaticMethod
        => GetMethodFromType(_derivedSymbol, "ProtectedStaticMethod"),

      MethodCategory.PrivateProtectedStaticMethod
        => GetMethodFromType(_derivedSymbol, "PrivateProtectedStaticMethod"),

      MethodCategory.ProtectedInternalStaticMethod
        => GetMethodFromType(_derivedSymbol, "ProtectedInternalStaticMethod"),

      MethodCategory.MethodWithAttribute
        => GetMethodFromType(_derivedSymbol, "MethodWithAttribute"),

      MethodCategory.BaseMethod or
      MethodCategory.BaseMethodReflectedFromDerived
        => GetMethodFromType(_baseSymbol, "BaseMethod"),

      MethodCategory.AbstractMethod
        => GetMethodFromType(_baseSymbol, "AbstractMethod"),

      MethodCategory.AbstractMethodReflectedFromDerived
        => GetMethodFromType(_derivedSymbol, "AbstractMethod"),

      MethodCategory.AbstractThenSealedMethod
        => GetMethodFromType(_baseSymbol, "AbstractThenSealedMethod"),

      MethodCategory.AbstractThenSealedMethodReflectedFromDerived
        => GetMethodFromType(_derivedSymbol, "AbstractThenSealedMethod"),

      MethodCategory.VirtualMethodNotOverridden or
      MethodCategory.VirtualMethodNotOverriddenReflectedFromDerived
        => GetMethodFromType(_baseSymbol, "VirtualMethodNotOverridden"),

      MethodCategory.VirtualMethodOverridden
        => GetMethodFromType(_baseSymbol, "VirtualMethodOverridden"),

      MethodCategory.VirtualMethodOverriddenReflectedFromDerived
        => GetMethodFromType(_derivedSymbol, "VirtualMethodOverridden"),

      MethodCategory.GenericMethodNotOverridden or
      MethodCategory.ConstructedGenericMethodNotOverridden
        => GetMethodFromType(_baseSymbol, "GenericMethodNotOverridden"),

      MethodCategory.GenericMethodNotOverriddenReflectedFromDerived or
      MethodCategory.ConstructedGenericMethodNotOverriddenReflectedFromDerived
        => GetMethodFromType(_baseSymbol, "GenericMethodNotOverridden"),

      MethodCategory.GenericMethodOverridden or
      MethodCategory.ConstructedGenericMethodOverridden
        => GetMethodFromType(_baseSymbol, "GenericMethodOverridden"),

      MethodCategory.GenericMethodOverriddenReflectedFromDerived or
      MethodCategory.ConstructedGenericMethodOverriddenReflectedFromDerived
        => GetMethodFromType(_derivedSymbol, "GenericMethodOverridden"),

      MethodCategory.ExplicitMethod
        => GetMethodFromType(_derivedSymbol, "GeneratorKit.Reflection.Tests.SymbolMethodInfo.IInterface.ExplicitMethod"),

      MethodCategory.InterfaceMethod
        => GetMethodFromType(_derivedSymbol, "InterfaceMethod"),

      _ => throw new InvalidOperationException()
    };

    SymbolMethodInfo result = NeedsReflectedType(category)
      ? new SymbolMethodInfo(_runtime, symbol, _runtime.CreateTypeDelegator(_derivedSymbol))
      : new SymbolMethodInfo(_runtime, symbol);

    return NeedsConstruction(category)
      ? result.MakeGenericMethod(IntSymbolType, StringSymbolType)
      : result;

    static IMethodSymbol GetMethodFromType(INamedTypeSymbol symbol, string name)
    {
      return (IMethodSymbol)symbol.GetMembers().Single(x => x.Kind is SymbolKind.Method && x.Name == name);
    }

    static bool NeedsReflectedType(MethodCategory category)
    {
      return category is
        MethodCategory.BaseMethodReflectedFromDerived or
        MethodCategory.VirtualMethodNotOverriddenReflectedFromDerived or
        MethodCategory.GenericMethodNotOverriddenReflectedFromDerived or
        MethodCategory.ConstructedGenericMethodNotOverriddenReflectedFromDerived;
    }

    static bool NeedsConstruction(MethodCategory category)
    {
      return category is
        MethodCategory.ConstructedGenericMethod or
        MethodCategory.ConstructedGenericMethodNotOverridden or
        MethodCategory.ConstructedGenericMethodNotOverriddenReflectedFromDerived or
        MethodCategory.ConstructedGenericMethodOverridden or
        MethodCategory.ConstructedGenericMethodOverriddenReflectedFromDerived;
    }
  }
}

public class MethodsDataAttribute : DataAttribute
{
  public MethodCategory[] Except { get; set; } = Array.Empty<MethodCategory>();

  public override IEnumerable<object[]> GetData(MethodInfo testMethod)
  {
    return Enum.GetValues<MethodCategory>().Where(x => !Except.Contains(x)).Select(x => new object[1] { x });
  }
}

public class MethodsDataExceptAttribute : MethodsDataAttribute
{
  public MethodsDataExceptAttribute(params MethodCategory[] except)
  {
    Except = except;
  }
}

public enum MethodCategory
{
  PublicMethod,
  InternalMethod,
  PrivateMethod,
  ProtectedMethod,
  PrivateProtectedMethod,
  ProtectedInternalMethod,
  GenericMethod,
  ConstructedGenericMethod,
  PublicStaticMethod,
  InternalStaticMethod,
  PrivateStaticMethod,
  ProtectedStaticMethod,
  PrivateProtectedStaticMethod,
  ProtectedInternalStaticMethod,
  MethodWithAttribute,
  BaseMethod,
  BaseMethodReflectedFromDerived,
  AbstractMethod,
  AbstractMethodReflectedFromDerived,
  AbstractThenSealedMethod,
  AbstractThenSealedMethodReflectedFromDerived,
  VirtualMethodNotOverridden,
  VirtualMethodNotOverriddenReflectedFromDerived,
  VirtualMethodOverridden,
  VirtualMethodOverriddenReflectedFromDerived,
  GenericMethodNotOverridden,
  GenericMethodNotOverriddenReflectedFromDerived,
  GenericMethodOverridden,
  GenericMethodOverriddenReflectedFromDerived,
  ConstructedGenericMethodNotOverridden,
  ConstructedGenericMethodNotOverriddenReflectedFromDerived,
  ConstructedGenericMethodOverridden,
  ConstructedGenericMethodOverriddenReflectedFromDerived,
  ExplicitMethod,
  InterfaceMethod
}