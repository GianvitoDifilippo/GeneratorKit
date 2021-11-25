using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolArrayType : SymbolType
{
  private readonly IArrayTypeSymbol _symbol;
  private readonly SymbolType _elementType;

  public SymbolArrayType(GeneratorRuntime runtime, Compilation compilation, IArrayTypeSymbol symbol)
    : base(runtime, compilation)
  {
    _symbol = symbol;
    _elementType = runtime.CreateTypeDelegator(_symbol.ElementType);
  }

  public override ITypeSymbol Symbol => _symbol;


  // System.Type overrides

  public override MethodBase? DeclaringMethod => throw new InvalidOperationException($"Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override string Namespace => _elementType.Namespace;

  public override string? FullName => WithArraySuffix(_elementType.FullName);

  public override bool IsConstructedGenericType => false;

  public override bool IsGenericParameter => false;

  public override bool IsGenericType => false;

  public override bool IsGenericTypeDefinition => false;

  public override bool IsSerializable => true;

  public override bool IsGenericParameter => false;

  public override string Name => WithArraySuffix(_elementType.Name);

  public override int GetArrayRank()
  {
    return _symbol.Rank;
  }

  public override IList<CustomAttributeData> GetCustomAttributesData()
  {
    List<CustomAttributeData> result = new List<CustomAttributeData>
    {
      CompilationCustomAttributeData.FromParameterlessAttribute(_runtime, _compilation.GetTypeByMetadataName("System.SerializableAttribute")!)
    };

    return new ReadOnlyCollection<CustomAttributeData>(result);
  }

  protected override TypeAttributes GetAttributeFlagsImpl()
  {
    return TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable;
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

  protected override SymbolAssembly AssemblyCore => _elementType.Assembly;

  protected override SymbolType[] GenericTypeArgumentsCore => Array.Empty<SymbolType>();

  protected override SymbolModule ModuleCore => _elementType.Module;

  protected override SymbolType GetElementTypeCore()
  {
    return _elementType;
  }

  protected override SymbolType[] GetGenericArgumentsCore()
  {
    return _elementType.GetGenericArguments();
  }

  protected override SymbolType[] GetGenericParameterConstraintsCore()
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");
  }

  private string WithArraySuffix(string name)
  {
    int rank = GetArrayRank();
    return rank == 1
      ? $"{name}[]"
      : $"{name}[{new string(',', rank - 1)}]";
  }
}