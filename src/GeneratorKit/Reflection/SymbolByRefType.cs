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

  public override ITypeSymbol Symbol => _elementType.Symbol;


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

  public override Type MakeGenericType(params Type[] typeArguments)
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericTypeDefinition is true.");
  }


  // SymbolType overrides

  public override SymbolType MakeGenericType(params SymbolType[] typeArguments)
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericTypeDefinition is true.");
  }


  // SymbolTypeBase overrides

  protected override SymbolAssembly AssemblyCore => _elementType.Assembly;

  protected override SymbolType? BaseTypeCore => null;

  protected override SymbolModule ModuleCore => _elementType.Module;

  protected override SymbolType? GetElementTypeCore()
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
}
