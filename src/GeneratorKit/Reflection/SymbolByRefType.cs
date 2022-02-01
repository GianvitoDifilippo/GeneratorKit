using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal class SymbolByRefType : SymbolType
{
  private readonly SymbolType _elementType;

  public SymbolByRefType(GeneratorRuntime runtime, SymbolType elementType)
    : base(runtime)
  {
    _elementType = elementType;
  }

  protected override ITypeSymbol SymbolCore => _elementType.Symbol;


  // System.Type overrides

  public override bool ContainsGenericParameters => _elementType.ContainsGenericParameters;

  public override MethodBase? DeclaringMethod => throw new InvalidOperationException($"Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override GenericParameterAttributes GenericParameterAttributes => throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override int GenericParameterPosition => throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override string Namespace => _elementType.Namespace;

  public override string Name => _elementType.Name + "&";

  protected override TypeAttributes GetAttributeFlagsImpl()
  {
    return default;
  }

  public override int GetArrayRank()
  {
    throw new ArgumentException("Must be an array type.");
  }

  protected override bool HasElementTypeImpl()
  {
    return true;
  }

  protected override bool IsArrayImpl()
  {
    return false;
  }

  protected override bool IsByRefImpl()
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


  // GeneratorRuntimeType overrides

  protected override SymbolNamedType RuntimeDefinition => throw new InvalidOperationException();

  protected override Type[] RuntimeTypeArguments => throw new InvalidOperationException();

  protected override IRuntimeType? RuntimeBaseType => throw new InvalidOperationException();


  // SymbolTypeBase overrides

  protected override SymbolAssembly AssemblyCore => _elementType.Assembly;

  protected override SymbolType? BaseTypeCore => null;

  protected override SymbolModule ModuleCore => _elementType.Module;

  protected override SymbolType? GetElementTypeCore()
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

  protected override SymbolType[] GetGenericParameterConstraintsCore()
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");
  }

  protected override SymbolType GetGenericTypeDefinitionCore()
  {
    throw new InvalidOperationException("This operation is only valid on generic types.");
  }

  protected override SymbolType[] GetInterfacesCore()
  {
    return Array.Empty<SymbolType>();
  }

  protected override SymbolType MakeArrayTypeCore()
  {
    throw new TypeLoadException();
  }

  protected override SymbolType MakeArrayTypeCore(int rank)
  {
    throw new TypeLoadException();
  }

  protected override SymbolType MakeByRefTypeCore()
  {
    throw new TypeLoadException();
  }

  protected override HybridGenericType MakeGenericTypeCore(Type[] typeArguments)
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericTypeDefinition is true.");
  }

  protected override SymbolType MakeGenericTypeCore(SymbolType[] typeArguments)
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericTypeDefinition is true.");
  }
}
