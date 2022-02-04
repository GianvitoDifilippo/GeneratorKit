using GeneratorKit.Comparers;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal class HybridGenericType : HybridGenericTypeBase
{
  private readonly GeneratorRuntime _runtime;
  private readonly SymbolNamedType _definition;
  private readonly Type[] _typeArguments;
  private Type? _runtimeType;

  public HybridGenericType(GeneratorRuntime runtime, SymbolNamedType definition, Type[] typeArguments)
  {
    _runtime = runtime;
    _definition = definition;
    _typeArguments = typeArguments;
  }


  // Systm.Type overrides

  public override string? AssemblyQualifiedName => TypeNameBuilder.ToString(this, TypeNameBuilder.Format.AssemblyQualifiedName);

  public override bool ContainsGenericParameters => _typeArguments.Any(t => t.IsGenericParameter); // TODO: Check deep

  public override string? FullName => TypeNameBuilder.ToString(this, TypeNameBuilder.Format.FullName);

  public override Guid GUID => throw new NotImplementedException();

  public override bool IsConstructedGenericType => true;

  public override bool IsGenericType => true;

  public override bool IsGenericParameter => false;

  public override bool IsGenericTypeDefinition => false;

  public override string Name => _definition.Name;

  public override string Namespace => _definition.Namespace;

  public override Type UnderlyingSystemType => RuntimeType.UnderlyingSystemType;

  public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(bool inherit)
  {
    return _definition.GetCustomAttributes(inherit);
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit)
  {
    return _definition.GetCustomAttributes(attributeType, inherit);
  }

  public override Type? GetElementType()
  {
    return null;
  }

  public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  public override EventInfo[] GetEvents(BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  public override FieldInfo GetField(string name, BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  public override FieldInfo[] GetFields(BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  public override Type[] GetGenericArguments()
  {
    return _typeArguments;
  }

  public override Type GetInterface(string name, bool ignoreCase)
  {
    throw new NotImplementedException();
  }

  public override Type[] GetInterfaces()
  {
    throw new NotImplementedException();
  }

  public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  public override Type GetNestedType(string name, BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  public override Type[] GetNestedTypes(BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
  {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    return _definition.IsDefined(attributeType, inherit);
  }

  protected override TypeAttributes GetAttributeFlagsImpl()
  {
    return _definition.Attributes;
  }

  protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
  {
    throw new NotImplementedException();
  }

  protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
  {
    throw new NotImplementedException();
  }

  protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
  {
    throw new NotImplementedException();
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


  // HybridGenericTypeBase overrides

  protected override SymbolAssembly AssemblyCore => _definition.Assembly;

  protected override SymbolType? DeclaringTypeCore => _definition.DeclaringType;

  protected override SymbolModule ModuleCore => _definition.Module;

  protected override SymbolType? ReflectedTypeCore => throw new NotImplementedException();

  protected override RuntimeTypeBase BaseTypeCore
  {
    get
    {
      SymbolType definitionBaseType = _definition.BaseType!;

      if (!definitionBaseType.IsGenericType)
        return definitionBaseType;

      ImmutableArray<ITypeSymbol> baseTypeArgumentSymbols = ((INamedTypeSymbol)definitionBaseType.Symbol).TypeArguments;
      Type[] baseTypeArguments = baseTypeArgumentSymbols.Select(t => t.TypeKind is TypeKind.TypeParameter
        ? _typeArguments[((ITypeParameterSymbol)t).Ordinal]
        : _runtime.CreateTypeDelegator(t))
        .ToArray();

      return definitionBaseType.GetGenericTypeDefinition().MakeGenericType(baseTypeArguments);
    }
  }

  protected override SymbolType GetGenericTypeDefinitionCore()
  {
    return _definition;
  }


  // RuntimeTypeBase overrides

  protected override IRuntimeType? RuntimeBaseType => BaseTypeCore;

  protected override IRuntimeType? RuntimeDeclaringType => _definition.DeclaringType;

  protected override SymbolType RuntimeDefinition => _definition;

  protected override IRuntimeType RuntimeElementType => throw new InvalidOperationException(); // TODO: Message

  protected override IEnumerable<IRuntimeType> RuntimeInterfaces => throw new NotImplementedException();

  protected override Type RuntimeType => _runtimeType ??= _runtime.GetRuntimeType(this);

  protected override Type[] RuntimeTypeParameters => _definition.GetGenericArguments();


  // System.Object overrides

  public sealed override int GetHashCode()
  {
    return TypeEqualityComparer.Default.GetHashCode(this);
  }

  public sealed override string? ToString()
  {
    return TypeNameBuilder.ToString(this, TypeNameBuilder.Format.ToString);
  }


  // New members

  public new SymbolAssembly Assembly => AssemblyCore;

  public new SymbolType? DeclaringType => DeclaringTypeCore;

  public new SymbolModule Module => ModuleCore;

  public new SymbolType? ReflectedType => ReflectedTypeCore;

  public new SymbolType GetGenericTypeDefinition() => GetGenericTypeDefinitionCore();
}

internal abstract class HybridGenericTypeBase : RuntimeTypeBase
{
  // System.Type overrides

  public sealed override Assembly Assembly => AssemblyCore;

  public sealed override Type BaseType => BaseTypeCore;

  public sealed override Type? DeclaringType => DeclaringTypeCore;

  public sealed override Module Module => ModuleCore;

  public sealed override Type? ReflectedType => ReflectedTypeCore;

  public sealed override Type GetGenericTypeDefinition() => GetGenericTypeDefinitionCore();


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolAssembly AssemblyCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract RuntimeTypeBase BaseTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType? DeclaringTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType? ReflectedTypeCore { get; }

  protected abstract SymbolType GetGenericTypeDefinitionCore();
}