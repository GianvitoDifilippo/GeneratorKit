using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal abstract class SymbolType : SymbolTypeBase
{
  private static readonly SymbolDisplayFormat s_namespaceFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

  protected readonly Compilation _compilation;
  protected readonly GeneratorRuntime _runtime;

  protected SymbolType(GeneratorRuntime runtime, Compilation compilation)
  {
    _runtime = runtime;
    _compilation = compilation;
  }

  public abstract ITypeSymbol Symbol { get; }


  public sealed override string? AssemblyQualifiedName => FullName is null
    ? null
    : Assembly.CreateQualifiedName(Assembly.FullName, FullName);

  public sealed override Guid GUID => throw new NotImplementedException();

  public sealed override bool IsSecurityCritical => true;

  public sealed override bool IsSecuritySafeCritical => false;

  public sealed override bool IsSecurityTransparent => false;

  public sealed override bool IsSerializable
  {
    get
    {
      if (Symbol.TypeKind is TypeKind.Enum) return true;

      if (IsPrimitive) return true;

      if (FullName is
        "System.DateTime"       or
        "System.DateTimeOffset" or
        "System.Decimal"        or
        "System.Enum"           or
        "System.Guid"           or
        "System.Object"         or
        "System.String"         or
        "System.Uri"            or
        "System.Xml.XmlQualifiedName") return true;

      return Symbol.GetAttributes().Any(x => x.AttributeClass is not null && x.AttributeClass.ToDisplayString() == "System.SerializableAttribute");
    }
  }

  public sealed override string Namespace => Symbol.ContainingNamespace.ToDisplayString(s_namespaceFormat);

  public sealed override Type UnderlyingSystemType => _runtime.GetRuntimeType(this) ?? this;

  protected sealed override ConstructorInfo? GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
  {
    ConstructorInfo[] candidates = GetConstructors(bindingAttr);

    if (candidates.Length == 0)
      return null;

    if (types.Length == 0 && candidates.Length == 1)
    {
      ConstructorInfo firstCandidate = candidates[0];

      ParameterInfo[] parameters = firstCandidate.GetParameters();
      if (parameters == null || parameters.Length == 0)
      {
        return firstCandidate;
      }
    }

    return (binder ?? DefaultBinder).SelectMethod(bindingAttr, candidates.ToArray(), types, modifiers) as ConstructorInfo;
  }

  public sealed override object[] GetCustomAttributes(bool inherit)
  {
    throw new NotImplementedException();
  }

  public sealed override object[] GetCustomAttributes(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public sealed override MemberInfo[] GetDefaultMembers()
  {
    AttributeData? attribute = Symbol
      .GetAttributes()
      .SingleOrDefault(x => x.AttributeClass is { } attributeClass && attributeClass.ToDisplayString() == "System.Reflection.DefaultMemberAttribute");
    if (attribute is not null)
    {
      return attribute.ConstructorArguments[0].Value is string member
        ? GetMember(member)
        : Array.Empty<MemberInfo>();
    }
    return GetPropertiesEnumerable(BindingFlags.Public | BindingFlags.Instance)
      .Where(x => x.GetIndexParameters().Any())
      .ToArray();
  }

  public sealed override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
  {
    return GetMembersEnumerable(bindingAttr).Where(x => x.Name == name).ToArray();
  }

  public sealed override MemberInfo[] GetMembers(BindingFlags bindingAttr)
{
    return GetMembersEnumerable(bindingAttr).ToArray();
  }

  protected sealed override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
  {
    throw new NotImplementedException();
  }

  protected sealed override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
  {
    throw new NotImplementedException();
  }

  public sealed override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
  {
    throw new NotImplementedException();
  }

  protected sealed override bool IsByRefImpl()
  {
    return false;
  }

  protected sealed override bool IsCOMObjectImpl()
  {
    return false;
  }

  public sealed override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }


  // SymbolTypeBase overrides

  protected sealed override SymbolAssembly AssemblyCore => _runtime.CreateAssemblyDelegator(Symbol.ContainingAssembly);

  protected sealed override SymbolType? BaseTypeCore => Symbol.BaseType is { } baseType
    ? _runtime.CreateTypeDelegator(baseType)
    : null;

  protected sealed override SymbolType? DeclaringTypeCore => Symbol.ContainingType is { } containingType
    ? _runtime.CreateTypeDelegator(containingType)
    : null;

  protected sealed override SymbolModule ModuleCore => _runtime.CreateModuleDelegator(Symbol.ContainingModule);

  protected sealed override SymbolType? ReflectedTypeCore => DeclaringTypeCore;

  protected override SymbolType[] FindInterfacesCore(TypeFilter filter, object filterCriteria)
  {
    throw new NotImplementedException();
  }

  protected sealed override SymbolConstructorInfo[] GetConstructorsCore(BindingFlags bindingAttr)
  {
    return GetConstructorsEnumerable(bindingAttr).ToArray();
  }

  protected sealed override SymbolType GetEnumUnderlyingTypeCore()
  {
    throw new NotImplementedException();
  }

  protected sealed override SymbolEventInfo GetEventCore(string name, BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  protected sealed override SymbolEventInfo[] GetEventsCore()
  {
    throw new NotImplementedException();
  }

  protected sealed override SymbolEventInfo[] GetEventsCore(BindingFlags bindingAttr)
  {
    return GetEventsEnumerable(bindingAttr).ToArray();
  }

  protected sealed override SymbolFieldInfo GetFieldCore(string name, BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  protected sealed override SymbolFieldInfo[] GetFieldsCore(BindingFlags bindingAttr)
  {
    return GetFieldsEnumerable(bindingAttr).ToArray();
  }

  protected sealed override SymbolType GetGenericTypeDefinitionCore()
  {
    throw new NotImplementedException();
  }

  protected sealed override SymbolType GetInterfaceCore(string name, bool ignoreCase)
  {
    throw new NotImplementedException();
  }

  protected sealed override SymbolType[] GetInterfacesCore()
  {
    return Symbol.AllInterfaces.Select(x => _runtime.CreateTypeDelegator(x)).ToArray();
  }

  protected sealed override SymbolMethodInfo[] GetMethodsCore(BindingFlags bindingAttr)
  {
    return GetMethodsEnumerable(bindingAttr).ToArray();
  }

  protected sealed override SymbolType GetNestedTypeCore(string name, BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  protected sealed override SymbolType[] GetNestedTypesCore(BindingFlags bindingAttr)
  {
    return GetNestedTypesEnumerable(bindingAttr).ToArray();
  }

  protected sealed override SymbolPropertyInfo[] GetPropertiesCore(BindingFlags bindingAttr)
  {
    return GetPropertiesEnumerable(bindingAttr).ToArray();
  }

  protected sealed override SymbolType MakeArrayTypeCore()
  {
    return MakeArrayTypeCore(1);
  }

  protected sealed override SymbolType MakeArrayTypeCore(int rank)
  {
    return _runtime.CreateTypeDelegator(_compilation.CreateArrayTypeSymbol(Symbol, rank));
  }

  protected sealed override SymbolType MakeByRefTypeCore()
  {
    throw new NotImplementedException();
  }

  protected sealed override SymbolType MakeGenericTypeCore(params Type[] typeArguments)
  {
    throw new NotImplementedException();
  }

  protected sealed override SymbolType MakePointerTypeCore()
  {
    throw new NotImplementedException();
  }


  // New members


  // Private members

  private IEnumerable<SymbolConstructorInfo> GetConstructorsEnumerable(BindingFlags bindingAttr)
  {
    return GetMembersSymbols(bindingAttr, true)
      .Where(x => x is IMethodSymbol methodSymbol && methodSymbol.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor)
      .Select(x => new SymbolConstructorInfo(_runtime, (IMethodSymbol)x));
  }

  private IEnumerable<SymbolEventInfo> GetEventsEnumerable(BindingFlags bindingAttr)
  {
    return GetMembersSymbols(bindingAttr, true)
      .Where(x => x.Kind is SymbolKind.Event)
      .Select(x => new SymbolEventInfo(_runtime, (IEventSymbol)x));
  }

  private IEnumerable<SymbolFieldInfo> GetFieldsEnumerable(BindingFlags bindingAttr)
  {
    return GetMembersSymbols(bindingAttr, true)
      .Where(x => x.Kind is SymbolKind.Field)
      .Select(x => new SymbolFieldInfo(_runtime, (IFieldSymbol)x));
  }

  private IEnumerable<SymbolMethodInfo> GetMethodsEnumerable(BindingFlags bindingAttr)
  {
    return GetMembersSymbols(bindingAttr, true)
      .Where(x => x is IMethodSymbol methodSymbol && methodSymbol.MethodKind is not MethodKind.Constructor and not MethodKind.StaticConstructor)
      .Select(x => new SymbolMethodInfo(_runtime, (IMethodSymbol)x));
  }

  private IEnumerable<SymbolType> GetNestedTypesEnumerable(BindingFlags bindingAttr)
  {
    return GetMembersSymbols(bindingAttr, true)
      .Where(x => x.Kind is SymbolKind.NamedType)
      .Select(x => _runtime.CreateTypeDelegator((ITypeSymbol)x));
  }

  private IEnumerable<SymbolPropertyInfo> GetPropertiesEnumerable(BindingFlags bindingAttr)
  {
    return GetMembersSymbols(bindingAttr, true)
      .Where(x => x.Kind is SymbolKind.Property)
      .Select(x => new SymbolPropertyInfo(_runtime, (IPropertySymbol)x));
  }

  private IEnumerable<MemberInfo> GetMembersEnumerable(BindingFlags bindingAttr)
  {
    return GetMembersSymbols(bindingAttr, true)
      .Where(x => x is IFieldSymbol or IPropertySymbol or IEventSymbol or IMethodSymbol or INamedTypeSymbol)
      .Select<ISymbol, MemberInfo>(x => x switch
      {
        IFieldSymbol fieldSymbol         => _runtime.CreateFieldInfoDelegator(fieldSymbol),
        IPropertySymbol propertySymbol   => _runtime.CreatePropertyInfoDelegator(propertySymbol),
        IEventSymbol eventSymbol         => _runtime.CreateEventInfoDelegator(eventSymbol),
        IMethodSymbol methodSymbol       => methodSymbol.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor
                                            ? _runtime.CreateConstructorInfoDelegator(methodSymbol)
                                            : _runtime.CreateMethodInfoDelegator(methodSymbol),
        INamedTypeSymbol namedTypeSymbol => _runtime.CreateTypeDelegator(namedTypeSymbol),
        _ => throw new InvalidOperationException()
      });
  }

  private IEnumerable<ISymbol> GetMembersSymbols(BindingFlags bindingAttr, bool isReflectedType)
  {
    bool hasPublicFlag = bindingAttr.HasFlag(BindingFlags.Public);
    bool hasNonPublicFlag = bindingAttr.HasFlag(BindingFlags.NonPublic);
    bool hasInstanceFlag = bindingAttr.HasFlag(BindingFlags.Instance);
    bool hasStaticFlag = bindingAttr.HasFlag(BindingFlags.Static);
    bool hasFlattenHierarchyFlag = bindingAttr.HasFlag(BindingFlags.FlattenHierarchy);
    bool hasDeclaredOnlyFlag = bindingAttr.HasFlag(BindingFlags.DeclaredOnly);

    foreach (ISymbol member in Symbol.GetMembers())
    {
      bool isPublic = member.DeclaredAccessibility is Accessibility.Public;
      bool isInstance = !member.IsStatic;
      bool isNestedType = member.Kind is SymbolKind.NamedType;
      bool isConstructor = member.Kind is SymbolKind.Method && ((IMethodSymbol)member).MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor;

      if (!isReflectedType && (isConstructor || isNestedType))
      {
        continue;
      }

      if (isPublic && hasPublicFlag)
      {
        if (isInstance && hasInstanceFlag || isNestedType)
        {
          yield return member;
        }
        else if (!isInstance && hasStaticFlag)
        {
          if (isReflectedType || (hasFlattenHierarchyFlag && member.DeclaredAccessibility is not Accessibility.Private))
          {
            yield return member;
          }
        }
      }
      else if (!isPublic && hasNonPublicFlag)
      {
        if (isInstance && hasInstanceFlag || isNestedType)
        {
          if (isReflectedType || member.DeclaredAccessibility is not Accessibility.Private)
          {
            yield return member;
          }
        }
        else if (!isInstance && hasStaticFlag)
        {
          if (isReflectedType || (hasFlattenHierarchyFlag && member.DeclaredAccessibility is not Accessibility.Private))
          {
            yield return member;
          }
        }
      }
    }

    if (!hasDeclaredOnlyFlag && BaseType is SymbolNamedType baseType)
    {
      foreach (ISymbol baseMember in baseType.GetMembersSymbols(bindingAttr, false))
      {
        yield return baseMember;
      }
    }
  }
}

internal abstract class SymbolTypeBase : Type
{
  private protected SymbolTypeBase() { }


  // System.Type overrides

  public sealed override Assembly Assembly => AssemblyCore;

  public sealed override Type? BaseType => BaseTypeCore;

  public sealed override Type? DeclaringType => DeclaringTypeCore;

  public sealed override Type[] GenericTypeArguments => GenericTypeArgumentsCore;

  public sealed override Module Module => ModuleCore;

  public sealed override Type? ReflectedType => ReflectedTypeCore;

  public sealed override Type[] FindInterfaces(TypeFilter filter, object filterCriteria) => FindInterfacesCore(filter, filterCriteria);

  public sealed override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => GetConstructorsCore(bindingAttr);

  public sealed override Type? GetElementType() => GetElementTypeCore();

  public sealed override Type GetEnumUnderlyingType() => GetEnumUnderlyingTypeCore();

  public sealed override EventInfo GetEvent(string name, BindingFlags bindingAttr) => GetEventCore(name, bindingAttr);

  public sealed override EventInfo[] GetEvents() => GetEventsCore();

  public sealed override EventInfo[] GetEvents(BindingFlags bindingAttr) => GetEventsCore(bindingAttr);

  public sealed override FieldInfo GetField(string name, BindingFlags bindingAttr) => GetFieldCore(name, bindingAttr);

  public sealed override FieldInfo[] GetFields(BindingFlags bindingAttr) => GetFieldsCore(bindingAttr);

  public sealed override Type[] GetGenericArguments() => GetGenericArgumentsCore();

  public sealed override Type[] GetGenericParameterConstraints() => GetGenericParameterConstraintsCore();

  public sealed override Type GetGenericTypeDefinition() => GetGenericTypeDefinitionCore();

  public sealed override Type GetInterface(string name, bool ignoreCase) => GetInterfaceCore(name, ignoreCase);

  public sealed override Type[] GetInterfaces() => GetInterfacesCore();

  public sealed override MethodInfo[] GetMethods(BindingFlags bindingAttr) => GetMethodsCore(bindingAttr);

  public sealed override Type GetNestedType(string name, BindingFlags bindingAttr) => GetNestedTypeCore(name, bindingAttr);

  public sealed override Type[] GetNestedTypes(BindingFlags bindingAttr) => GetNestedTypesCore(bindingAttr);

  public sealed override PropertyInfo[] GetProperties(BindingFlags bindingAttr) => GetPropertiesCore(bindingAttr);

  public sealed override Type MakeArrayType() => MakeArrayTypeCore();

  public sealed override Type MakeArrayType(int rank) => MakeArrayTypeCore(rank);

  public sealed override Type MakeByRefType() => MakeByRefTypeCore();

  public sealed override Type MakeGenericType(params Type[] typeArguments) => MakeGenericTypeCore(typeArguments);

  public sealed override Type MakePointerType() => MakePointerTypeCore();


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolAssembly AssemblyCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType? BaseTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType? DeclaringTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType[] GenericTypeArgumentsCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType? ReflectedTypeCore { get; }

  protected abstract SymbolType[] FindInterfacesCore(TypeFilter filter, object filterCriteria);

  protected abstract SymbolConstructorInfo[] GetConstructorsCore(BindingFlags bindingAttr);

  protected abstract SymbolType GetEnumUnderlyingTypeCore();

  protected abstract SymbolType? GetElementTypeCore();

  protected abstract SymbolEventInfo GetEventCore(string name, BindingFlags bindingAttr);

  protected abstract SymbolEventInfo[] GetEventsCore();

  protected abstract SymbolEventInfo[] GetEventsCore(BindingFlags bindingAttr);

  protected abstract SymbolFieldInfo GetFieldCore(string name, BindingFlags bindingAttr);

  protected abstract SymbolFieldInfo[] GetFieldsCore(BindingFlags bindingAttr);

  protected abstract SymbolType[] GetGenericArgumentsCore();

  protected abstract SymbolType[] GetGenericParameterConstraintsCore();

  protected abstract SymbolType GetGenericTypeDefinitionCore();

  protected abstract SymbolType GetInterfaceCore(string name, bool ignoreCase);

  protected abstract SymbolType[] GetInterfacesCore();

  protected abstract SymbolMethodInfo[] GetMethodsCore(BindingFlags bindingAttr);

  protected abstract SymbolPropertyInfo[] GetPropertiesCore(BindingFlags bindingAttr);

  protected abstract SymbolType GetNestedTypeCore(string name, BindingFlags bindingAttr);

  protected abstract SymbolType[] GetNestedTypesCore(BindingFlags bindingAttr);

  protected abstract SymbolType MakeArrayTypeCore();

  protected abstract SymbolType MakeArrayTypeCore(int rank);

  protected abstract SymbolType MakeByRefTypeCore();

  protected abstract SymbolType MakeGenericTypeCore(params Type[] typeArguments);

  protected abstract SymbolType MakePointerTypeCore();
}