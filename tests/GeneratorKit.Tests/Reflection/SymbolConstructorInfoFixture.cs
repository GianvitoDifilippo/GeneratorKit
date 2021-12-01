using GeneratorKit.TestHelpers;
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

  public class ClassWithDefaultCtor { }
}

";

  private readonly FakeGeneratorRuntime _runtime;
  private readonly Type _classType;
  private readonly Type _classWithDefaultCtorType;
  private readonly INamedTypeSymbol _classSymbol;
  private readonly INamedTypeSymbol _classWithDefaultCtorSymbol;

  public SymbolConstructorInfoFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName);
    if (!output.IsValid)
    {
      throw new Exception($"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");
    }

    _runtime = new FakeGeneratorRuntime(output.Compilation);

    _classType = output.Assembly!.GetType(Namespace + ".Class")!;
    _classWithDefaultCtorType = output.Assembly!.GetType(Namespace + ".ClassWithDefaultCtor")!;

    _classSymbol = output.Compilation.GetTypeByMetadataName(Namespace + ".Class")!;
    _classWithDefaultCtorSymbol = output.Compilation.GetTypeByMetadataName(Namespace + ".ClassWithDefaultCtor")!;

    _runtime.AddType(_classSymbol, _classType);
    _runtime.AddType(_classWithDefaultCtorSymbol, _classWithDefaultCtorType);
  }

  public ConstructorInfo GetReference(ConstructorCategory category)
  {
    return category switch
    {
      ConstructorCategory.PublicConstructor
        => GetConstructorFromType(_classType, 0),

      ConstructorCategory.InternalConstructor
        => GetConstructorFromType(_classType, 1),

      ConstructorCategory.PrivateConstructor
        => GetConstructorFromType(_classType, 2),

      ConstructorCategory.ProtectedConstructor
        => GetConstructorFromType(_classType, 3),

      ConstructorCategory.PrivateProtectedConstructor
        => GetConstructorFromType(_classType, 4),

      ConstructorCategory.ProtectedInternalConstructor
        => GetConstructorFromType(_classType, 5),

      ConstructorCategory.StaticConstructor
        => _classType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Static)[0],

      ConstructorCategory.DefaultConstructor
        => GetConstructorFromType(_classWithDefaultCtorType, 0),

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
        => GetConstructorFromType(_classSymbol, 0),

      ConstructorCategory.InternalConstructor
        => GetConstructorFromType(_classSymbol, 1),

      ConstructorCategory.PrivateConstructor
        => GetConstructorFromType(_classSymbol, 2),

      ConstructorCategory.ProtectedConstructor
        => GetConstructorFromType(_classSymbol, 3),

      ConstructorCategory.PrivateProtectedConstructor
        => GetConstructorFromType(_classSymbol, 4),

      ConstructorCategory.ProtectedInternalConstructor
        => GetConstructorFromType(_classSymbol, 5),

      ConstructorCategory.StaticConstructor
        => _classSymbol.StaticConstructors[0],

      ConstructorCategory.DefaultConstructor
        => GetConstructorFromType(_classWithDefaultCtorSymbol, 0),

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
  StaticConstructor,
  DefaultConstructor
}