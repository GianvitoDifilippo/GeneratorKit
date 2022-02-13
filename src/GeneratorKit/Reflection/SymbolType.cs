using GeneratorKit.Comparers;
using GeneratorKit.Proxy;
using GeneratorKit.Reflection.Context;
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

  private Type? _runtimeType;

  protected SymbolType(IReflectionContext context)
  {
    Context = context;
  }

  public ITypeSymbol Symbol => SymbolCore;
  public bool IsSource => Symbol.IsSource();
  public Type RuntimeType => _runtimeType ??= Context.GetRuntimeType(this);

  protected IReflectionContext Context { get; }
  protected abstract ITypeSymbol SymbolCore { get; }


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
    // TODO: Add events
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
    throw new NotSupportedException();
  }

  public sealed override bool IsAssignableFrom(Type? c)
  {
    if (c is null)
      return false;

    return Context.IsAssignableFrom(this, c) ||  IsAssignableFromCore(c);
  }

  protected sealed override bool IsCOMObjectImpl()
  {
    return false;
  }

  public sealed override bool IsInstanceOfType(object? o)
  {
    if (o is null)
      return false;

    if (o is IProxied proxied && proxied.Delegate is OperationDelegate @delegate)
    {
      return IsAssignableFrom(@delegate.Type);
    }

    return IsAssignableFrom(o.GetType());
  }

  public sealed override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotSupportedException();
  }


  // SymbolTypeBase overrides

  protected sealed override SymbolType? DeclaringTypeCore => Symbol.ContainingType is { } containingType
    ? Context.CreateTypeDelegator(containingType)
    : null;

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
    throw new NotSupportedException();
    // return GetEventsEnumerable(bindingAttr).FirstOrDefault(x => x.Name == name);
  }

  protected sealed override SymbolEventInfo[] GetEventsCore()
  {
    throw new NotSupportedException();
    // return GetEventsEnumerable(s_defaultLookup).ToArray();
  }

  protected sealed override SymbolEventInfo[] GetEventsCore(BindingFlags bindingAttr)
  {
    throw new NotSupportedException();
    // return GetEventsEnumerable(bindingAttr).ToArray();
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

  public new SymbolNamedType GetGenericTypeDefinition() => GetGenericTypeDefinitionCore();

  public new SymbolType GetInterface(string name) => GetInterfaceCore(name, false);

  public new SymbolType GetInterface(string name, bool ignoreCase) => GetInterfaceCore(name, ignoreCase);

  public new SymbolType[] GetInterfaces() => GetInterfacesCore(); // TODO: IEnumerable

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

  public new SymbolNamedType MakeGenericType(params Type[] typeArguments) => MakeGenericTypeCore(typeArguments);

  public new SymbolType MakePointerType() => MakePointerTypeCore();


  // Other members

  public abstract bool IsSuperclassOf(Type? c);

  protected abstract bool IsAssignableFromCore(Type c);

  private IEnumerable<SymbolConstructorInfo> GetConstructorsEnumerable(BindingFlags bindingAttr)
  {
    return Symbol.GetMembers(bindingAttr, MemberFlags.Constructors, true)
      .Select(x => Context.CreateConstructorInfoDelegator((IMethodSymbol)x));
  }

  private IEnumerable<SymbolEventInfo> GetEventsEnumerable(BindingFlags bindingAttr)
  {
    throw new NotSupportedException();
  }

  private IEnumerable<SymbolFieldInfo> GetFieldsEnumerable(BindingFlags bindingAttr)
  {
    return Symbol.GetMembers(bindingAttr, MemberFlags.Fields, true)
      .Select(member => Context.CreateFieldInfoDelegator((IFieldSymbol)member, this));
  }

  private IEnumerable<SymbolMethodInfo> GetMethodsEnumerable(BindingFlags bindingAttr)
  {
    return Symbol.GetMembers(bindingAttr, MemberFlags.Methods, true)
      .Select(member => Context.CreateMethodInfoDelegator((IMethodSymbol)member, this));
  }

  private IEnumerable<SymbolType> GetNestedTypesEnumerable(BindingFlags bindingAttr)
  {
    return Symbol.GetMembers(bindingAttr, MemberFlags.NestedTypes, true)
      .Select(member => Context.CreateTypeDelegator((ITypeSymbol)member));
  }

  private IEnumerable<SymbolPropertyInfo> GetPropertiesEnumerable(BindingFlags bindingAttr)
  {
    return Symbol.GetMembers(bindingAttr, MemberFlags.Properties, true)
      .Select(member => Context.CreatePropertyInfoDelegator((IPropertySymbol)member, this));
  }

  private IEnumerable<MemberInfo> GetMembersEnumerable(BindingFlags bindingAttr)
  {
    return Symbol.GetMembers(bindingAttr, MemberFlags.AllMembers, true)
      .Where(member => member.Kind is SymbolKind.Field or SymbolKind.Property or SymbolKind.Method or SymbolKind.NamedType) // TODO: Add events when supported
      .Select<ISymbol, MemberInfo>(member => member switch
      {
        IFieldSymbol fieldSymbol         => Context.CreateFieldInfoDelegator(fieldSymbol, this),
        IPropertySymbol propertySymbol   => Context.CreatePropertyInfoDelegator(propertySymbol, this),
        IEventSymbol eventSymbol         => throw new NotSupportedException(),
        IMethodSymbol methodSymbol       => methodSymbol.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor
                                            ? Context.CreateConstructorInfoDelegator(methodSymbol)
                                            : Context.CreateMethodInfoDelegator(methodSymbol, this),
        INamedTypeSymbol namedTypeSymbol => Context.CreateTypeDelegator(namedTypeSymbol),
        _                                => throw Errors.Unreacheable
      });
  }
}

internal abstract class SymbolTypeBase : Type
{
  // System.Type overrides

  public sealed override Assembly Assembly => AssemblyCore;

  public sealed override Type? BaseType => BaseTypeCore;

  public sealed override Type? DeclaringType => DeclaringTypeCore;

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

  protected abstract SymbolNamedType GetGenericTypeDefinitionCore();

  protected abstract SymbolType GetInterfaceCore(string name, bool ignoreCase);

  protected abstract SymbolType[] GetInterfacesCore();

  protected abstract SymbolMethodInfo[] GetMethodsCore(BindingFlags bindingAttr);

  protected abstract SymbolPropertyInfo[] GetPropertiesCore(BindingFlags bindingAttr);

  protected abstract SymbolType? GetNestedTypeCore(string name, BindingFlags bindingAttr);

  protected abstract SymbolType[] GetNestedTypesCore(BindingFlags bindingAttr);

  protected abstract SymbolType MakeArrayTypeCore();

  protected abstract SymbolType MakeArrayTypeCore(int rank);

  protected abstract SymbolType MakeByRefTypeCore();

  protected abstract SymbolNamedType MakeGenericTypeCore(Type[] typeArguments);

  protected abstract SymbolType MakePointerTypeCore();
}