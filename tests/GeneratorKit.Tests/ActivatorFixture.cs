#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Interpret;
using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using Xunit;
using static GeneratorKit.TestHelpers.ProxyTypes;

namespace GeneratorKit;

public class ActivatorFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName;

  private const string s_source = @"

using static GeneratorKit.TestHelpers.ProxyTypes;

namespace " + Namespace + @"
{
  public class NonGenericClassSource : NonGenericClass { }

  public class NonGenericClassWithMembersSource : NonGenericClassWithMembers
  {
    public NonGenericClassWithMembersSource(int value1)
      : base(value1)
    {
    }

    public NonGenericClassWithMembersSource()
      : base(42)
    {
    }

    public NonGenericClassWithMembersSource(int value1, string value2)
      : base(value2)
    {
    }
  }

  public class GenericClassSource<T> : GenericClass<T> { }

  public class GenericClassWithMembersSource<T> : GenericClassWithMembers<T>
    where T : class
  {
    public GenericClassWithMembersSource(int value1)
      : base(value1)
    {
    }

    public GenericClassWithMembersSource()
      : base(42)
    {
    }

    public GenericClassWithMembersSource(int value1, T value2)
      : base(value2)
    {
    }
  }

  public class NonGenericClassGenericBaseSource : GenericClassWithMembers<string>
  {
    public NonGenericClassGenericBaseSource(int value1)
      : base(value1)
    {
    }

    public NonGenericClassGenericBaseSource()
      : base(42)
    {
    }

    public NonGenericClassGenericBaseSource(int value1, string value2)
      : base(value2)
    {
    }
  }

  public class InterfaceSource : IInterface
  {
    public InterfaceSource(int value1)
    {
    }

    public InterfaceSource(string value2)
    {
    }
  }
}
";

  private readonly Compilation _compilation;
  private readonly FakeGeneratorRuntime _runtime;

  private readonly INamedTypeSymbol _nonGenericClassSourceSymbol;
  private readonly INamedTypeSymbol _nonGenericClassWithMembersSourceSymbol;
  private readonly INamedTypeSymbol _genericClassSourceSymbol;
  private readonly INamedTypeSymbol _genericClassWithMembersSourceSymbol;
  private readonly INamedTypeSymbol _nonGenericClassGenericBaseSourceSymbol;
  private readonly INamedTypeSymbol _interfaceSourceSymbol;

  public ActivatorFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName, referencedAssemblies: new[] { typeof(ProxyTypes).Assembly });
    Assert.True(output.IsValid, $"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");

    _compilation = output.Compilation;
    _runtime = new FakeGeneratorRuntime(_compilation);

    _nonGenericClassSourceSymbol = GetTypeSymbolFromCompilation("NonGenericClassSource");
    _nonGenericClassWithMembersSourceSymbol = GetTypeSymbolFromCompilation("NonGenericClassWithMembersSource");
    _genericClassSourceSymbol = GetTypeSymbolFromCompilation("GenericClassSource`1");
    _genericClassWithMembersSourceSymbol = GetTypeSymbolFromCompilation("GenericClassWithMembersSource`1");
    _nonGenericClassGenericBaseSourceSymbol = GetTypeSymbolFromCompilation("NonGenericClassGenericBaseSource");
    _interfaceSourceSymbol = GetTypeSymbolFromCompilation("InterfaceSource");

    INamedTypeSymbol stringSymbol = _compilation.GetSpecialType(SpecialType.System_String);
    _runtime.AddType(_nonGenericClassSourceSymbol, typeof(NonGenericClassProxy));
    _runtime.AddType(_nonGenericClassWithMembersSourceSymbol, typeof(NonGenericClassWithMembersProxy));
    _runtime.AddType(_genericClassSourceSymbol, typeof(GenericClassProxy<>));
    _runtime.AddType(_genericClassSourceSymbol.Construct(stringSymbol), typeof(GenericClassProxy<string>));
    _runtime.AddType(_genericClassWithMembersSourceSymbol, typeof(GenericClassWithMembersProxy<>));
    _runtime.AddType(_genericClassWithMembersSourceSymbol.Construct(stringSymbol), typeof(GenericClassWithMembersProxy<string>));
    _runtime.AddType(_nonGenericClassGenericBaseSourceSymbol, typeof(GenericClassWithMembersProxy<string>));
    _runtime.AddType(_interfaceSourceSymbol, typeof(InterfaceProxy));

    INamedTypeSymbol GetTypeSymbolFromCompilation(string name)
    {
      INamedTypeSymbol? result = _compilation.GetTypeByMetadataName($"{Namespace}.{name}");
      Assert.NotNull(result);
      return result!;
    }
  }

  internal SymbolType GetSourceType(SourceType sourceType)
  {
    INamedTypeSymbol symbol = sourceType switch
    {
      SourceType.NonGenericClass                  => _nonGenericClassSourceSymbol,
      SourceType.NonGenericClassWithCtors         => _nonGenericClassWithMembersSourceSymbol,
      SourceType.GenericClass                     => _genericClassSourceSymbol,
      SourceType.GenericClassWithCtors            => _genericClassWithMembersSourceSymbol,
      SourceType.NonGenericClassGenericBaseSource => _nonGenericClassGenericBaseSourceSymbol,
      SourceType.Interface                        => _interfaceSourceSymbol,
      _                                           => throw Errors.Unreacheable
    };

    return new SymbolNamedType(_runtime, symbol);
  }

  internal static InterpreterFrame GetClassFrame(params Type[] typeArguments)
  {
    return InterpreterFrame.NewClassFrame(null, new Dictionary<ISymbol, object?>(SymbolEqualityComparer.Default), typeArguments);
  }

  internal static InterpreterFrame GetInstanceFrame(InterpreterFrame classFrame, IRuntimeType type, object instance)
  {
    return InterpreterFrame.NewInstanceFrame(classFrame, new Dictionary<ISymbol, object?>(SymbolEqualityComparer.Default), instance);
  }

  internal static InterpreterFrame GetConstructorFrame(InterpreterFrame classFrame)
  {
    return InterpreterFrame.NewMethodFrame(classFrame, new Dictionary<ISymbol, object?>(SymbolEqualityComparer.Default), Type.EmptyTypes);
  }

  public enum SourceType
  {
    NonGenericClass,
    NonGenericClassWithCtors,
    GenericClass,
    GenericClassWithCtors,
    NonGenericClassGenericBaseSource,
    Interface
  }
}