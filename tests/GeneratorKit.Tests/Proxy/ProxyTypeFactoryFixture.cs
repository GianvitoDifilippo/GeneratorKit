using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Proxy;

public class ProxyTypeFactoryFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName + ".ProxyTypeFactory";

  private const string s_source =
@"

namespace " + Namespace + @"
{
  public interface IInterface<T1, T2>
  {
    void PublicMethod();
    T2 GenericMethodInInterface<T>(T1 arg1);

    T2 InterfaceProperty { get; }
  }

  public abstract class BaseClass<T1, T2>
  {
    public abstract void AbstractMethod();
    public virtual void VirtualMethod() => throw null;

    protected abstract T2 BaseProperty { get; }
  }

  public class Class<T1, T2, T3> : BaseClass<T1, int>, IInterface<T2, string>
  {
    private T1 _genericField;
    private static int _staticField;
    private readonly int _readonlyField;
    private string _initField = ""_initField"";
    private object? _field;

    public Class(int arg1) { }
    private Class(string arg1) { }
    public Class(T1 arg1) { }

    public long Property { get => throw null; set => throw null; }
    public string AutoProperty { get; set; }
    public T1 GenericProperty { get; set; }
    protected override int BaseProperty => throw null;
    public string InterfaceProperty => throw null;

    public string StringMethod(int arg1, string arg2) => throw null;
    public void VoidMethod() => throw null;
    public override void AbstractMethod() => throw null;
    public override void VirtualMethod() => throw null;
    public void PublicMethod() => throw null;
    public T4 GenericMethod<T4, T5>(T1 arg1, int arg2, T5 arg3) => throw null;
    public string GenericMethodInInterface<T>(T2 arg1) => throw null;
  }
}

";

  private readonly Compilation _compilation;
  private readonly Assembly _assembly;
  private readonly FakeGeneratorRuntime _runtime;

  public ProxyTypeFactoryFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName);
    if (!output.IsValid)
    {
      throw new Exception($"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");
    }

    _compilation = output.Compilation;
    _assembly = output.Assembly!;
    _runtime = new FakeGeneratorRuntime(output.Compilation);

    INamedTypeSymbol objectTypeSymbol = output.Compilation.GetSpecialType(SpecialType.System_Object);
    INamedTypeSymbol intTypeSymbol = output.Compilation.GetSpecialType(SpecialType.System_Int32);
    INamedTypeSymbol voidTypeSymbol = output.Compilation.GetSpecialType(SpecialType.System_Void);
    INamedTypeSymbol longTypeSymbol = output.Compilation.GetSpecialType(SpecialType.System_Int64);
    INamedTypeSymbol stringTypeSymbol = output.Compilation.GetSpecialType(SpecialType.System_String);

    INamedTypeSymbol baseClassSymbol = GetSymbolTypeFromCompilation("BaseClass`2");
    INamedTypeSymbol interfaceSymbol = GetSymbolTypeFromCompilation("IInterface`2");
    Type baseClassType = GetTypeFromAssembly("BaseClass`2");
    Type interfaceType = GetTypeFromAssembly("IInterface`2");

    _runtime.AddType(objectTypeSymbol, typeof(object));
    _runtime.AddType(intTypeSymbol, typeof(int));
    _runtime.AddType(voidTypeSymbol, typeof(void));
    _runtime.AddType(longTypeSymbol, typeof(long));
    _runtime.AddType(stringTypeSymbol, typeof(string));
    _runtime.AddType(baseClassSymbol, baseClassType);
    _runtime.AddType(interfaceSymbol, interfaceType);
  }

  internal GeneratorRuntime Runtime => _runtime;

  internal SymbolType GetSymbolType(TypeCategory category)
  {
    INamedTypeSymbol symbol = category switch
    {
      TypeCategory.Class => GetSymbolTypeFromCompilation("Class`3"),
      _                  => throw new InvalidOperationException()
    };

    return new SymbolNamedType(_runtime, symbol);

    
  }

  private INamedTypeSymbol GetSymbolTypeFromCompilation(string name)
  {
    return _compilation.GetTypeByMetadataName($"{Namespace}.{name}") ?? throw new Exception($"Could not find type {name} on compilation.");
  }

  private Type GetTypeFromAssembly(string name)
  {
    return _assembly.GetType($"{Namespace}.{name}") ?? throw new Exception($"Could not find type {name} on assembly.");
  }
}

public enum TypeCategory
{
  Class
}