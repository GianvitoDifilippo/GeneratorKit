using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal class ProxyTypeBuilderInstantiation : Type
{
  private readonly Type _proxyTypeDefinition;
  private readonly Type[] _genericArguments;
  private readonly int[] _positions;
  private Type? _underlyingSystemType;

  private ProxyTypeBuilderInstantiation(Type proxyTypeDefinition, Type[] genericArguments, int[] positions)
  {
    _proxyTypeDefinition = proxyTypeDefinition;
    _genericArguments = genericArguments;
    _positions = positions;
  }

  public override bool ContainsGenericParameters => _genericArguments.Any(t => t.IsGenericParameter); // TODO: Deep search

  public override string? FullName => null;

  public override bool IsConstructedGenericType => true;

  public override bool IsGenericType => true;

  public override bool IsGenericTypeDefinition => false;

  public override bool IsGenericParameter => false;

  public override string Namespace => _proxyTypeDefinition.Namespace;

  public override string Name => _proxyTypeDefinition.Name;

  public override Type UnderlyingSystemType => _underlyingSystemType ??= GetUnderlyingSystemType();

  public override Type[] GetGenericArguments()
  {
    return _genericArguments;
  }

  public override Type GetGenericTypeDefinition()
  {
    return _proxyTypeDefinition;
  }

  public override Type MakeGenericType(params Type[] typeArguments)
  {
    int length = _genericArguments.Length;
    Type?[] genericArguments = new Type[length];
    for (int i = 0; i < length; i++)
    {
      Type genericArgument = _genericArguments[i];
      genericArguments[i] = genericArgument.IsGenericParameter ? null : genericArgument.UnderlyingSystemType;
    }
    for (int i = 0; i < typeArguments.Length; i++)
    {
      int position = _positions[i];
      if (position != -1)
      {
        genericArguments[position] = typeArguments[i];
      }
    }

    return _proxyTypeDefinition.MakeGenericType(genericArguments);
  }

  public override bool Equals(Type o)
  {
    if (!o.IsGenericType)
      return false;

    if (o.GetGenericTypeDefinition() != _proxyTypeDefinition)
      return false;

    Type[] genericArguments = o.GetGenericArguments();
    if (genericArguments.Length != _genericArguments.Length)
      return false;

    for (int i = 0; i < genericArguments.Length; i++)
    {
      Type genericArgument = genericArguments[i];
      if (genericArgument.IsGenericParameter)
      {
        if (!_genericArguments[i].IsGenericParameter || _positions[genericArgument.GenericParameterPosition] != i)
          return false;
      }
      else
      {
        if (!_genericArguments[i].Equals(genericArgument))
          return false;
      }
    }

    return true;
  }

  public static Type Create(IRuntimeType type, IRuntimeType signatureType, Type proxyTypeDefinition)
  {
    if (!proxyTypeDefinition.IsGenericType)
      return proxyTypeDefinition;

    Type[] typeArguments = type.TypeArguments;
    Type[] signatureArguments = signatureType.TypeArguments;
    if (typeArguments.Length == signatureArguments.Length)
      return proxyTypeDefinition;

    int[] positions = CreatePositions(typeArguments.Length);
    
    for (int i = 0; i < signatureArguments.Length; i++)
    {
      Type genericArgument = signatureArguments[i];
      if (genericArgument.IsGenericParameter)
      {
        positions[genericArgument.GenericParameterPosition] = i;
      }
    }
    return new ProxyTypeBuilderInstantiation(proxyTypeDefinition, signatureArguments, positions!);
  }

  private Type GetUnderlyingSystemType()
  {
    int length = _genericArguments.Length;
    Type[] typeArguments = new Type[length];
    Type[] genericParameters = _proxyTypeDefinition.GetGenericArguments();
    for (int i = 0; i < length; i++)
    {
      Type genericArgument = _genericArguments[i];
      typeArguments[i] = genericArgument.IsGenericParameter
        ? genericParameters[_positions[genericArgument.GenericParameterPosition]]
        : genericArgument.UnderlyingSystemType;
    }
    return _proxyTypeDefinition.MakeGenericType(typeArguments);
  }

  private static int[] CreatePositions(int length)
{
    int[] positions = new int[length];
    for (int i = 0; i < length; i++)
      positions[i] = -1;

    return positions;
  }

  #region Not supported
  public override Assembly Assembly => throw new NotSupportedException();
  public override string? AssemblyQualifiedName => throw new NotSupportedException();
  public override Type BaseType => throw new NotSupportedException();
  public override Guid GUID => throw new NotSupportedException();
  public override Module Module => throw new NotSupportedException();
  public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => throw new NotSupportedException();
  public override object[] GetCustomAttributes(bool inherit) => throw new NotSupportedException();
  public override object[] GetCustomAttributes(Type attributeType, bool inherit) => throw new NotSupportedException();
  public override Type GetElementType() => throw new NotSupportedException();
  public override EventInfo GetEvent(string name, BindingFlags bindingAttr) => throw new NotSupportedException();
  public override EventInfo[] GetEvents(BindingFlags bindingAttr) => throw new NotSupportedException();
  public override FieldInfo GetField(string name, BindingFlags bindingAttr) => throw new NotSupportedException();
  public override FieldInfo[] GetFields(BindingFlags bindingAttr) => throw new NotSupportedException();
  public override Type GetInterface(string name, bool ignoreCase) => throw new NotSupportedException();
  public override Type[] GetInterfaces() => throw new NotSupportedException();
  public override MemberInfo[] GetMembers(BindingFlags bindingAttr) => throw new NotSupportedException();
  public override MethodInfo[] GetMethods(BindingFlags bindingAttr) => throw new NotSupportedException();
  public override Type GetNestedType(string name, BindingFlags bindingAttr) => throw new NotSupportedException();
  public override Type[] GetNestedTypes(BindingFlags bindingAttr) => throw new NotSupportedException();
  public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) => throw new NotSupportedException();
  public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters) => throw new NotSupportedException();
  public override bool IsDefined(Type attributeType, bool inherit) => throw new NotSupportedException();
  protected override TypeAttributes GetAttributeFlagsImpl() => throw new NotSupportedException();
  protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) => throw new NotSupportedException();
  protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) => throw new NotSupportedException();
  protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) => throw new NotSupportedException();
  protected override bool HasElementTypeImpl() => throw new NotSupportedException();
  protected override bool IsArrayImpl() => throw new NotSupportedException();
  protected override bool IsByRefImpl() => throw new NotSupportedException();
  protected override bool IsCOMObjectImpl() => throw new NotSupportedException();
  protected override bool IsPointerImpl() => throw new NotSupportedException();
  protected override bool IsPrimitiveImpl() => throw new NotSupportedException();
  #endregion
}