using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using Microsoft.CodeAnalysis;
using System;

namespace GeneratorKit.Proxy;

public class ProxyTypeFactoryFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName + ".ProxyTypeFactory";

  private const string s_source =
@"

namespace " + Namespace + @"
{
  public class Class
  {
    private static int _staticField;
    private readonly int _readonlyField;
    private string _initField = ""_initField"";
    private object? _field;

    public long Property { get => throw null; set => throw null; }
    public string AutoProperty { get; set; }

    public string StringMethod(int arg1, string arg2) => throw null;
    public void VoidMethod() => throw null;
  }
}

";

  private readonly Compilation _compilation;
  private readonly FakeGeneratorRuntime _runtime;

  public ProxyTypeFactoryFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName);
    if (!output.IsValid)
    {
      throw new Exception($"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");
    }

    _compilation = output.Compilation;
    _runtime = new FakeGeneratorRuntime(output.Compilation);

    INamedTypeSymbol objectTypeSymbol = output.Compilation.GetSpecialType(SpecialType.System_Object);
    INamedTypeSymbol intTypeSymbol = output.Compilation.GetSpecialType(SpecialType.System_Int32);
    INamedTypeSymbol voidTypeSymbol = output.Compilation.GetSpecialType(SpecialType.System_Void);
    INamedTypeSymbol longTypeSymbol = output.Compilation.GetSpecialType(SpecialType.System_Int64);
    INamedTypeSymbol stringTypeSymbol = output.Compilation.GetSpecialType(SpecialType.System_String);
    _runtime.AddType(objectTypeSymbol, typeof(object));
    _runtime.AddType(intTypeSymbol, typeof(int));
    _runtime.AddType(voidTypeSymbol, typeof(void));
    _runtime.AddType(longTypeSymbol, typeof(long));
    _runtime.AddType(stringTypeSymbol, typeof(string));
  }

  internal GeneratorRuntime Runtime => _runtime;

  internal SymbolType GetSymbolType(TypeCategory category)
  {
    INamedTypeSymbol symbol = category switch
    {
      TypeCategory.Class => GetSymbolTypeFromCompilation("Class"),
      _                  => throw new InvalidOperationException()
    };

    return new SymbolNamedType(_runtime, symbol);

    INamedTypeSymbol GetSymbolTypeFromCompilation(string name)
    {
      return _compilation.GetTypeByMetadataName($"{Namespace}.{name}") ?? throw new Exception($"Could not find type {name} on compilation.");
    }
  }
}

public enum TypeCategory
{
  Class
}