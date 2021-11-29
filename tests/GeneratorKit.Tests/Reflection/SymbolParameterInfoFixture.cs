using GeneratorKit.TestHelpers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace GeneratorKit.Reflection;

public class SymbolParameterInfoFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName + ".SymbolParameterInfo";

  private const string s_source =
@"

namespace " + Namespace + @"
{
  public class Class
  {
    public int Method(int arg1, string arg2) => throw null;
    public T GenericMethod<T>(T arg1) => throw null;
    public int ReturnValue() => throw null;
    public ref int RefReturnValue() => throw null;
    public void WithRefParameter(ref int arg1) => throw null;
    public void WithInParameter(in int arg1) => throw null;
    public void WithOutParameter(out int arg1) => throw null;
    public void WithDefaultValue(int arg1 = 2) => throw null;
    public void WithParams(params int[] arg1) => throw null;
    public void WithAttribute([System.CLSCompliant(true)] int arg1) => throw null;
    public int this[int arg1] => throw null;
  }

  public static class StaticClass
  {
    public static void WithThis(this int arg1) => throw null;
  }
}

";

  private readonly FakeGeneratorRuntime _runtime;
  private readonly Type _type;
  private readonly Type _staticType;
  private readonly INamedTypeSymbol _symbol;
  private readonly INamedTypeSymbol _staticSymbol;

  public SymbolParameterInfoFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName);
    if (!output.IsValid)
    {
      throw new Exception($"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");
    }

    _runtime = new FakeGeneratorRuntime(output.Compilation);

    _type = output.Assembly!.GetType(Namespace + ".Class")!;
    _staticType = output.Assembly!.GetType(Namespace + ".StaticClass")!;

    _symbol = output.Compilation.GetTypeByMetadataName(Namespace + ".Class")!;
    _staticSymbol = output.Compilation.GetTypeByMetadataName(Namespace + ".StaticClass")!;

    _runtime.AddType(_symbol, _type);
  }

  public ParameterInfo GetReference(ParameterCategory category)
  {
    return category switch
    {
      ParameterCategory.Position0Parameter        => GetParameterFromMethod("Method", 0),
      ParameterCategory.Position1Parameter        => GetParameterFromMethod("Method", 1),
      ParameterCategory.GenericParameter          => GetParameterFromMethod("GenericMethod"),
      ParameterCategory.ReturnParameter           => GetParameterFromMethod("ReturnValue", -1),
      ParameterCategory.RefReturnParameter        => GetParameterFromMethod("RefReturnValue", -1),
      ParameterCategory.RefParameter              => GetParameterFromMethod("WithRefParameter"),
      ParameterCategory.InParameter               => GetParameterFromMethod("WithInParameter"),
      ParameterCategory.OutParameter              => GetParameterFromMethod("WithOutParameter"),
      ParameterCategory.ParameterWithDefaultValue => GetParameterFromMethod("WithDefaultValue"),
      ParameterCategory.ParamsParameter           => GetParameterFromMethod("WithParams"),
      ParameterCategory.ParameterWithAttribute    => GetParameterFromMethod("WithAttribute"),
      ParameterCategory.IndexerParameter          => GetParameterFromIndexer(),
      ParameterCategory.ThisParameter             => GetParameterFromMethod("WithThis"),
      _                                           => throw new InvalidOperationException()
    };

    ParameterInfo GetParameterFromMethod(string name, int position = 0)
    {
      Type type = name == "WithThis" ? _staticType : _type;

      MethodInfo method = type.GetMethod(name) ?? throw new Exception($"Could not find method {name} in type {type.Name}.");
      return position == -1
        ? method.ReturnParameter
        : method.GetParameters()[position];
    }

    ParameterInfo GetParameterFromIndexer(int position = 0)
    {
      PropertyInfo property = _type.GetProperty("Item") ?? throw new Exception($"Could not find property Item in type {_type.Name}.");
      return property.GetIndexParameters()[position];
    }
  }

  internal SymbolParameterInfo GetDelegator(ParameterCategory category)
  {
    return category switch
    {
      ParameterCategory.Position0Parameter        => GetParameterFromMethod("Method", 0),
      ParameterCategory.Position1Parameter        => GetParameterFromMethod("Method", 1),
      ParameterCategory.GenericParameter          => GetParameterFromMethod("GenericMethod"),
      ParameterCategory.ReturnParameter           => GetParameterFromMethod("ReturnValue", -1),
      ParameterCategory.RefReturnParameter        => GetParameterFromMethod("RefReturnValue", -1),
      ParameterCategory.RefParameter              => GetParameterFromMethod("WithRefParameter"),
      ParameterCategory.InParameter               => GetParameterFromMethod("WithInParameter"),
      ParameterCategory.OutParameter              => GetParameterFromMethod("WithOutParameter"),
      ParameterCategory.ParameterWithDefaultValue => GetParameterFromMethod("WithDefaultValue"),
      ParameterCategory.ParamsParameter           => GetParameterFromMethod("WithParams"),
      ParameterCategory.ParameterWithAttribute    => GetParameterFromMethod("WithAttribute"),
      ParameterCategory.IndexerParameter          => GetParameterFromIndexer(),
      ParameterCategory.ThisParameter             => GetParameterFromMethod("WithThis"),
      _                                           => throw new InvalidOperationException()
    };

    SymbolParameterInfo GetParameterFromMethod(string name, int position = 0)
    {
      INamedTypeSymbol symbol = name == "WithThis" ? _staticSymbol : _symbol;
      IMethodSymbol methodSymbol = (IMethodSymbol)symbol.GetMembers(name).Single();
      return position == -1
        ? new SymbolReturnParameter(_runtime, methodSymbol)
        : new SymbolArgumentParameter(_runtime, methodSymbol.Parameters[position]);
    }

    SymbolArgumentParameter GetParameterFromIndexer(int position = 0)
    {
      IPropertySymbol methodSymbol = (IPropertySymbol)_symbol.GetMembers("this[]").Single();
      IParameterSymbol parameterSymbol = methodSymbol.Parameters[position];
      return new SymbolArgumentParameter(_runtime, parameterSymbol);
    }
  }
}

public class ParametersDataAttribute : DataAttribute
{
  public ParameterCategory[] Except { get; set; } = Array.Empty<ParameterCategory>();

  public override IEnumerable<object[]> GetData(MethodInfo testMethod)
  {
    return Enum.GetValues<ParameterCategory>().Where(x => !Except.Contains(x)).Select(x => new object[1] { x });
  }
}

public class ParametersDataExceptAttribute : ParametersDataAttribute
{
  public ParametersDataExceptAttribute(params ParameterCategory[] except)
  {
    Except = except;
  }
}

public enum ParameterCategory
{
  Position0Parameter,
  Position1Parameter,
  GenericParameter,
  ReturnParameter,
  RefReturnParameter,
  RefParameter,
  InParameter,
  OutParameter,
  ParameterWithDefaultValue,
  ParamsParameter,
  ParameterWithAttribute,
  IndexerParameter,
  ThisParameter,
}