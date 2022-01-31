using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using GeneratorKit.TestHelpers;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;
using static GeneratorKit.TestHelpers.ProxyTypes;

namespace GeneratorKit.Interpret;

public class InterpreterFixture
{
  public const string AssemblyName = "GeneratorKit.Reflection.Tests";
  public const string Namespace = AssemblyName + ".Interpret";

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
    public GenericClassWithMembersSource(int value1, T value2)
      : base(value2)
    {
    }
  }

  public class InterfaceSource : IInterface
  {
    public InterfaceSource(int value1)
    {
    }
  }

  public class NonGenericClass_Interpret
  {
    public void SimpleAssignment()
    {
      int v1;
      v1 = 1;
    }

    public string IfElse(int arg)
    {
      if (arg == 0)
      {
        return ""zero"";
      }
      else
      {
        return ""not zero"";
      }
    }

    public void TupleAssignment()
    {
      int v1;
      string v2;
      (v1, v2) = (2, ""str"");
    }

    public string NameOf()
    {
      return nameof(NonGenericClass_Interpret);
    }
  }

  public class GenericClass_Interpret<T>
  {
    public int Invocation_NonGenericMethod_GenericType()
    {
      return NonGenericMethod_GenericType();
    }

    public int Invocation_GenericMethod_GenericType1()
    {
      return GenericMethod_GenericType<string>();
    }

    public int Invocation_GenericMethod_GenericType2()
    {
      return GenericMethod_GenericType<T>();
    }

    public int Invocation_StaticMethod()
    {
      return StaticMethod();
    }

    public int Property_InstanceProperty()
    {
      return InstanceProperty;
    }

    public int Property_StaticProperty()
    {
      return StaticProperty;
    }

    private int NonGenericMethod_GenericType()
    {
      return typeof(T).Name.Length;
    }

    private int GenericMethod_GenericType<TT>()
    {
      return typeof(TT).Name.Length;
    }

    private static int StaticMethod()
    {
      return typeof(T).Name.Length;
    }

    private int InstanceProperty => typeof(T).Name.Length;

    private static int StaticProperty => typeof(T).Name.Length;
  }
}
";

  private readonly Compilation _compilation;
  private readonly FakeGeneratorRuntime _runtime;
  private readonly FakeOperationManager _operationManager;

  private readonly INamedTypeSymbol _nonGenericClassSourceSymbol;
  private readonly INamedTypeSymbol _nonGenericClassWithMembersSourceSymbol;
  private readonly INamedTypeSymbol _genericClassSourceSymbol;
  private readonly INamedTypeSymbol _genericClassWithMembersSourceSymbol;
  private readonly INamedTypeSymbol _interfaceSourceSymbol;
  private readonly SymbolType _nongenericClassInterpretType;
  private readonly SymbolType _genericClassInterpretType;

  public InterpreterFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName, referencedAssemblies: new[] { typeof(ProxyTypes).Assembly });
    Assert.True(output.IsValid, $"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");

    _compilation = output.Compilation;
    _runtime = new FakeGeneratorRuntime(_compilation);
    _operationManager = new FakeOperationManager(_compilation);

    _nonGenericClassSourceSymbol = GetTypeSymbolFromCompilation("NonGenericClassSource");
    _nonGenericClassWithMembersSourceSymbol = GetTypeSymbolFromCompilation("NonGenericClassWithMembersSource");
    _genericClassSourceSymbol = GetTypeSymbolFromCompilation("GenericClassSource`1");
    _genericClassWithMembersSourceSymbol = GetTypeSymbolFromCompilation("GenericClassWithMembersSource`1");
    _interfaceSourceSymbol = GetTypeSymbolFromCompilation("InterfaceSource");

    _runtime.AddType(_nonGenericClassSourceSymbol, typeof(NonGenericClassProxy));
    _runtime.AddType(_nonGenericClassWithMembersSourceSymbol, typeof(NonGenericClassWithMembersProxy));

    INamedTypeSymbol nongenericClassInterpretSymbol = GetTypeSymbolFromCompilation("NonGenericClass_Interpret");
    INamedTypeSymbol genericClassInterpretSymbol = GetTypeSymbolFromCompilation("GenericClass_Interpret`1");
    _nongenericClassInterpretType = new SymbolNamedType(_runtime, nongenericClassInterpretSymbol);
    _genericClassInterpretType = new SymbolNamedType(_runtime, genericClassInterpretSymbol);
    
    _runtime.AddType(_nonGenericClassSourceSymbol, typeof(NonGenericClassProxy));
    _runtime.AddType(_nongenericClassInterpretType, typeof(ObjectProxy));
    _runtime.AddType(_genericClassInterpretType, typeof(ObjectProxy));

    INamedTypeSymbol GetTypeSymbolFromCompilation(string name)
    {
      INamedTypeSymbol? result = _compilation.GetTypeByMetadataName($"{Namespace}.{name}");
      Assert.NotNull(result);
      return result!;
    }
  }

  internal Interpreter CreateSut(IFrameProvider frameProvider)
  {
    return new Interpreter(_runtime, _operationManager, frameProvider);
  }

  internal SymbolType GetSourceType(SourceType sourceType)
  {
    INamedTypeSymbol symbol = sourceType switch
    {
      SourceType.NonGenericClass                  => _nonGenericClassSourceSymbol,
      SourceType.NonGenericClassWithCtors         => _nonGenericClassWithMembersSourceSymbol,
      SourceType.GenericClass                     => _genericClassSourceSymbol,
      SourceType.GenericClassWithCtors            => _genericClassWithMembersSourceSymbol,
      SourceType.Interface                        => _interfaceSourceSymbol,
      _                                           => throw Errors.Unreacheable
    };

    return new SymbolNamedType(_runtime, symbol);
  }

  internal SymbolMethodInfo GetInterpretedOperationMethod(InterpretedOperationType operationType)
  {
    SymbolType type = GetType(operationType);
    SymbolMethodInfo? method = type.GetMethod(operationType.ToString());
    Assert.NotNull(method);
    return _runtime.CreateMethodInfoDelegator(method!.Symbol);
  }

  internal InterpreterFrame GetClassFrame(IFrameProvider frameProvider, SymbolType sourceType, params Type[] genericArguments)
  {
    return genericArguments.Length > 0
      ? frameProvider.GetClassFrame(sourceType.MakeGenericType(genericArguments))
      : frameProvider.GetClassFrame(sourceType);
  }

  internal InterpreterFrame GetInstanceFrame(IFrameProvider frameProvider, Interpreter sut, InterpretedOperationType operationType, params Type[] genericArguments)
  {
    SymbolType type = GetType(operationType);
    InterpreterFrame classFrame = GetClassFrame(frameProvider, type, genericArguments);
    ObjectProxy instance = new ObjectProxy();
    InterpreterFrame instanceFrame = frameProvider.GetInstanceFrame(classFrame, instance);
    instance.Delegate = new OperationDelegate(sut, instanceFrame, new Dictionary<int, SymbolMethodInfo>());
    return instanceFrame;
  }

  private SymbolType GetType(InterpretedOperationType operationType)
  {
    return (int)operationType switch
    {
      < 4096 => _nongenericClassInterpretType,
      < 8192 => _genericClassInterpretType,
      _      => throw Errors.Unreacheable
    };
  }


  public enum SourceType
  {
    NonGenericClass,
    NonGenericClassWithCtors,
    GenericClass,
    GenericClassWithCtors,
    Interface
  }

  public enum InterpretedOperationType
  {
    SimpleAssignment = 0,
    IfElse,
    TupleAssignment,
    NameOf,

    Invocation_NonGenericMethod_GenericType = 4096,
    Invocation_GenericMethod_GenericType1,
    Invocation_GenericMethod_GenericType2,
    Invocation_StaticMethod,
    Property_InstanceProperty,
    Property_StaticProperty
  }
}