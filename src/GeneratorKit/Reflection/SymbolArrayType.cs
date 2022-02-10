using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolArrayType : SymbolType
{
  private SymbolType? _elementType;

  public SymbolArrayType(IRuntime runtime, IGeneratorContext context, IArrayTypeSymbol symbol)
    : base(runtime, context)
  {
    Symbol = symbol;
  }

  public new IArrayTypeSymbol Symbol { get; }

  public override INamedTypeSymbol OriginalSymbol => throw new InvalidOperationException();

  protected override ITypeSymbol SymbolCore => Symbol;

  private SymbolType ElementType => _elementType ??= Context.CreateTypeDelegator(Symbol.ElementType);


  // System.Type overrides

  public override MethodBase? DeclaringMethod => throw new InvalidOperationException($"Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override string Namespace => ElementType.Namespace;

  public override GenericParameterAttributes GenericParameterAttributes => throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override int GenericParameterPosition => throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override bool IsConstructedGenericType => false;

  public override bool IsGenericParameter => false;

  public override bool IsGenericType => false;

  public override bool IsGenericTypeDefinition => false;

  public override bool IsSerializable => true;

  public override MemberTypes MemberType => MemberTypes.TypeInfo;

  public override string Name => WithArraySuffix(ElementType.Name);

  public override int GetArrayRank()
  {
    return Symbol.Rank;
  }

  public override IList<CustomAttributeData> GetCustomAttributesData()
  {
    INamedTypeSymbol serializableAttributeSymbol = Runtime.Compilation.GetTypeByMetadataName("System.SerializableAttribute")!;
    List<CustomAttributeData> result = new List<CustomAttributeData>
    {
      CompilationCustomAttributeData.FromParameterlessAttribute(Context, serializableAttributeSymbol)
    };

    return new ReadOnlyCollection<CustomAttributeData>(result);
  }

  protected override TypeAttributes GetAttributeFlagsImpl()
  {
    return TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable;
  }

  public override Array GetEnumValues()
  {
    throw new InvalidOperationException();
  }

  public override Type[] GetGenericArguments()
  {
    return ElementType.GetGenericArguments();
  }

  public override Type[] GetGenericParameterConstraints()
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");
  }

  protected override bool HasElementTypeImpl()
  {
    return true;
  }

  protected override bool IsArrayImpl()
  {
    return true;
  }

  protected sealed override bool IsByRefImpl()
  {
    return false;
  }

  protected override bool IsPointerImpl()
  {
    return false;
  }

  protected override bool IsPrimitiveImpl()
  {
    return false;
  }

  protected override bool IsValueTypeImpl()
  {
    return false;
  }


  // SymbolTypeBase overrides

  protected override SymbolAssembly AssemblyCore => ElementType.Assembly;

  protected override SymbolType? BaseTypeCore => Context.CreateTypeDelegator(Runtime.Compilation.GetSpecialType(SpecialType.System_Array));

  protected override SymbolModule ModuleCore => ElementType.Module;

  protected override SymbolType GetElementTypeCore()
  {
    return ElementType;
  }

  protected override SymbolType GetEnumUnderlyingTypeCore()
  {
    throw new InvalidOperationException();
  }

  protected override SymbolType GetGenericTypeDefinitionCore()
  {
    throw new InvalidOperationException("This operation is only valid on generic types.");
  }

  protected override SymbolType[] GetInterfacesCore()
  {
    return Symbol.AllInterfaces.Map(x => Context.CreateTypeDelegator(x));
  }

  protected override SymbolType MakeArrayTypeCore()
  {
    return Context.CreateTypeDelegator(Runtime.Compilation.CreateArrayTypeSymbol(Symbol));
  }

  protected override SymbolType MakeArrayTypeCore(int rank)
  {
    if (rank == 1)
      throw new NotSupportedException("MDArrays of rank 1 are currently not supported.");

    return Context.CreateTypeDelegator(Runtime.Compilation.CreateArrayTypeSymbol(Symbol, rank));
  }

  protected override SymbolType MakeByRefTypeCore()
  {
    return new SymbolByRefType(Runtime, Context, this);
  }

  protected override SymbolType MakeGenericTypeCore(Type[] typeArguments)
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericTypeDefinition is true.");
  }

  private string WithArraySuffix(string name)
  {
    int rank = GetArrayRank();
    return rank == 1
      ? $"{name}[]"
      : $"{name}[{new string(',', rank - 1)}]";
  }
}