using GeneratorKit.Comparers;
using GeneratorKit.Reflection.Context;
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

  public SymbolArrayType(IReflectionContext context, IArrayTypeSymbol symbol)
    : base(context)
  {
    Symbol = symbol;
  }

  public new IArrayTypeSymbol Symbol { get; }

  protected override ITypeSymbol SymbolCore => Symbol;

  private SymbolType ElementType => _elementType ??= Context.CreateTypeDelegator(Symbol.ElementType);


  // System.Type overrides

  public override bool ContainsGenericParameters => Context.ContainsGenericParameters(Symbol);

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
    INamedTypeSymbol serializableAttributeSymbol = Context.Compilation.GetTypeByMetadataName("System.SerializableAttribute")!;
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

  protected override SymbolType? BaseTypeCore => BaseType;

  protected override SymbolModule ModuleCore => ElementType.Module;

  protected override SymbolType GetElementTypeCore()
  {
    return ElementType;
  }

  protected override SymbolType GetEnumUnderlyingTypeCore()
  {
    throw new InvalidOperationException();
  }

  protected override SymbolNamedType GetGenericTypeDefinitionCore()
  {
    throw new InvalidOperationException("This operation is only valid on generic types.");
  }

  protected override SymbolType[] GetInterfacesCore()
  {
    return Symbol.AllInterfaces.Map(x => Context.CreateTypeDelegator(x));
  }

  public override bool IsSuperclassOf(Type? c)
  {
    return false;
  }

  protected override bool IsAssignableFromCore(Type c)
  {
    return TypeEqualityComparer.Default.Equals(this, c);
  }

  protected override SymbolType MakeArrayTypeCore()
  {
    return Context.CreateTypeDelegator(Context.Compilation.CreateArrayTypeSymbol(Symbol));
  }

  protected override SymbolType MakeArrayTypeCore(int rank)
  {
    if (rank == 1)
      throw new NotSupportedException("MDArrays of rank 1 are currently not supported.");

    return Context.CreateTypeDelegator(Context.Compilation.CreateArrayTypeSymbol(Symbol, rank));
  }

  protected override SymbolType MakeByRefTypeCore()
  {
    return new SymbolByRefType(Context, this);
  }

  protected override SymbolNamedType MakeGenericTypeCore(Type[] typeArguments)
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericTypeDefinition is true.");
  }


  // New members

  public new SymbolNamedType BaseType => Context.CreateTypeDelegator(Context.Compilation.GetSpecialType(SpecialType.System_Array));


  // Other members

  private string WithArraySuffix(string name)
  {
    int rank = GetArrayRank();
    return rank == 1
      ? $"{name}[]"
      : $"{name}[{new string(',', rank - 1)}]";
  }
}