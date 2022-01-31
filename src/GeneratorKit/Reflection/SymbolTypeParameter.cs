using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolTypeParameter : SymbolType
{
  private readonly ITypeParameterSymbol _symbol;

  public SymbolTypeParameter(GeneratorRuntime runtime, ITypeParameterSymbol symbol)
    : base(runtime)
  {
    _symbol = symbol;
  }

  public override ITypeSymbol Symbol => _symbol;

  // System.Type overrides

  protected override SymbolType? BaseTypeCore => _runtime.CreateTypeDelegator(BaseTypeSymbol);

  public override MethodBase? DeclaringMethod => _symbol.DeclaringMethod is not null
    ? _runtime.CreateMethodInfoDelegator(_symbol.DeclaringMethod)
    : null;

  public override GenericParameterAttributes GenericParameterAttributes
  {
    get
    {
      GenericParameterAttributes result = _symbol.Variance switch
      {
        VarianceKind.None => GenericParameterAttributes.None,
        VarianceKind.Out  => GenericParameterAttributes.Covariant,
        VarianceKind.In   => GenericParameterAttributes.Contravariant,
        _                 => throw Errors.Unreacheable
      };

      if (_symbol.HasReferenceTypeConstraint)
        result |= GenericParameterAttributes.ReferenceTypeConstraint;
      if (_symbol.HasValueTypeConstraint)
        result |= GenericParameterAttributes.NotNullableValueTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint;
      if (_symbol.HasConstructorConstraint)
        result |= GenericParameterAttributes.DefaultConstructorConstraint;

      return result;
    }
  }

  public override int GenericParameterPosition => _symbol.Ordinal;

  public override bool IsConstructedGenericType => false;

  public override bool IsGenericParameter => true;

  public override bool IsEnum => !_symbol.IsValueType && _symbol.ConstraintTypes.Contains(_runtime.Compilation.GetSpecialType(SpecialType.System_Enum));

  public override bool IsSerializable => IsEnum;

  public override MemberTypes MemberType => MemberTypes.TypeInfo;

  public override string Name => Symbol.MetadataName;

  public override int GetArrayRank()
  {
    throw new ArgumentException("Must be an array type.");
  }

  public override IList<CustomAttributeData> GetCustomAttributesData()
  {
    return Array.Empty<CustomAttributeData>();
  }

  protected override TypeAttributes GetAttributeFlagsImpl()
  {
    return TypeAttributes.Public;
  }

  public override Array GetEnumValues()
  {
    throw new InvalidOperationException();
  }

  protected override bool HasElementTypeImpl()
  {
    return false;
  }

  protected override bool IsArrayImpl()
  {
    return false;
  }

  protected override bool IsByRefImpl()
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
    return _symbol.IsValueType || IsEnum;
  }


  // GeneratorRuntimeType overrides

  protected override SymbolNamedType RuntimeDefinition => throw new InvalidOperationException();

  protected override Type[] RuntimeTypeArguments => throw new InvalidOperationException();

  protected override IRuntimeType? RuntimeBaseType => throw new InvalidOperationException();


  // SymbolTypeBase overrides

  protected override SymbolAssembly AssemblyCore => _runtime.CreateAssemblyDelegator(_symbol.ContainingAssembly);

  protected override SymbolModule ModuleCore => _runtime.CreateModuleDelegator(_symbol.ContainingModule);

  public override string Namespace => _symbol.ContainingNamespace.ToDisplayString(s_namespaceFormat);

  protected override SymbolType? GetElementTypeCore()
  {
    return null;
  }

  protected override SymbolType GetEnumUnderlyingTypeCore()
  {
    throw new InvalidOperationException();
  }

  protected override SymbolType[] GetGenericArgumentsCore()
  {
    return Array.Empty<SymbolType>();
  }

  protected override SymbolType[] GetGenericParameterConstraintsCore()
  {
    IEnumerable<ITypeSymbol> constraintTypes = _symbol.ConstraintTypes;
    if (_symbol.HasValueTypeConstraint)
      constraintTypes = constraintTypes.Concat(new[] { _runtime.Compilation.GetSpecialType(SpecialType.System_ValueType) });

    return constraintTypes
      .Select(x => _runtime.CreateTypeDelegator(x))
      .ToArray();
  }

  protected override SymbolType GetGenericTypeDefinitionCore()
  {
    throw new InvalidOperationException("This operation is only valid on generic types.");
  }

  protected override SymbolType[] GetInterfacesCore()
  {
    return GetInterfaceSymbols().Select(x => _runtime.CreateTypeDelegator(x)).ToArray();

    IEnumerable<ITypeSymbol> GetInterfaceSymbols()
    {
      foreach (ITypeSymbol constraintType in _symbol.ConstraintTypes)
      {
        if (constraintType.TypeKind is TypeKind.Interface)
        {
          yield return constraintType;
        }
        foreach (ITypeSymbol constraintInterfaces in constraintType.AllInterfaces)
        {
          yield return constraintInterfaces;
        }
      }
    }
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

  private ITypeSymbol BaseTypeSymbol => _symbol.HasValueTypeConstraint
    ? _runtime.Compilation.GetSpecialType(SpecialType.System_ValueType)
    : _symbol.ConstraintTypes.Length != 0
      ? _symbol.ConstraintTypes[0]
      : _runtime.Compilation.GetSpecialType(SpecialType.System_Object);
}