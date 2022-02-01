using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolArrayType : SymbolType
{
  private readonly SymbolType _elementType;

  public SymbolArrayType(GeneratorRuntime runtime, IArrayTypeSymbol symbol)
    : base(runtime)
  {
    Symbol = symbol;
    _elementType = runtime.CreateTypeDelegator(Symbol.ElementType);
  }

  public new IArrayTypeSymbol Symbol { get; }

  protected override ITypeSymbol SymbolCore => Symbol;


  // System.Type overrides

  public override MethodBase? DeclaringMethod => throw new InvalidOperationException($"Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override string Namespace => _elementType.Namespace;

  public override GenericParameterAttributes GenericParameterAttributes => throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override int GenericParameterPosition => throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override bool IsConstructedGenericType => false;

  public override bool IsGenericParameter => false;

  public override bool IsGenericType => false;

  public override bool IsGenericTypeDefinition => false;

  public override bool IsSerializable => true;

  public override MemberTypes MemberType => MemberTypes.TypeInfo;

  public override string Name => WithArraySuffix(_elementType.Name);

  public override int GetArrayRank()
  {
    return Symbol.Rank;
  }

  public override IList<CustomAttributeData> GetCustomAttributesData()
  {
    INamedTypeSymbol serializableAttributeSymbol = _runtime.Compilation.GetTypeByMetadataName("System.SerializableAttribute")!;
    List<CustomAttributeData> result = new List<CustomAttributeData>
    {
      CompilationCustomAttributeData.FromParameterlessAttribute(_runtime, serializableAttributeSymbol)
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


  // GeneratorRuntimeType overrides

  protected override SymbolNamedType RuntimeDefinition => throw new InvalidOperationException();

  protected override Type[] RuntimeTypeArguments => throw new InvalidOperationException();

  protected override IRuntimeType? RuntimeBaseType => throw new InvalidOperationException();


  // SymbolTypeBase overrides

  protected override SymbolAssembly AssemblyCore => _elementType.Assembly;

  protected override SymbolType? BaseTypeCore => _runtime.CreateTypeDelegator(Symbol.BaseType!);

  protected override SymbolModule ModuleCore => _elementType.Module;

  protected override SymbolType GetElementTypeCore()
  {
    return _elementType;
  }

  protected override SymbolType GetEnumUnderlyingTypeCore()
  {
    throw new InvalidOperationException();
  }

  protected override SymbolType[] GetGenericArgumentsCore()
  {
    return _elementType.GetGenericArguments();
  }

  protected override SymbolType GetGenericTypeDefinitionCore()
  {
    throw new InvalidOperationException("This operation is only valid on generic types.");
  }

  protected override SymbolType[] GetGenericParameterConstraintsCore()
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");
  }

  protected override SymbolType[] GetInterfacesCore()
  {
    return Symbol.AllInterfaces.Select(x => _runtime.CreateTypeDelegator(x)).ToArray();
  }

  protected override SymbolType MakeArrayTypeCore()
  {
    return _runtime.CreateTypeDelegator(_runtime.Compilation.CreateArrayTypeSymbol(Symbol));
  }

  protected override SymbolType MakeArrayTypeCore(int rank)
  {
    if (rank == 1)
      throw new NotSupportedException("MDArrays of rank 1 are currently not supported.");

    return _runtime.CreateTypeDelegator(_runtime.Compilation.CreateArrayTypeSymbol(Symbol, rank));
  }

  protected override SymbolType MakeByRefTypeCore()
  {
    return new SymbolByRefType(_runtime, this);
  }

  protected override HybridGenericType MakeGenericTypeCore(Type[] typeArguments)
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericTypeDefinition is true.");
  }

  protected override SymbolType MakeGenericTypeCore(SymbolType[] typeArguments)
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