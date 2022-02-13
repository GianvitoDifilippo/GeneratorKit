using GeneratorKit.Interpret.Frame;
using GeneratorKit.Proxy;
using GeneratorKit.Reflection;
using GeneratorKit.Reflection.Context;
using GeneratorKit.TestHelpers;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Threading;
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
  public class NonGenericClassSource : NonGenericClass
  {
    static NonGenericClassSource() => _staticFieldFromCctor = 'a';

    private readonly int _instanceField = 12;
    private string InstanceProperty { get; set; } = ""prop1"";

    private static readonly char _staticFieldFromCctor;
    private static readonly int _staticField = 13;
    private static string StaticProperty { get; set; } = ""prop2"";
  }

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
    private int _field = 4;

    public NonGenericClass_Interpret A { get; }

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

    public void Discard()
    {
      _ = 4;
    }

    public int Default()
    {
      return default;
    }

    public T DefaultGeneric<T>()
    {
      return default(T);
    }

    public int For()
    {
      int total = 0;
      for (int i = 0; i < 5; i++)
      {
        total += i;
      }
      return total;
    }

    public int ForEach(System.Collections.Generic.IEnumerable<int> enumerable)
    {
      int total = 1;
      foreach (int integer in enumerable)
      {
        total *= integer;
      }
      return total;
    }

    public int While()
    {
      int total = 2;
      while (total < 12)
      {
        total *= 2;
      }
      return total;
    }

    public int DoWhile()
    {
      int total = 2;
      do
      {
        total *= 2;
      }
      while (total > 2 && total < 24);
      return total;
    }

    public int For_WithBreak()
    {
      int total = 0;
      for (int i = 0; i < 5; i++)
      {
        if (i == 4) break;
        total += i;
      }
      return total;
    }

    public int ForEach_WithBreak(System.Collections.Generic.IEnumerable<int> enumerable)
    {
      int total = 1;
      foreach (int integer in enumerable)
      {
        if (integer == 2)
        {
          total = 91;
          break;
        }
        total *= integer;
      }
      return total;
    }

    public int While_WithBreak()
    {
      int total = 2;
      while (total < 12)
      {
        if (total == 4) break;
        total *= 2;
      }
      return total;
    }

    public int DoWhile_WithBreak()
    {
      int total = 2;
      do
      {
        if (total == 8) break;
        total *= 2;
      }
      while (total > 2 && total < 24);
      return total;
    }

    public int For_WithContinue()
    {
      int total = 0;
      for (int i = 0; i < 5; i++)
      {
        if (i == 4)
        {
          total++;
          continue;
        }
        total += i;
      }
      return total;
    }

    public int ForEach_WithContinue(System.Collections.Generic.IEnumerable<int> enumerable)
    {
      int total = 1;
      foreach (int integer in enumerable)
      {
        if (integer == 2)
        {
          total++;
          continue;
        }
        total *= integer;
      }
      return total;
    }

    public int While_WithContinue()
    {
      int total = 2;
      while (total < 12)
      {
        if (total == 4)
        {
          total++;
          continue;
        }
        total *= 2;
      }
      return total;
    }

    public int DoWhile_WithContinue()
    {
      int total = 2;
      do
      {
        if (total == 8)
        {
          total++;
          continue;
        }
        total *= 2;
      }
      while (total > 2 && total < 24);
      return total;
    }

    public void StringInterpolation()
    {
      string str1 = $""{new System.DateTime(2022, 02, 01, 19, 44, 12):HH:mm:ss}"";
      string str2 = $""|{""Left"",-7}|{""Right"",7}|"";
      string str3 = $""{System.Math.PI,20}"";
      string str4 = $""{System.Math.PI,20:F3}"";
      string str5 = $""'str2' is {(str2.Length > str3.Length ? ""longer"" : ""shorter"")} than 'str3'"";
    }

    public void ObjectInitializer()
    {
      OtherClass obj = new OtherClass
      {
        Prop1 = 3,
        Prop2 = ""prop""
      };
      var prop1 = obj.Prop1;
      var prop2 = obj.Prop2;
    }

    public void Coalesce()
    {
      string? i1 = null;
      string i2 = i1 ?? ""4"";
      string i3 = i2 ?? ""5"";
    }

    public void CoalesceAssignment()
    {
      int? i1 = null;
      i1 ??= 4;
      i1 ??= 5;
    }

    public void ConditionalAccess()
    {
      OtherClass obj1 = null;
      int? v1 = obj1?.Prop1;
      OtherClass obj2 = new OtherClass { Prop1 = 4 };
      int? v2 = obj2?.Prop1;
    }

    public void FieldReference_Source()
    {
      OtherClass obj = new OtherClass();
      _field = 5;
      int f1 = _field;
      obj.Field = 12;
      int f2 = obj.Field;
    }

    public void IsType()
    {
      OtherClass obj1 = null;
      OtherClass obj2 = new OtherClass();
      int i1 = 3;
      bool b1 = obj1 is OtherClass;
      bool b2 = obj1 is object;
      bool b3 = obj2 is OtherClass;
      bool b4 = obj2 is object;
      bool b5 = obj2 is int;
      bool b6 = i1 is int;
      bool b7 = i1 is object;
      bool b8 = i1 is string;
    }

  public void Array()
  {
    int[] arr1 = new int[4] { 1, 2, 3, 4 };
    int[][] arr2 = new int[][] { new[] { 1, 2 }, new[] { 3, 4 }, new[] { 5, 6 } };
    int[,,] arr3 = new int[2, 3, 4] { { { 1, 2, 3, 4 }, { 5, 6, 7, 8 }, { 9, 10, 11, 12 } }, { { 13, 14, 15, 16 }, { 17, 18, 19, 20 }, { 21, 22, 23, 24 } } };
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

    public void GenericObjectCreation(T arg1)
    {
      OtherGenericClass<T, int> obj = new OtherGenericClass<T, int>(arg1, 4);
      T value1 = obj.Arg1;
      int value2 = obj.Arg2;
    }

    public T FieldReference_Ref(T arg1)
    {
      GenericClassWithMembersProxy<T> obj = new GenericClassWithMembersProxy<T>(arg1);
      return obj.Field;
    }

    public T MethodReference_Ref<T1>(T1 arg)
    {
      System.Func<T1, T> func = MethodReference_Ref;
      return default(T);
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

  public class OtherClass
  {
    public int Prop1 { get; set; }
    public string Prop2 { get; set; }
    public int Field = 50;
  }

  public class OtherGenericClass<T1, T2>
  {
    private int _a;

    public OtherGenericClass(T1 arg1, T2 arg2)
    {
      Arg1 = arg1;
      Arg2 = arg2;
    }

    public T1 Arg1 { get; }
    public T2 Arg2 { get; }
  }
}
";

  private readonly Compilation _compilation;

  private readonly INamedTypeSymbol _nonGenericClassSourceSymbol;
  private readonly INamedTypeSymbol _nonGenericClassWithMembersSourceSymbol;
  private readonly INamedTypeSymbol _genericClassSourceSymbol;
  private readonly INamedTypeSymbol _genericClassWithMembersSourceSymbol;
  private readonly INamedTypeSymbol _interfaceSourceSymbol;
  private readonly INamedTypeSymbol _nongenericClassInterpretSymbol;
  private readonly INamedTypeSymbol _genericClassInterpretSymbol;

  public InterpreterFixture()
  {
    CompilationOutput output = CompilationOutput.Create(s_source, AssemblyName, referencedAssemblies: new[] { typeof(ProxyTypes).Assembly });
    Assert.True(output.IsValid, $"Could not compile the source code.\n\nDiagnostics:\n{string.Join('\n', output.Diagnostics)}");

    _compilation = output.Compilation;

    _nonGenericClassSourceSymbol = GetTypeSymbolFromCompilation("NonGenericClassSource");
    _nonGenericClassWithMembersSourceSymbol = GetTypeSymbolFromCompilation("NonGenericClassWithMembersSource");
    _genericClassSourceSymbol = GetTypeSymbolFromCompilation("GenericClassSource`1");
    _genericClassWithMembersSourceSymbol = GetTypeSymbolFromCompilation("GenericClassWithMembersSource`1");
    _interfaceSourceSymbol = GetTypeSymbolFromCompilation("InterfaceSource");
    _nongenericClassInterpretSymbol = GetTypeSymbolFromCompilation("NonGenericClass_Interpret");
    _genericClassInterpretSymbol = GetTypeSymbolFromCompilation("GenericClass_Interpret`1");

    INamedTypeSymbol GetTypeSymbolFromCompilation(string name)
    {
      INamedTypeSymbol? result = _compilation.GetTypeByMetadataName($"{Namespace}.{name}");
      Assert.NotNull(result);
      return result!;
    }
  }

  internal Interpreter CreateSut(FakeFrameProvider frameProvider, out GeneratorRuntime runtime)
  {
    ProxyManager proxyManager = new ProxyManager();
    proxyManager.RegisterProxyType(typeof(NonGenericClassProxy));
    proxyManager.RegisterProxyType(typeof(NonGenericClassWithMembersProxy));

    FakeDependencyFactory dependencyFactory = new FakeDependencyFactory(frameProvider, _compilation);
    runtime = new GeneratorRuntime(_compilation, dependencyFactory, CancellationToken.None);
    return dependencyFactory.Interpreter;
  }

  internal SymbolType GetSourceType(GeneratorRuntime runtime, SourceType sourceType)
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

    return (SymbolNamedType)runtime.CreateTypeDelegator(symbol);
  }

  internal IMethodSymbol GetInterpretedOperationMethod(GeneratorRuntime runtime, InterpretedOperationType operationType)
  {
    SymbolType type = GetType(runtime, operationType);
    SymbolMethodInfo? method = type.GetMethod(operationType.ToString());
    Assert.NotNull(method);
    return method!.OriginalSymbol;
  }

  internal InterpreterFrame GetInstanceFrame(GeneratorRuntime runtime, Interpreter sut, InterpretedOperationType operationType, params Type[] genericArguments)
  {
    SymbolType type = GetType(runtime, operationType);
    InterpreterFrame typeFrame = genericArguments.Length > 0
      ? sut.GetTypeFrame(type.MakeGenericType(genericArguments))
      : sut.GetTypeFrame(type);
    ObjectProxy instance = new ObjectProxy();
    InterpreterFrame instanceFrame = sut.GetInstanceFrame(typeFrame, type, instance);
    instance.Delegate = new OperationDelegate(sut, type, instanceFrame, new Dictionary<int, IMethodSymbol>());
    return instanceFrame;
  }

  private SymbolType GetType(GeneratorRuntime runtime, InterpretedOperationType operationType)
  {
    return (int)operationType switch
    {
      < 4096 => (SymbolType)runtime.CreateTypeDelegator(_nongenericClassInterpretSymbol),
      < 8192 => (SymbolType)runtime.CreateTypeDelegator(_genericClassInterpretSymbol),
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
    Discard,
    Default,
    DefaultGeneric,
    For,
    ForEach,
    ObjectInitializer,
    Coalesce,
    CoalesceAssignment,
    ConditionalAccess,
    While,
    DoWhile,
    For_WithBreak,
    ForEach_WithBreak,
    While_WithBreak,
    DoWhile_WithBreak,
    For_WithContinue,
    ForEach_WithContinue,
    While_WithContinue,
    DoWhile_WithContinue,
    StringInterpolation,
    FieldReference_Source,
    IsType,
    Array,

    Invocation_NonGenericMethod_GenericType = 4096,
    Invocation_GenericMethod_GenericType1,
    Invocation_GenericMethod_GenericType2,
    Invocation_StaticMethod,
    Property_InstanceProperty,
    Property_StaticProperty,
    GenericObjectCreation,
    FieldReference_Ref,
    MethodReference_Ref
  }
}