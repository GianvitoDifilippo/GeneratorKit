using GeneratorKit.Comparers;
using GeneratorKit.Reflection.Context;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolNamedType : SymbolType
{
  public SymbolNamedType(IReflectionContext context, INamedTypeSymbol symbol)
    : base(context)
  {
    Symbol = symbol;
  }

  public new INamedTypeSymbol Symbol { get; }

  public bool IsAnonymousType => Symbol.IsAnonymousType;

  protected override ITypeSymbol SymbolCore => Symbol;


  // System.Type overrides

  public override bool ContainsGenericParameters => Context.ContainsGenericParameters(Symbol);

  public override MethodBase? DeclaringMethod => throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override GenericParameterAttributes GenericParameterAttributes => throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override int GenericParameterPosition => throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");

  public override bool IsConstructedGenericType => IsGenericType && !IsGenericTypeDefinition;

  public override bool IsEnum => Symbol.EnumUnderlyingType is not null;

  public override bool IsGenericParameter => false;

  public override bool IsGenericType => Symbol.IsGenericType;

  public override bool IsGenericTypeDefinition => Context.IsGenericTypeDefinition(Symbol);

  public override bool IsSerializable
  {
    get
    {
      if (Symbol.TypeKind is TypeKind.Enum) return true;

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

      return Symbol.GetAttributes().Any(x => x.AttributeClass is not null && x.AttributeClass.ToDisplayString() == "System.SerializableAttribute");
    }
  }

  public override MemberTypes MemberType => Symbol.ContainingType is null
    ? MemberTypes.TypeInfo
    : MemberTypes.NestedType;

  public override string Name => Symbol.MetadataName;

  public override string Namespace => Symbol.ContainingNamespace.ToDisplayString(s_namespaceFormat);

  public override int GetArrayRank()
  {
    throw new ArgumentException("Must be an array type.");
  }

  protected override TypeAttributes GetAttributeFlagsImpl()
  {
    TypeAttributes result = default;
    if (Symbol.ContainingType is not null)
    {
      result |= Symbol.DeclaredAccessibility switch
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
    else if (Symbol.DeclaredAccessibility is Accessibility.Public)
      result |= TypeAttributes.Public;

    if (Symbol.TypeKind == TypeKind.Interface)
      result |= TypeAttributes.Interface;
    if (Symbol.IsAbstract)
      result |= TypeAttributes.Abstract;
    if (Symbol.IsSealed)
      result |= TypeAttributes.Sealed;
    if (Symbol.IsSerializable)
      result |= TypeAttributes.Serializable;
    if (Symbol.TypeKind != TypeKind.Interface && !Symbol.StaticConstructors.Any() && Symbol.TypeKind is not TypeKind.Enum)
      result |= TypeAttributes.BeforeFieldInit;
    if (Symbol.IsValueType && Symbol.TypeKind is not TypeKind.Enum)
      result |= TypeAttributes.SequentialLayout;

    return result;
  }

  public override IList<CustomAttributeData> GetCustomAttributesData()
  {
    List<CustomAttributeData> result = Symbol
      .GetAttributes()
      .Select(data => CompilationCustomAttributeData.FromAttributeData(Context, data) as CustomAttributeData)
      .ToList();

    if (Symbol.MemberNames.Any(x => x is "this[]"))
    {
      INamedTypeSymbol defaultMemberAttributeSymbol = Context.Compilation.GetTypeByMetadataName("System.Reflection.DefaultMemberAttribute")!;
      IMethodSymbol constructor = defaultMemberAttributeSymbol.InstanceConstructors[0];
      CustomAttributeTypedArgument[] arguments = new[] { new CustomAttributeTypedArgument(typeof(string), "Item") };
      result.Add(CompilationCustomAttributeData.FromSymbol(Context, constructor, arguments, Array.Empty<CustomAttributeNamedArgument>()));
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

    foreach (ISymbol member in Symbol.GetMembers().Where(x => x.Kind == SymbolKind.Field))
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

    return Symbol
      .GetMembers()
      .Where(x => x.Kind == SymbolKind.Field)
      .Cast<IFieldSymbol>()
      .OrderBy(x => ToUInt64(x.ConstantValue!))
      .Select(x => x.Name)
      .ToArray();
  }

  public override Array GetEnumValues()
  {
    if (!IsEnum)
      throw new InvalidOperationException();

    return UnderlyingSystemType.GetEnumValues();
  }

  public override Type[] GetGenericArguments()
  {
    return Symbol.TypeArguments.Map(Context.GetContextType);
  }

  public override Type[] GetGenericParameterConstraints()
  {
    throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericParameter is true.");
  }

  protected override bool HasElementTypeImpl()
  {
    return false;
  }

  protected override bool IsArrayImpl()
  {
    return false;
  }

  protected override bool IsAssignableFromCore(Type? c)
  {
    if (c is null)
      return false;

    if (TypeEqualityComparer.Default.Equals(this, c))
      return true;

    if (IsSuperclassOf(c))
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
      foreach (string name in Symbol.MemberNames)
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
      if (!TypeEqualityComparer.Default.Equals(underlyingType, valueType))
        throw new ArgumentException("Object must be the same type as the enum.", nameof(value));

      ulong ulValue = ToUInt64(value);
      
      foreach (ISymbol member in Symbol.GetMembers().Where(x => x.Kind == SymbolKind.Field))
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

  public override bool IsSubclassOf(Type? c)
  {
    if (c is null)
      return false;

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

  protected override SymbolAssembly AssemblyCore => Context.CreateAssemblyDelegator(Symbol.ContainingAssembly);

  protected override SymbolType? BaseTypeCore => BaseType;

  protected override SymbolModule ModuleCore => Context.CreateModuleDelegator(Symbol.ContainingModule);

  protected override SymbolType? GetElementTypeCore()
  {
    return null;
  }

  protected override SymbolType GetEnumUnderlyingTypeCore()
  {
    if (!IsEnum)
      throw new InvalidOperationException();

    return Context.CreateTypeDelegator(Symbol.EnumUnderlyingType!);
  }

  protected override SymbolNamedType GetGenericTypeDefinitionCore()
  {
    if (!IsGenericType)
      throw new InvalidOperationException("This operation is only valid on generic types.");

    return Context.GetGenericTypeDefinition(this);
  }

  protected override SymbolType[] GetInterfacesCore()
  {
    return Symbol.AllInterfaces.Map(Context.CreateTypeDelegator);
  }

  public override bool IsSuperclassOf(Type? c)
  {
    if (c is null)
      return false;

    if (TypeEqualityComparer.Default.Equals(c, this))
      return false;

    Type? type = c.BaseType;
    while (type is not null)
    {
      if (TypeEqualityComparer.Default.Equals(type, this))
        return true;

      type = type.BaseType;
    }

    return false;
  }

  protected override SymbolType MakeArrayTypeCore()
  {
    return Context.CreateTypeDelegator(Context.Compilation.CreateArrayTypeSymbol(Symbol));
  }

  protected override SymbolType MakeArrayTypeCore(int rank)
  {
    if (rank == 1)
      throw new NotSupportedException("MDArrays of rank 1 are currently not supported.");

    return Context.CreateTypeDelegator(Context.Compilation.CreateArrayTypeSymbol(Symbol, rank));
  }

  protected override SymbolType MakeByRefTypeCore()
  {
    return new SymbolByRefType(Context, this);
  }

  protected override SymbolNamedType MakeGenericTypeCore(Type[] typeArguments)
  {
    if (!IsGenericTypeDefinition)
      throw new InvalidOperationException("Method may only be called on a Type for which Type.IsGenericTypeDefinition is true.");

    return Context.MakeGenericType(this, typeArguments);
  }


  // New members

  public new SymbolNamedType? BaseType => Symbol.BaseType is { } baseType
    ? Context.CreateTypeDelegator(baseType)
    : null;


  // Other members

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
        throw new InvalidOperationException("Invalid object type in ToUInt64");
    }
    return result;
  }
}