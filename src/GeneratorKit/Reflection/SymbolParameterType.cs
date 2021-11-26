using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal class SymbolParameterType : SymbolType
{
  private readonly SymbolType _type;
  private readonly IParameterSymbol _symbol;

  public SymbolParameterType(GeneratorRuntime runtime, Compilation compilation, SymbolType type, IParameterSymbol symbol)
    : base(runtime, compilation)
  {
    _type = type;
    _symbol = symbol;
  }

  public override ITypeSymbol Symbol => _type.Symbol;


  // System.Type overrides

  public override string FullName => _type.FullName;

  public override GenericParameterAttributes GenericParameterAttributes => _type.GenericParameterAttributes;

  public override int GenericParameterPosition => _type.GenericParameterPosition;

  public override string Name => _type.Name;

  public override string Namespace => _type.Namespace;

  public override bool IsSerializable => false;

  public override MemberTypes MemberType => _type.MemberType;

  protected override TypeAttributes GetAttributeFlagsImpl() => _type.Attributes;

  public override Array GetEnumValues() => _type.GetEnumValues();

  protected override bool HasElementTypeImpl() => _type.HasElementType;

  protected override bool IsArrayImpl() => _type.IsArray;

  protected override bool IsByRefImpl()
  {
    return _symbol.RefKind is not RefKind.None;
  }

  protected override bool IsPointerImpl() => _type.IsPointer;

  protected override bool IsPrimitiveImpl() => _type.IsPrimitive;


  // SymbolTypeBase overrides

  protected override SymbolAssembly AssemblyCore => _type.Assembly;

  protected override SymbolType? BaseTypeCore => _type.BaseType;

  protected override SymbolType[] GenericTypeArgumentsCore => _type.GenericTypeArguments;

  protected override SymbolModule ModuleCore => _runtime.CreateModuleDelegator(_symbol.ContainingModule);

  protected override SymbolType? GetElementTypeCore() => _type.GetElementType();

  protected override SymbolType[] GetGenericArgumentsCore() => _type.GetGenericArguments();

  protected override SymbolType[] GetGenericParameterConstraintsCore() => _type.GetGenericParameterConstraints();

  protected override SymbolType GetGenericTypeDefinitionCore() => _type.GetGenericTypeDefinition();

  protected override SymbolType[] GetInterfacesCore() => _type.GetInterfaces();

  protected override SymbolType MakeGenericTypeCore(params Type[] typeArguments) => _type.MakeGenericType(typeArguments);
}