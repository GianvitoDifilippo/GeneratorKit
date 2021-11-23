using GeneratorKit.Comparers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GeneratorKit.Reflection;

internal sealed class SymbolNamedType : SymbolType
{
  private readonly INamedTypeSymbol _symbol;

  public SymbolNamedType(GeneratorRuntime runtime, Compilation compilation, INamedTypeSymbol symbol)
    : base(runtime, compilation)
  {
    _symbol = symbol;
  }

  public override ITypeSymbol Symbol => _symbol;


  public override bool ContainsGenericParameters =>
    _symbol.IsGenericType &&
    _symbol.TypeArguments.Any(x => x.TypeKind is TypeKind.TypeParameter);

  public override MethodBase? DeclaringMethod => throw new InvalidOperationException($"Method may only be called on a {nameof(Type)} for which {nameof(Type)}.{nameof(IsGenericParameter)} is true.");

  public override string? FullName
  {
    get
    {
      StringBuilder builder = new StringBuilder(_symbol.ContainingNamespace.ToString());
      builder.Append('.');
      INamedTypeSymbol? symbol = _symbol.ContainingType;
      while (symbol is not null)
      {
        builder.Append(symbol.MetadataName);
        builder.Append('+');
        symbol = symbol.ContainingType;
      }
      builder.Append(_symbol.MetadataName);
      
      if (!_symbol.IsGenericType || _symbol.TypeArguments.Any(arg => arg.TypeKind is TypeKind.Error or TypeKind.TypeParameter))
        return builder.ToString();

      builder.Append('[');
      builder.Append('[');
      builder.Append(_runtime.CreateTypeDelegator(_symbol.TypeArguments[0]).AssemblyQualifiedName);
      builder.Append(']');
      for (int i = 1; i < _symbol.TypeArguments.Length; i++)
      {
        builder.Append(',');
        builder.Append('[');
        builder.Append(_runtime.CreateTypeDelegator(_symbol.TypeArguments[i]).AssemblyQualifiedName);
        builder.Append(']');
      }
      builder.Append(']');

      return builder.ToString();
    }
  }

  public override Type[] GenericTypeArguments => _symbol.TypeArguments
    .Where(x => x.TypeKind is not TypeKind.TypeParameter)
    .Select(x => _runtime.CreateTypeDelegator(x))
    .ToArray();

  public override bool IsConstructedGenericType => _symbol.TypeArguments.Any(x => x.TypeKind is not TypeKind.TypeParameter);

  public override bool IsEnum => _symbol.EnumUnderlyingType is not null;

  public override bool IsGenericType => _symbol.IsGenericType;

  public override bool IsGenericTypeDefinition => _symbol.IsGenericType && _symbol.IsDefinition;

  public override string Name => Symbol.MetadataName;

  protected override TypeAttributes GetAttributeFlagsImpl()
  {
    TypeAttributes result = default;
    if (_symbol.ContainingType is not null)
    {
      result |= _symbol.DeclaredAccessibility switch
      {
        Accessibility.Public               => TypeAttributes.NestedPublic,
        Accessibility.ProtectedOrInternal  => TypeAttributes.NestedFamORAssem,
        Accessibility.Protected            => TypeAttributes.NestedFamily,
        Accessibility.Internal             => TypeAttributes.NestedAssembly,
        Accessibility.ProtectedAndInternal => TypeAttributes.NestedFamANDAssem,
        Accessibility.Private              => TypeAttributes.NestedPrivate,
        _                                  => default
      };
    }
    else if (_symbol.DeclaredAccessibility is Accessibility.Public)
      result |= TypeAttributes.Public;

    if (_symbol.TypeKind == TypeKind.Interface)
      result |= TypeAttributes.Interface;
    if (_symbol.IsAbstract)
      result |= TypeAttributes.Abstract;
    if (_symbol.IsSealed)
      result |= TypeAttributes.Sealed;
    if (_symbol.IsSerializable)
      result |= TypeAttributes.Serializable;
    if (_symbol.TypeKind != TypeKind.Interface && !_symbol.StaticConstructors.Any() && _symbol.TypeKind is not TypeKind.Enum)
      result |= TypeAttributes.BeforeFieldInit;
    if (_symbol.IsValueType && _symbol.TypeKind is not TypeKind.Enum)
      result |= TypeAttributes.SequentialLayout;

    return result;
  }

  public override IList<CustomAttributeData> GetCustomAttributesData()
  {
    List<CustomAttributeData> result = _symbol
      .GetAttributes()
      .Select(x => (CustomAttributeData)new CompilationCustomAttributeData(_runtime, x))
      .ToList();

    if (_symbol.MemberNames.Any(x => x is "this[]"))
    {
      INamedTypeSymbol defaultMember = _compilation.GetTypeByMetadataName("System.Reflection.DefaultMemberAttribute")!;
      result.Add(new ConstructedCustomAttributeData(
        _runtime.CreateConstructorInfoDelegator(defaultMember.InstanceConstructors[0]),
        new[] { new CustomAttributeTypedArgument(typeof(string), "Item") },
        Array.Empty<CustomAttributeNamedArgument>()));
    }

    if (_symbol.Equals(_compilation.GetSpecialType(SpecialType.System_Int32), SymbolEqualityComparer.Default))
    {
      AddAttributeData("System.SerializableAttribute");
      AddAttributeData("System.Runtime.CompilerServices.IsReadOnlyAttribute");
    }
    else if (_symbol.Equals(_compilation.GetSpecialType(SpecialType.System_Enum), SymbolEqualityComparer.Default))
    {
      AddAttributeData("System.SerializableAttribute");
    }
    else if (_symbol.Equals(_compilation.GetSpecialType(SpecialType.System_Object), SymbolEqualityComparer.Default))
    {
      AddAttributeData("System.SerializableAttribute");
    }

    return new ReadOnlyCollection<CustomAttributeData>(result);

    void AddAttributeData(string fullyQualifiedMetadataName)
    {
      if (_compilation.GetTypeByMetadataName(fullyQualifiedMetadataName) is not INamedTypeSymbol symbol)
      {
        throw new ArgumentException($"Cannot find symbol of name {fullyQualifiedMetadataName}", nameof(fullyQualifiedMetadataName));
      }
      result.Add(ConstructedCustomAttributeData.CreateParameterlessAttribute(_runtime, symbol));
    }
  }

  public override Type? GetElementType()
  {
    return null;
  }

  public override Type[] GetGenericArguments()
  {
    return _symbol.TypeArguments
      .Select(x => _runtime.CreateTypeDelegator(x))
      .ToArray();
  }

  protected override bool HasElementTypeImpl()
  {
    return false;
  }

  protected override bool IsArrayImpl()
  {
    return false;
  }

  public override bool IsAssignableFrom(Type c)
  {
    if (c is null)
      return false;

    if (TypeEqualityComparer.Default.Equals(this, c))
      return true;

    if (c.IsSubclassOf(this))
      return true;

    if (IsInterface)
    {
      return c.GetInterfaces().Any(x => TypeEqualityComparer.Default.Equals(this, x));
    }

    return false;
  }

  protected override bool IsPointerImpl()
  {
    return false;
  }

  protected override bool IsPrimitiveImpl()
  {
    return Namespace is "System" && Name is
      "Boolean" or
      "Char"    or
      "SByte"   or
      "Byte"    or
      "Int16"   or
      "UInt16"  or
      "Int32"   or
      "UInt32"  or
      "Int64"   or
      "UInt64"  or
      "Single"  or
      "Double"  or
      "IntPtr"  or
      "UIntPtr";
  }

  public override bool IsSubclassOf(Type c)
  {
    if (TypeEqualityComparer.Default.Equals(this, c))
      return false;

    Type? type = BaseType;
    while (type is not null)
    {
      if (TypeEqualityComparer.Default.Equals(type, c))
        return true;

      type = type.BaseType;
    }

    return false;
  }

  protected override bool IsValueTypeImpl()
  {
    return Symbol.TypeKind is TypeKind.Enum or TypeKind.Struct;
  }
}