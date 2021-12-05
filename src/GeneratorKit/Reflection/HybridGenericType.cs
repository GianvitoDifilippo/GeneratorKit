using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal class HybridGenericType : Type
{
  private readonly GeneratorRuntime _runtime;
  private readonly SymbolNamedType _definitionType;
  private readonly Type[] _typeArguments;

  public HybridGenericType(GeneratorRuntime runtime, SymbolNamedType definitionType, Type[] typeArguments)
  {
    _runtime = runtime;
    _definitionType = definitionType;
    _typeArguments = typeArguments;
  }

  public override Assembly Assembly => _definitionType.Assembly;

  public override string? AssemblyQualifiedName => TypeNameBuilder.ToString(this, TypeNameBuilder.Format.AssemblyQualifiedName);

  public override Type BaseType
  {
    get
    {
      SymbolType definitionBaseType = _definitionType.BaseType!;

      if (!definitionBaseType.IsGenericType)
        return definitionBaseType;

      ImmutableArray<ITypeSymbol> baseTypeArgumentSymbols = ((INamedTypeSymbol)definitionBaseType.Symbol).TypeArguments;
      Type[] baseTypeArguments = baseTypeArgumentSymbols.Select(x => x.TypeKind is TypeKind.TypeParameter
        ? _typeArguments[((ITypeParameterSymbol)x).Ordinal]
        : _runtime.CreateTypeDelegator(x))
        .ToArray();

      return definitionBaseType.MakeGenericType(baseTypeArguments);
    }
  }

  public override bool ContainsGenericParameters => _typeArguments.Any(x => x.IsGenericParameter);

  public override string? FullName => TypeNameBuilder.ToString(this, TypeNameBuilder.Format.FullName);

  public override Type[] GenericTypeArguments => _typeArguments;

  public override Guid GUID => throw new NotSupportedException();

  public override bool IsGenericType => true;

  public override bool IsGenericParameter => false;

  public override bool IsGenericTypeDefinition => false;

  public override Module Module => _definitionType.Module;

  public override string Namespace => _definitionType.Namespace;

  public override Type UnderlyingSystemType
  {
    get
    {
      Type? runtimeDefinitionType = _runtime.GetRuntimeType(_definitionType);
      if (runtimeDefinitionType is null)
        return this;

      Type[] typeArguments = _typeArguments
        .Select(x => x is SymbolType symbolType ? _runtime.GetRuntimeType(symbolType) ?? symbolType : x)
        .ToArray();
      return runtimeDefinitionType.MakeGenericType(typeArguments);
    }
  }

  public override string Name => _definitionType.Name;

  public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
  {
    throw new NotSupportedException();
  }

  public override object[] GetCustomAttributes(bool inherit)
  {
    throw new NotSupportedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit)
  {
    throw new NotSupportedException();
  }

  public override Type GetElementType()
  {
    throw new NotSupportedException();
  }

  public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
  {
    throw new NotSupportedException();
  }

  public override EventInfo[] GetEvents(BindingFlags bindingAttr)
  {
    throw new NotSupportedException();
  }

  public override FieldInfo GetField(string name, BindingFlags bindingAttr)
  {
    throw new NotSupportedException();
  }

  public override FieldInfo[] GetFields(BindingFlags bindingAttr)
  {
    throw new NotSupportedException();
  }

  public override Type[] GetGenericArguments()
  {
    return _typeArguments;
  }

  public override Type GetInterface(string name, bool ignoreCase)
  {
    throw new NotSupportedException();
  }

  public override Type[] GetInterfaces()
  {
    throw new NotSupportedException();
  }

  public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
  {
    throw new NotSupportedException();
  }

  public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
  {
    throw new NotSupportedException();
  }

  public override Type GetNestedType(string name, BindingFlags bindingAttr)
  {
    throw new NotSupportedException();
  }

  public override Type[] GetNestedTypes(BindingFlags bindingAttr)
  {
    throw new NotSupportedException();
  }

  public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
  {
    throw new NotSupportedException();
  }

  public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
  {
    throw new NotSupportedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotSupportedException();
  }

  protected override TypeAttributes GetAttributeFlagsImpl()
  {
    throw new NotSupportedException();
  }

  protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
  {
    throw new NotSupportedException();
  }

  protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
  {
    throw new NotSupportedException();
  }

  protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
  {
    throw new NotSupportedException();
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

  protected override bool IsCOMObjectImpl()
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

  public override string? ToString()
  {
    return TypeNameBuilder.ToString(this, TypeNameBuilder.Format.ToString);
  }
}