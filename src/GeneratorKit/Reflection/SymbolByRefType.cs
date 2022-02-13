using GeneratorKit.Comparers;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal class SymbolByRefType : SymbolType
{
  private readonly SymbolType _elementType;

  public SymbolByRefType(IReflectionRuntime runtime, IGeneratorContext context, SymbolType elementType)
    : base(runtime, context)
  {
    _elementType = elementType;
  }

  public override INamedTypeSymbol OriginalSymbol => throw new InvalidOperationException();

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

  public override Type[] GetGenericArguments()
  {
    return _elementType.GetGenericArguments();
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

  protected override SymbolType GetGenericTypeDefinitionCore()
  {
    throw new InvalidOperationException("This operation is only valid on generic types.");
  }

  protected override SymbolType[] GetInterfacesCore()
  {
    return Array.Empty<SymbolType>();
  }

  protected override bool IsAssignableFromCore(Type c)
  {
    return TypeEqualityComparer.Default.Equals(this, c);
  }

  public override bool IsSuperclassOf(Type? c)
  {
    return false;
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

  protected override SymbolType MakeGenericTypeCore(Type[] typeArguments)
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericTypeDefinition is true.");
  }
}
