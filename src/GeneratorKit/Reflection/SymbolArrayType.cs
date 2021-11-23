using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolArrayType : SymbolType
{
  private readonly IArrayTypeSymbol _symbol;

  public SymbolArrayType(GeneratorRuntime runtime, Compilation compilation, IArrayTypeSymbol symbol)
    : base(runtime, compilation)
  {
    _symbol = symbol;
  }

  public override ITypeSymbol Symbol => _symbol;

  public override Type[] GenericTypeArguments => Array.Empty<Type>();

  public override string? FullName => WithArraySuffix(GetElementType().FullName);

  public override string Name => WithArraySuffix(GetElementType().Name);

  public override int GetArrayRank()
  {
    return _symbol.Rank;
  }

  public override Type GetElementType()
  {
    return _runtime.CreateTypeDelegator(_symbol.ElementType);
  }

  protected override TypeAttributes GetAttributeFlagsImpl()
  {
    throw new NotImplementedException();
  }

  protected override bool HasElementTypeImpl()
  {
    return true;
  }

  protected override bool IsArrayImpl()
  {
    return true;
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

  private string WithArraySuffix(string name)
  {
    int rank = GetArrayRank();
    if (rank == 1)
    {
      return $"{name}[]";
    }
    return $"{name}[{new string(',', rank - 1)}]";
  }
}