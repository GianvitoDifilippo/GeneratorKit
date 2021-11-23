using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolTypeParameter : SymbolType
{
  private readonly ITypeParameterSymbol _symbol;

  public SymbolTypeParameter(GeneratorRuntime runtime, Compilation compilation, ITypeParameterSymbol symbol)
    : base(runtime, compilation)
  {
    _symbol = symbol;
  }

  public override ITypeSymbol Symbol => _symbol;


  public override string? FullName => null;

  public override int GenericParameterPosition => _symbol.Ordinal;

  public override bool IsConstructedGenericType => false;

  public override bool IsGenericParameter => true;

  public override string Name => Symbol.MetadataName;

  public override Type? GetElementType()
  {
    return null;
  }

  protected override TypeAttributes GetAttributeFlagsImpl()
  {
    throw new NotImplementedException();
  }

  protected override bool HasElementTypeImpl()
  {
    return false;
  }

  protected override bool IsArrayImpl()
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
}