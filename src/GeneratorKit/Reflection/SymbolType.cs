using GeneratorKit.Comparers;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GeneratorKit.Reflection;

internal abstract class SymbolType : SymbolTypeBase
{
  private const BindingFlags s_defaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
  protected static readonly SymbolDisplayFormat s_namespaceFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

  protected readonly GeneratorRuntime _runtime;
  private Type? _runtimeType;

  protected SymbolType(GeneratorRuntime runtime)
  {
    _runtime = runtime;
  }

  public abstract ITypeSymbol Symbol { get; }


  // Systm.Type overrides

  public sealed override string? AssemblyQualifiedName => TypeNameBuilder.ToString(this, TypeNameBuilder.Format.AssemblyQualifiedName);

  public sealed override string? FullName => TypeNameBuilder.ToString(this, TypeNameBuilder.Format.FullName);

  public sealed override Guid GUID => throw new NotSupportedException();

  public sealed override bool IsSecurityCritical => true;

  public sealed override bool IsSecuritySafeCritical => false;

  public sealed override bool IsSecurityTransparent => false;

  public sealed override StructLayoutAttribute StructLayoutAttribute => throw new NotSupportedException();

  public sealed override RuntimeTypeHandle TypeHandle => UnderlyingSystemType.TypeHandle;

  public sealed override Type UnderlyingSystemType => RuntimeType.UnderlyingSystemType;

  public override bool Equals(Type? o)
  {
    return TypeEqualityComparer.Default.Equals(this, o);
  }

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
      .FirstOrDefault(x => x.AttributeClass is { } attributeClass && attributeClass.ToDisplayString() == "System.Reflection.DefaultMemberAttribute");
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

  public sealed override InterfaceMapping GetInterfaceMap(Type interfaceType)
  {
    throw new NotSupportedException();
  }

  public sealed override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
  {
    return GetMembersEnumerable(bindingAttr).Where(x => x.Name == name).ToArray();
  }

  public sealed override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
  {
    IEnumerable<MemberInfo> result = Enumerable.Empty<MemberInfo>();

    if ((type & MemberTypes.Constructor) != 0)
    {
      result = result.Concat(GetConstructorsEnumerable(bindingAttr));
    }
    if ((type & MemberTypes.Event) != 0)
    {
      result = result.Concat(GetEventsEnumerable(bindingAttr).Where(x => x.Name == name));
    }
    if ((type & MemberTypes.Field) != 0)
    {
      result = result.Concat(GetFieldsEnumerable(bindingAttr).Where(x => x.Name == name));
    }
    if ((type & MemberTypes.Method) != 0)
    {
      result = result.Concat(GetMethodsEnumerable(bindingAttr).Where(x => x.Name == name));
    }
    if ((type & MemberTypes.Property) != 0)
    {
      result = result.Concat(GetPropertiesEnumerable(bindingAttr).Where(x => x.Name == name));
    }
    if ((type & MemberTypes.NestedType) != 0)
    {
      result = result.Concat(GetNestedTypesEnumerable(bindingAttr).Where(x => x.Name == name));
    }

    return result.ToArray();
  }

  public sealed override MemberInfo[] GetMembers(BindingFlags bindingAttr)
  {
    return GetMembersEnumerable(bindingAttr).ToArray();
  }

  protected sealed override MethodInfo? GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
  {
    if (name is null) throw new ArgumentNullException(nameof(name));

    MethodInfo[] candidates = GetMethods(bindingAttr)
      .Where(x => x.Name == name)
      .Where(x => (x.CallingConvention & callConvention) != 0)
      .ToArray();

    if (candidates.Length == 0)
      return null;

    if (types is null || types.Length == 0)
    {
      MethodInfo firstCandidate = candidates[0];

      if (candidates.Length == 1)
      {
        return firstCandidate;
      }
      else if (types is null)
      {
        for (int j = 1; j < candidates.Length; j++)
        {
          MethodInfo methodInfo = candidates[j];
          if (CompareMethodSigAndName(methodInfo, firstCandidate))
          {
            throw new AmbiguousMatchException();
          }
        }

        return FindMostDerivedNewSlotMethod(candidates) as MethodInfo;
      }
    }

    return (binder ?? DefaultBinder).SelectMethod(bindingAttr, candidates, types, modifiers) as MethodInfo;

    static bool CompareMethodSigAndName(MethodBase m1, MethodBase m2)
    {
      ParameterInfo[] params1 = m1.GetParameters();
      ParameterInfo[] params2 = m2.GetParameters();

      if (params1.Length != params2.Length)
        return false;

      int numParams = params1.Length;
      for (int i = 0; i < numParams; i++)
      {
        if (params1[i].ParameterType != params2[i].ParameterType)
          return false;
      }

      return true;
    }

    static MethodBase? FindMostDerivedNewSlotMethod(MethodBase[] match)
    {
      int deepestHierarchy = 0;
      MethodBase? methodWithDeepestHierarchy = null;

      for (int i = 0; i < match.Length; i++)
      {
        int currentHierarchyDepth = GetHierarchyDepth(match[i].DeclaringType);

        if (currentHierarchyDepth == deepestHierarchy)
          throw new AmbiguousMatchException();

        if (currentHierarchyDepth > deepestHierarchy)
        {
          deepestHierarchy = currentHierarchyDepth;
          methodWithDeepestHierarchy = match[i];
        }
      }

      return methodWithDeepestHierarchy;

      static int GetHierarchyDepth(Type type)
      {
        int depth = 1;

        Type currentType = type.BaseType;
        while (currentType is not null)
        {
          depth++;
          currentType = currentType.BaseType;
        }

        return depth;
      }
    }
  }

  protected sealed override PropertyInfo? GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
  {
    if (name is null) throw new ArgumentNullException(nameof(name));

    PropertyInfo[] candidates = GetProperties(bindingAttr)
      .Where(x => x.Name == name)
      .ToArray();

    if (candidates.Length == 0)
      return null;

    if (types is null || types.Length == 0)
    {
      if (candidates.Length == 1)
      {
        PropertyInfo firstCandidate = candidates[0];

        return returnType is null || returnType.IsEquivalentTo(firstCandidate.PropertyType)
          ? firstCandidate
          : null;
      }
      else
      {
        if (returnType is null)
          throw new AmbiguousMatchException();
      }
    }

    return (binder ?? DefaultBinder).SelectProperty(bindingAttr, candidates, returnType, types, modifiers);
  }

  public sealed override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
  {
    return UnderlyingSystemType.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
  }

  protected sealed override bool IsCOMObjectImpl()
  {
    return false;
  }

  public sealed override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotSupportedException();
  }


  // SymbolTypeBase overrides

  protected sealed override SymbolType? DeclaringTypeCore => Symbol.ContainingType is { } containingType
    ? _runtime.CreateTypeDelegator(containingType)
    : null;

  protected sealed override SymbolType[] GenericTypeArgumentsCore => IsGenericType && !IsGenericTypeDefinition
    ? GetGenericArgumentsCore()
    : Array.Empty<SymbolType>();

  protected sealed override SymbolType? ReflectedTypeCore => DeclaringTypeCore;

  protected sealed override SymbolType[] FindInterfacesCore(TypeFilter filter, object filterCriteria)
  {
    throw new NotImplementedException();
  }

  protected sealed override SymbolConstructorInfo[] GetConstructorsCore(BindingFlags bindingAttr)
  {
    return GetConstructorsEnumerable(bindingAttr).ToArray();
  }

  protected sealed override SymbolEventInfo GetEventCore(string name, BindingFlags bindingAttr)
  {
    return GetEventsEnumerable(bindingAttr).FirstOrDefault(x => x.Name == name);
  }

  protected sealed override SymbolEventInfo[] GetEventsCore()
  {
    return GetEventsEnumerable(s_defaultLookup).ToArray();
  }

  protected sealed override SymbolEventInfo[] GetEventsCore(BindingFlags bindingAttr)
  {
    return GetEventsEnumerable(bindingAttr).ToArray();
  }

  protected sealed override SymbolFieldInfo GetFieldCore(string name, BindingFlags bindingAttr)
  {
    return GetFieldsEnumerable(bindingAttr).FirstOrDefault(x => x.Name == name);
  }

  protected sealed override SymbolFieldInfo[] GetFieldsCore(BindingFlags bindingAttr)
  {
    return GetFieldsEnumerable(bindingAttr).ToArray();
  }

  protected sealed override SymbolType GetInterfaceCore(string name, bool ignoreCase)
  {
    return ignoreCase
      ? GetInterfacesCore().FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
      : GetInterfacesCore().FirstOrDefault(x => x.Name == name);
  }

  protected sealed override SymbolMethodInfo[] GetMethodsCore(BindingFlags bindingAttr)
  {
    return GetMethodsEnumerable(bindingAttr).ToArray();
  }

  protected sealed override SymbolType? GetNestedTypeCore(string name, BindingFlags bindingAttr)
  {
    return GetNestedTypesEnumerable(bindingAttr).FirstOrDefault(x => x.Name == name);
  }

  protected sealed override SymbolType[] GetNestedTypesCore(BindingFlags bindingAttr)
  {
    return GetNestedTypesEnumerable(bindingAttr).ToArray();
  }

  protected sealed override SymbolPropertyInfo[] GetPropertiesCore(BindingFlags bindingAttr)
  {
    return GetPropertiesEnumerable(bindingAttr).ToArray();
  }

  protected sealed override SymbolType MakePointerTypeCore()
  {
    throw new NotSupportedException();
  }


  // GeneratorRuntimeType overrides

  protected override bool IsSource => Symbol.IsSource();


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

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolAssembly Assembly => AssemblyCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType? BaseType => BaseTypeCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType? DeclaringType => DeclaringTypeCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType[] GenericTypeArguments => GenericTypeArgumentsCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolModule Module => ModuleCore;

  public new SymbolType? ReflectedType => ReflectedTypeCore;

  public new SymbolType[] FindInterfaces(TypeFilter filter, object filterCriteria) => FindInterfacesCore(filter, filterCriteria);

  public new SymbolConstructorInfo[] GetConstructors() => (SymbolConstructorInfo[])base.GetConstructors();

  public new SymbolConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => GetConstructorsCore(bindingAttr);

  public new SymbolType? GetElementType() => GetElementTypeCore();

  public new SymbolEventInfo GetEvent(string name) => GetEventCore(name, s_defaultLookup);

  public new SymbolEventInfo GetEvent(string name, BindingFlags bindingAttr) => GetEventCore(name, bindingAttr);

  public new SymbolEventInfo[] GetEvents() => GetEventsCore();

  public new SymbolEventInfo[] GetEvents(BindingFlags bindingAttr) => GetEventsCore(bindingAttr);

  public new SymbolFieldInfo GetField(string name) => GetFieldCore(name, s_defaultLookup);

  public new SymbolFieldInfo GetField(string name, BindingFlags bindingAttr) => GetFieldCore(name, bindingAttr);

  public new SymbolFieldInfo[] GetFields(BindingFlags bindingAttr) => GetFieldsCore(bindingAttr);

  public new SymbolType[] GetGenericArguments() => GetGenericArgumentsCore();

  public new SymbolType[] GetGenericParameterConstraints() => GetGenericParameterConstraintsCore();

  public new SymbolType GetGenericTypeDefinition() => GetGenericTypeDefinitionCore();

  public new SymbolType GetInterface(string name) => GetInterfaceCore(name, false);

  public new SymbolType GetInterface(string name, bool ignoreCase) => GetInterfaceCore(name, ignoreCase);

  public new SymbolType[] GetInterfaces() => GetInterfacesCore();

  public new SymbolMethodInfo? GetMethod(string name) => (SymbolMethodInfo?)base.GetMethod(name);

  public new SymbolMethodInfo? GetMethod(string name, BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[] types, ParameterModifier[]? modifiers) => (SymbolMethodInfo?)base.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);

  public new SymbolMethodInfo[] GetMethods() => (SymbolMethodInfo[])base.GetMethods();

  public new SymbolMethodInfo[] GetMethods(BindingFlags bindingAttr) => GetMethodsCore(bindingAttr);

  public new SymbolPropertyInfo[] GetProperties(BindingFlags bindingAttr) => GetPropertiesCore(bindingAttr);

  public new SymbolType? GetNestedType(string name, BindingFlags bindingAttr) => GetNestedTypeCore(name, bindingAttr);

  public new SymbolType[] GetNestedTypes(BindingFlags bindingAttr) => GetNestedTypesCore(bindingAttr);

  public new SymbolType MakeArrayType() => MakeArrayTypeCore();

  public new SymbolType MakeArrayType(int rank) => MakeArrayTypeCore(rank);

  public new SymbolType MakeByRefType() => MakeByRefTypeCore();

  public new HybridGenericType MakeGenericType(params Type[] typeArguments) => MakeGenericTypeCore(typeArguments);

  public new SymbolType MakePointerType() => MakePointerTypeCore();


  // Other members

  public Type RuntimeType => _runtimeType ??= _runtime.GetRuntimeType(this);

  public SymbolType MakeGenericType(params SymbolType[] typeArguments)
  {
    return MakeGenericTypeCore(typeArguments);
  }

  private IEnumerable<SymbolConstructorInfo> GetConstructorsEnumerable(BindingFlags bindingAttr)
  {
    return GetMembersSymbols(bindingAttr, true)
      .Where(x => x is IMethodSymbol methodSymbol && methodSymbol.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor)
      .Select(x => _runtime.CreateConstructorInfoDelegator((IMethodSymbol)x));
  }

  private IEnumerable<SymbolEventInfo> GetEventsEnumerable(BindingFlags bindingAttr)
  {
    return GetMembersSymbols(bindingAttr, true)
      .Where(x => x.Kind is SymbolKind.Event)
      .Select(x => new SymbolEventInfo(_runtime, (IEventSymbol)x, this));
  }

  private IEnumerable<SymbolFieldInfo> GetFieldsEnumerable(BindingFlags bindingAttr)
  {
    return GetMembersSymbols(bindingAttr, true)
      .Where(x => x.Kind is SymbolKind.Field)
      .Select(x => new SymbolFieldInfo(_runtime, (IFieldSymbol)x, this));
  }

  private IEnumerable<SymbolMethodInfo> GetMethodsEnumerable(BindingFlags bindingAttr)
  {
    return GetMembersSymbols(bindingAttr, true)
      .Where(x => x is IMethodSymbol methodSymbol && methodSymbol.MethodKind is not MethodKind.Constructor and not MethodKind.StaticConstructor)
      .Select(x => new SymbolMethodInfo(_runtime, (IMethodSymbol)x, this));
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
      .Select(x => new SymbolPropertyInfo(_runtime, (IPropertySymbol)x, this));
  }

  private IEnumerable<MemberInfo> GetMembersEnumerable(BindingFlags bindingAttr)
  {
    return GetMembersSymbols(bindingAttr, true)
      .Where(x => x is IFieldSymbol or IPropertySymbol or IEventSymbol or IMethodSymbol or INamedTypeSymbol)
      .Select<ISymbol, MemberInfo>(x => x switch
      {
        IFieldSymbol fieldSymbol         => new SymbolFieldInfo(_runtime, fieldSymbol, this),
        IPropertySymbol propertySymbol   => new SymbolPropertyInfo(_runtime, propertySymbol, this),
        IEventSymbol eventSymbol         => new SymbolEventInfo(_runtime, eventSymbol, this),
        IMethodSymbol methodSymbol       => methodSymbol.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor
                                            ? _runtime.CreateConstructorInfoDelegator(methodSymbol)
                                            : new SymbolMethodInfo(_runtime, methodSymbol, this),
        INamedTypeSymbol namedTypeSymbol => _runtime.CreateTypeDelegator(namedTypeSymbol),
        _                                => throw Errors.Unreacheable
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

internal abstract class SymbolTypeBase : GeneratorRuntimeType
{
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

  public sealed override EventInfo? GetEvent(string name, BindingFlags bindingAttr) => GetEventCore(name, bindingAttr);

  public sealed override EventInfo[] GetEvents() => GetEventsCore();

  public sealed override EventInfo[] GetEvents(BindingFlags bindingAttr) => GetEventsCore(bindingAttr);

  public sealed override FieldInfo? GetField(string name, BindingFlags bindingAttr) => GetFieldCore(name, bindingAttr);

  public sealed override FieldInfo[] GetFields(BindingFlags bindingAttr) => GetFieldsCore(bindingAttr);

  public sealed override Type[] GetGenericArguments() => GetGenericArgumentsCore();

  public sealed override Type[] GetGenericParameterConstraints() => GetGenericParameterConstraintsCore();

  public sealed override Type GetGenericTypeDefinition() => GetGenericTypeDefinitionCore();

  public sealed override Type GetInterface(string name, bool ignoreCase) => GetInterfaceCore(name, ignoreCase);

  public sealed override Type[] GetInterfaces() => GetInterfacesCore();

  public sealed override MethodInfo[] GetMethods(BindingFlags bindingAttr) => GetMethodsCore(bindingAttr);

  public sealed override Type? GetNestedType(string name, BindingFlags bindingAttr) => GetNestedTypeCore(name, bindingAttr);

  public sealed override Type[] GetNestedTypes(BindingFlags bindingAttr) => GetNestedTypesCore(bindingAttr);

  public sealed override PropertyInfo[] GetProperties(BindingFlags bindingAttr) => GetPropertiesCore(bindingAttr);

  public sealed override Type MakeArrayType() => MakeArrayTypeCore();

  public sealed override Type MakeArrayType(int rank) => MakeArrayTypeCore(rank);

  public sealed override Type MakeByRefType() => MakeByRefTypeCore();

  public sealed override Type MakeGenericType(params Type[] typeArguments)
  {
    return typeArguments is SymbolType[] symbolTypeArguments
      ? MakeGenericTypeCore(symbolTypeArguments)
      : MakeGenericTypeCore(typeArguments);
  }

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

  protected abstract SymbolType? GetElementTypeCore();

  protected abstract SymbolType GetEnumUnderlyingTypeCore();

  protected abstract SymbolEventInfo? GetEventCore(string name, BindingFlags bindingAttr);

  protected abstract SymbolEventInfo[] GetEventsCore();

  protected abstract SymbolEventInfo[] GetEventsCore(BindingFlags bindingAttr);

  protected abstract SymbolFieldInfo? GetFieldCore(string name, BindingFlags bindingAttr);

  protected abstract SymbolFieldInfo[] GetFieldsCore(BindingFlags bindingAttr);

  protected abstract SymbolType[] GetGenericArgumentsCore();

  protected abstract SymbolType[] GetGenericParameterConstraintsCore();

  protected abstract SymbolType GetGenericTypeDefinitionCore();

  protected abstract SymbolType GetInterfaceCore(string name, bool ignoreCase);

  protected abstract SymbolType[] GetInterfacesCore();

  protected abstract SymbolMethodInfo[] GetMethodsCore(BindingFlags bindingAttr);

  protected abstract SymbolPropertyInfo[] GetPropertiesCore(BindingFlags bindingAttr);

  protected abstract SymbolType? GetNestedTypeCore(string name, BindingFlags bindingAttr);

  protected abstract SymbolType[] GetNestedTypesCore(BindingFlags bindingAttr);

  protected abstract SymbolType MakeArrayTypeCore();

  protected abstract SymbolType MakeArrayTypeCore(int rank);

  protected abstract SymbolType MakeByRefTypeCore();

  protected abstract HybridGenericType MakeGenericTypeCore(Type[] typeArguments);

  protected abstract SymbolType MakeGenericTypeCore(SymbolType[] typeArguments);

  protected abstract SymbolType MakePointerTypeCore();
}