using GeneratorKit.Comparers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GeneratorKit.Reflection;

internal sealed class SymbolNamedType : SymbolType
{
  private readonly INamedTypeSymbol _symbol;

  public SymbolNamedType(GeneratorRuntime runtime,  INamedTypeSymbol symbol)
    : base(runtime)
  {
    _symbol = symbol;
  }

  public override ITypeSymbol Symbol => _symbol;


  // System.Type overrides

  public override bool ContainsGenericParameters =>
    _symbol.IsGenericType &&
    _symbol.TypeArguments.Any(x => x.TypeKind is TypeKind.TypeParameter);

  public override MethodBase? DeclaringMethod => throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");

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

  public override GenericParameterAttributes GenericParameterAttributes => throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override int GenericParameterPosition => throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override bool IsConstructedGenericType => _symbol.TypeArguments.Any(x => x.TypeKind is not TypeKind.TypeParameter);

  public override bool IsEnum => _symbol.EnumUnderlyingType is not null;

  public override bool IsGenericParameter => false;

  public override bool IsGenericType => _symbol.IsGenericType;

  public override bool IsGenericTypeDefinition => _symbol.IsGenericType && _symbol.IsDefinition;

  public override bool IsSerializable
  {
    get
    {
      if (_symbol.TypeKind is TypeKind.Enum) return true;

      if (IsPrimitive) return true;

      if (FullName is
        "System.DateTime" or
        "System.DateTimeOffset" or
        "System.Decimal" or
        "System.Enum" or
        "System.Guid" or
        "System.Object" or
        "System.String" or
        "System.Uri" or
        "System.Xml.XmlQualifiedName") return true;

      return _symbol.GetAttributes().Any(x => x.AttributeClass is not null && x.AttributeClass.ToDisplayString() == "System.SerializableAttribute");
    }
  }

  public override MemberTypes MemberType => Symbol.ContainingType is null
    ? MemberTypes.TypeInfo
    : MemberTypes.NestedType;

  public override string Name => Symbol.MetadataName;

  public override string Namespace => _symbol.ContainingNamespace.ToDisplayString(s_namespaceFormat);

  public override int GetArrayRank()
  {
    throw new ArgumentException("Must be an array type.");
  }

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
      .Select(x => (CustomAttributeData)CompilationCustomAttributeData.FromAttributeData(_runtime, x))
      .ToList();

    if (_symbol.MemberNames.Any(x => x is "this[]"))
    {
      INamedTypeSymbol defaultMember = _runtime.Compilation.GetTypeByMetadataName("System.Reflection.DefaultMemberAttribute")!;
      CustomAttributeTypedArgument[] arguments = new[] { new CustomAttributeTypedArgument(typeof(string), "Item") };
      result.Add(CompilationCustomAttributeData.FromSymbol(_runtime, defaultMember.InstanceConstructors[0], arguments, Array.Empty<CustomAttributeNamedArgument>()));
    }

    return new ReadOnlyCollection<CustomAttributeData>(result);
  }

  public override string? GetEnumName(object value)
  {
    if (!IsEnum)
      throw new InvalidOperationException();

    if (value is null)
      throw new ArgumentNullException(nameof(value));

    Type valueType = value.GetType();

    if (!(valueType.IsEnum || IsIntegerType(valueType)))
      throw new ArgumentException();

    ulong ulValue = ToUInt64(value);

    foreach (ISymbol member in _symbol.GetMembers().Where(x => x.Kind == SymbolKind.Field))
    {
      IFieldSymbol field = (IFieldSymbol)member;
      ulong memberValue = Convert.ToUInt64(field.ConstantValue);
      if (ulValue == memberValue)
      {
        return member.Name;
      }
    }
    return null;
  }

  public override string[] GetEnumNames()
  {
    if (!IsEnum)
      throw new InvalidOperationException();

    return _symbol
      .GetMembers()
      .Where(x => x.Kind == SymbolKind.Field)
      .Cast<IFieldSymbol>()
      .OrderBy(x => ToUInt64(x.ConstantValue!))
      .Select(x => x.Name)
      .ToArray();
  }

  public override Type GetEnumUnderlyingType()
  {
    if (!IsEnum)
      throw new InvalidOperationException();

    return _runtime.CreateTypeDelegator(_symbol.EnumUnderlyingType!).GetRuntimeTypeOrThrow();
  }

  public override Array GetEnumValues()
  {
    if (!IsEnum)
      throw new InvalidOperationException();

    return GetRuntimeTypeOrThrow().GetEnumValues();
  }

  protected override SymbolType GetGenericTypeDefinitionCore()
  {
    if (!IsGenericType)
      throw new InvalidOperationException("This operation is only valid on generic types.");

    return _runtime.CreateTypeDelegator(_symbol.OriginalDefinition);
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

  protected sealed override bool IsByRefImpl()
  {
    return false;
  }

  public override bool IsEnumDefined(object value)
  {
    if (!IsEnum)
      throw new InvalidOperationException();

    Type valueType = value.GetType();

    if (valueType.IsEnum)
    {
      if (!TypeEqualityComparer.Default.Equals(this, valueType))
        throw new ArgumentException("Object must be the same type as the enum.", nameof(value));

      valueType = valueType.GetEnumUnderlyingType();
    }

    if (valueType == typeof(string))
    {
      foreach (string name in _symbol.MemberNames)
      {
        string stringValue = (string)value;
        if (name == stringValue)
        {
          return true;
        }
      }
      return false;
    }

    if (IsIntegerType(valueType))
    {
      Type underlyingType = GetEnumUnderlyingType();
      if (underlyingType != valueType)
        throw new ArgumentException("Object must be the same type as the enum.", nameof(value));

      ulong ulValue = ToUInt64(value);
      
      foreach (ISymbol member in _symbol.GetMembers().Where(x => x.Kind == SymbolKind.Field))
      {
        IFieldSymbol field = (IFieldSymbol)member;
        ulong memberValue = Convert.ToUInt64(field.ConstantValue);
        if (ulValue == memberValue)
        {
          return true;
        }
      }

      return false;
    }
    else
    {
      throw new InvalidOperationException("Unknown enum type");
    }
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


  // SymbolTypeBase overrides

  protected override SymbolAssembly AssemblyCore => _runtime.CreateAssemblyDelegator(_symbol.ContainingAssembly);

  protected override SymbolType? BaseTypeCore => _symbol.BaseType is { } baseType
    ? _runtime.CreateTypeDelegator(baseType)
    : null;

  protected override SymbolModule ModuleCore => _runtime.CreateModuleDelegator(_symbol.ContainingModule);

  protected override SymbolType[] GenericTypeArgumentsCore => _symbol.TypeArguments
    .Where(x => x.TypeKind is not TypeKind.TypeParameter)
    .Select(x => _runtime.CreateTypeDelegator(x))
    .ToArray();

  protected override SymbolType? GetElementTypeCore()
  {
    return null;
  }

  protected override SymbolType[] GetGenericArgumentsCore()
  {
    return _symbol.TypeArguments
      .Select(x => _runtime.CreateTypeDelegator(x))
      .ToArray();
  }

  protected override SymbolType[] GetGenericParameterConstraintsCore()
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");
  }

  protected sealed override SymbolType[] GetInterfacesCore()
  {
    return _symbol.AllInterfaces.Select(x => _runtime.CreateTypeDelegator(x)).ToArray();
  }

  protected sealed override SymbolType MakeGenericTypeCore(params Type[] typeArguments)
  {
    if (!IsGenericTypeDefinition)
      throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericTypeDefinition is true.");

    ITypeSymbol[] symbolTypeArguments = typeArguments
      .Select(x => _runtime.GetTypeSymbol(x) ?? throw new InvalidOperationException($"Could not resolve symbol for type {x.FullName}"))
      .ToArray();

    return _runtime.CreateTypeDelegator(_symbol.Construct(symbolTypeArguments));
  }

  private static bool IsIntegerType(Type t)
  {
    return
      t == typeof(int)    ||
      t == typeof(short)  ||
      t == typeof(ushort) ||
      t == typeof(byte)   ||
      t == typeof(sbyte)  ||
      t == typeof(uint)   ||
      t == typeof(long)   ||
      t == typeof(ulong)  ||
      t == typeof(char)   ||
      t == typeof(bool);
  }

  static ulong ToUInt64(object value)
  {
    TypeCode typeCode = Convert.GetTypeCode(value);
    ulong result;

    switch (typeCode)
    {
      case TypeCode.SByte:
      case TypeCode.Int16:
      case TypeCode.Int32:
      case TypeCode.Int64:
        result = (ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture);
        break;

      case TypeCode.Byte:
      case TypeCode.UInt16:
      case TypeCode.UInt32:
      case TypeCode.UInt64:
      case TypeCode.Boolean:
      case TypeCode.Char:
        result = Convert.ToUInt64(value, CultureInfo.InvariantCulture);
        break;

      default:
        Contract.Assert(false, "Invalid Object type in ToUInt64");
        throw new InvalidOperationException();
    }
    return result;
  }
}