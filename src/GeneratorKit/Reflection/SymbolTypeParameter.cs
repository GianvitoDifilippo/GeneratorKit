using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolTypeParameter : SymbolType
{
  public SymbolTypeParameter(IReflectionRuntime runtime, IGeneratorContext context, ITypeParameterSymbol symbol)
    : base(runtime, context)
  {
    Symbol = symbol;
  }

  public new ITypeParameterSymbol Symbol { get; }

  public override INamedTypeSymbol OriginalSymbol => throw new InvalidOperationException();

  protected override ITypeSymbol SymbolCore => Symbol;


  // System.Type overrides

  public override bool ContainsGenericParameters => true;

  public override MethodBase? DeclaringMethod => Symbol.DeclaringMethod is not null
    ? Context.CreateMethodInfoDelegator(Symbol.DeclaringMethod)
    : null;

  public override GenericParameterAttributes GenericParameterAttributes
  {
    get
    {
      GenericParameterAttributes result = Symbol.Variance switch
      {
        VarianceKind.None => GenericParameterAttributes.None,
        VarianceKind.Out  => GenericParameterAttributes.Covariant,
        VarianceKind.In   => GenericParameterAttributes.Contravariant,
        _                 => throw Errors.Unreacheable
      };

      if (Symbol.HasReferenceTypeConstraint)
        result |= GenericParameterAttributes.ReferenceTypeConstraint;
      if (Symbol.HasValueTypeConstraint)
        result |= GenericParameterAttributes.NotNullableValueTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint;
      if (Symbol.HasConstructorConstraint)
        result |= GenericParameterAttributes.DefaultConstructorConstraint;

      return result;
    }
  }

  public override int GenericParameterPosition => Symbol.Ordinal;

  public override bool IsConstructedGenericType => false;

  public override bool IsGenericParameter => true;

  public override bool IsEnum => !Symbol.IsValueType && Symbol.ConstraintTypes.Contains(Runtime.Compilation.GetSpecialType(SpecialType.System_Enum));

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

  public override Type[] GetGenericArguments()
  {
    return Array.Empty<SymbolType>();
  }

  public override Type[] GetGenericParameterConstraints()
  {
    IEnumerable<ITypeSymbol> constraintTypes = Symbol.ConstraintTypes;
    if (Symbol.HasValueTypeConstraint)
      constraintTypes = constraintTypes.Concat(new[] { Runtime.Compilation.GetSpecialType(SpecialType.System_ValueType) });

    return constraintTypes.Select(Context.GetContextType).ToArray();
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
    return Symbol.IsValueType || IsEnum;
  }


  // SymbolTypeBase overrides

  protected override SymbolAssembly AssemblyCore => Context.CreateAssemblyDelegator(Symbol.ContainingAssembly);

  protected override SymbolType? BaseTypeCore => Context.CreateTypeDelegator(BaseTypeSymbol);

  protected override SymbolModule ModuleCore => Context.CreateModuleDelegator(Symbol.ContainingModule);

  public override string Namespace => Symbol.ContainingNamespace.ToDisplayString(s_namespaceFormat);

  protected override SymbolType? GetElementTypeCore()
  {
    return null;
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
    return GetInterfaceSymbols().Select(x => Context.CreateTypeDelegator(x)).ToArray();

    IEnumerable<ITypeSymbol> GetInterfaceSymbols()
    {
      foreach (ITypeSymbol constraintType in Symbol.ConstraintTypes)
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
    return Context.CreateTypeDelegator(Runtime.Compilation.CreateArrayTypeSymbol(Symbol));
  }

  protected override SymbolType MakeArrayTypeCore(int rank)
  {
    if (rank == 1)
      throw new NotSupportedException("MDArrays of rank 1 are currently not supported.");

    return Context.CreateTypeDelegator(Runtime.Compilation.CreateArrayTypeSymbol(Symbol, rank));
  }

  protected override SymbolType MakeByRefTypeCore()
  {
    return new SymbolByRefType(Runtime, Context, this);
  }

  protected override SymbolType MakeGenericTypeCore(Type[] typeArguments)
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericTypeDefinition is true.");
  }

  private ITypeSymbol BaseTypeSymbol => Symbol.HasValueTypeConstraint
    ? Runtime.Compilation.GetSpecialType(SpecialType.System_ValueType)
    : Symbol.ConstraintTypes.Length != 0
      ? Symbol.ConstraintTypes[0]
      : Runtime.Compilation.GetSpecialType(SpecialType.System_Object);
}