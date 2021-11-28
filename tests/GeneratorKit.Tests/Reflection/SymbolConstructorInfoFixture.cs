using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace GeneratorKit.Reflection;

public class SymbolConstructorInfoFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName + ".SymbolConstructorInfo";

  private const BindingFlags s_instanceConstructors = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

  private const string s_source = @"

namespace " + Namespace + @"
{
  public class Class
  {
    static Class() { }
    [System.Obsolete]
    public Class() { }
    internal Class(int param1) { }
    private Class(int param1, short param2) { }
    protected Class(int param1, short param2, long param3) { }
    private protected Class(int param1, short param2, long param3, float param4) { }
    protected internal Class(int param1, short param2, long param3, float param4, double param5) { }
  }
}

";

  private readonly FakeGeneratorRuntime _runtime;
  private readonly Type _type;
  private readonly INamedTypeSymbol _symbol;

  public SymbolConstructorInfoFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName);
    if (!output.IsValid)
    {
      throw new Exception($"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");
    }

    _runtime = new FakeGeneratorRuntime(output.Compilation);

    _type = output.Assembly!.GetType(Namespace + ".Class")!;

    _symbol = output.Compilation.GetTypeByMetadataName(Namespace + ".Class")!;

    _runtime.AddType(_symbol, _type);
  }

  public ConstructorInfo GetReference(ConstructorCategory category)
  {
    return category switch
    {
      ConstructorCategory.PublicConstructor
        => GetConstructorFromType(_type, 0),

      ConstructorCategory.InternalConstructor
        => GetConstructorFromType(_type, 1),

      ConstructorCategory.PrivateConstructor
        => GetConstructorFromType(_type, 2),

      ConstructorCategory.ProtectedConstructor
        => GetConstructorFromType(_type, 3),

      ConstructorCategory.PrivateProtectedConstructor
        => GetConstructorFromType(_type, 4),

      ConstructorCategory.ProtectedInternalConstructor
        => GetConstructorFromType(_type, 5),

      ConstructorCategory.StaticConstructor
        => _type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Static)[0],

      _
        => throw new InvalidOperationException()
    };

    static ConstructorInfo GetConstructorFromType(Type type, int parameterCount)
    {
      return type.GetConstructors(s_instanceConstructors)
        .Single(x => x.GetParameters().Length == parameterCount) ?? throw new Exception($"Could not find constructor with {parameterCount} parameters on type {type.Name}.");
    }
  }

  internal SymbolConstructorInfo GetDelegator(ConstructorCategory category)
  {
    IMethodSymbol symbol = category switch
    {
      ConstructorCategory.PublicConstructor
        => GetConstructorFromType(_symbol, 0),

      ConstructorCategory.InternalConstructor
        => GetConstructorFromType(_symbol, 1),

      ConstructorCategory.PrivateConstructor
        => GetConstructorFromType(_symbol, 2),

      ConstructorCategory.ProtectedConstructor
        => GetConstructorFromType(_symbol, 3),

      ConstructorCategory.PrivateProtectedConstructor
        => GetConstructorFromType(_symbol, 4),

      ConstructorCategory.ProtectedInternalConstructor
        => GetConstructorFromType(_symbol, 5),

      ConstructorCategory.StaticConstructor
        => _symbol.StaticConstructors[0],

      _ => throw new InvalidOperationException()
    };

    return new SymbolConstructorInfo(_runtime, symbol);

    static IMethodSymbol GetConstructorFromType(INamedTypeSymbol symbol, int parameterCount)
    {
      return symbol.InstanceConstructors
        .SingleOrDefault(x => x.Parameters.Length == parameterCount) ?? throw new Exception($"Could not find constructor with {parameterCount} parameters on type {symbol.Name}.");
    }
  }
}

public class ConstructorsDataAttribute : DataAttribute
{
  public ConstructorCategory[] Except { get; set; } = Array.Empty<ConstructorCategory>();

  public override IEnumerable<object[]> GetData(MethodInfo testConstructor)
  {
    return Enum.GetValues<ConstructorCategory>().Where(x => !Except.Contains(x)).Select(x => new object[1] { x });
  }
}

public class ConstructorsDataExceptAttribute : ConstructorsDataAttribute
{
  public ConstructorsDataExceptAttribute(params ConstructorCategory[] except)
  {
    Except = except;
  }
}

public enum ConstructorCategory
{
  PublicConstructor,
  InternalConstructor,
  PrivateConstructor,
  ProtectedConstructor,
  PrivateProtectedConstructor,
  ProtectedInternalConstructor,
  StaticConstructor
}