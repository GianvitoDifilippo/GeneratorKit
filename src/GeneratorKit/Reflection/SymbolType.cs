using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace GeneratorKit.Reflection;

public abstract class SymbolType : SymbolTypeBase
{
  private readonly Compilation _compilation;
  protected readonly IGeneratorRuntime _runtime;

  protected SymbolType(IGeneratorRuntime runtime, Compilation compilation)
  {
    _runtime = runtime;
    _compilation = compilation;
  }

  public abstract ITypeSymbol Symbol { get; }

  protected override SymbolAssembly AssemblyCore => throw new NotImplementedException();

  protected override SymbolType? BaseTypeCore => throw new NotImplementedException();

  protected override SymbolType? DeclaringTypeCore => throw new NotImplementedException();

  protected override SymbolModule ModuleCore => throw new NotImplementedException();

  protected override SymbolType? ReflectedTypeCore => throw new NotImplementedException();

  public override string AssemblyQualifiedName => throw new NotImplementedException();

  public override string FullName => throw new NotImplementedException();

  public override Guid GUID => throw new NotImplementedException();

  public override string Namespace => throw new NotImplementedException();

  public override Type UnderlyingSystemType => throw new NotImplementedException();

  public override string Name => throw new NotImplementedException();

  protected override SymbolType[] FindInterfacesCore(TypeFilter filter, object filterCriteria)
  {
    throw new NotImplementedException();
  }

  protected override SymbolConstructorInfo[] GetConstructorsCore(BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  protected override SymbolType GetElementTypeCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolType GetEnumUnderlyingTypeCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolEventInfo GetEventCore(string name, BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  protected override SymbolEventInfo[] GetEventsCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolEventInfo[] GetEventsCore(BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  protected override SymbolFieldInfo GetFieldCore(string name, BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  protected override SymbolFieldInfo[] GetFieldsCore(BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  protected override SymbolType[] GetGenericArgumentsCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolType GetGenericTypeDefinitionCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolType GetInterfaceCore(string name, bool ignoreCase)
  {
    throw new NotImplementedException();
  }

  protected override SymbolType[] GetInterfacesCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolMethodInfo[] GetMethodsCore(BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  protected override SymbolPropertyInfo[] GetPropertiesCore(BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  protected override SymbolType GetNestedTypeCore(string name, BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  protected override SymbolType[] GetNestedTypesCore(BindingFlags bindingAttr)
  {
    throw new NotImplementedException();
  }

  protected override SymbolType MakeArrayTypeCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolType MakeArrayTypeCore(int rank)
  {
    throw new NotImplementedException();
  }

  protected override SymbolType MakeByRefTypeCore()
  {
    throw new NotImplementedException();
  }

  protected override SymbolType MakeGenericTypeCore(params Type[] typeArguments)
  {
    throw new NotImplementedException();
  }

  protected override SymbolType MakePointerTypeCore()
  {
    throw new NotImplementedException();
  }

  protected override TypeAttributes GetAttributeFlagsImpl()
  {
    throw new NotImplementedException();
  }

  protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
  {
    throw new NotImplementedException();
  }

  public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
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
    throw new NotImplementedException();
  }

  public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
  {
    throw new NotImplementedException();
  }

  protected override bool IsArrayImpl()
  {
    throw new NotImplementedException();
  }

  protected override bool IsByRefImpl()
  {
    throw new NotImplementedException();
  }

  protected override bool IsCOMObjectImpl()
  {
    throw new NotImplementedException();
  }

  protected override bool IsPointerImpl()
  {
    throw new NotImplementedException();
  }

  protected override bool IsPrimitiveImpl()
  {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(bool inherit)
  {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }
}

#region Base

public abstract class SymbolTypeBase : Type
{
  private protected SymbolTypeBase() { }


  // System.Type overrides

  public sealed override Assembly Assembly => AssemblyCore;

  public sealed override Type? BaseType => BaseTypeCore;

  public sealed override Type? DeclaringType => DeclaringTypeCore;

  public sealed override Module Module => ModuleCore;

  public sealed override Type? ReflectedType => ReflectedTypeCore;

  public sealed override Type[] FindInterfaces(TypeFilter filter, object filterCriteria) => FindInterfacesCore(filter, filterCriteria);

  public sealed override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => GetConstructorsCore(bindingAttr);

  public sealed override Type GetElementType() => GetElementTypeCore();

  public sealed override Type GetEnumUnderlyingType() => GetEnumUnderlyingTypeCore();

  public sealed override EventInfo GetEvent(string name, BindingFlags bindingAttr) => GetEventCore(name, bindingAttr);

  public sealed override EventInfo[] GetEvents() => GetEventsCore();

  public sealed override EventInfo[] GetEvents(BindingFlags bindingAttr) => GetEventsCore(bindingAttr);

  public sealed override FieldInfo GetField(string name, BindingFlags bindingAttr) => GetFieldCore(name, bindingAttr);

  public sealed override FieldInfo[] GetFields(BindingFlags bindingAttr) => GetFieldsCore(bindingAttr);

  public sealed override Type[] GetGenericArguments() => GetGenericArgumentsCore();

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
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType? ReflectedTypeCore { get; }

  protected abstract SymbolType[] FindInterfacesCore(TypeFilter filter, object filterCriteria);

  protected abstract SymbolConstructorInfo[] GetConstructorsCore(BindingFlags bindingAttr);

  protected abstract SymbolType GetElementTypeCore();

  protected abstract SymbolType GetEnumUnderlyingTypeCore();

  protected abstract SymbolEventInfo GetEventCore(string name, BindingFlags bindingAttr);

  protected abstract SymbolEventInfo[] GetEventsCore();

  protected abstract SymbolEventInfo[] GetEventsCore(BindingFlags bindingAttr);

  protected abstract SymbolFieldInfo GetFieldCore(string name, BindingFlags bindingAttr);

  protected abstract SymbolFieldInfo[] GetFieldsCore(BindingFlags bindingAttr);

  protected abstract SymbolType[] GetGenericArgumentsCore();

  protected abstract SymbolType GetGenericTypeDefinitionCore();

  protected abstract SymbolType GetInterfaceCore(string name, bool ignoreCase);

  protected abstract SymbolType[] GetInterfacesCore();

  protected abstract SymbolMethodInfo[] GetMethodsCore(BindingFlags bindingAttr);

  protected abstract SymbolPropertyInfo[] GetPropertiesCore(BindingFlags bindingAttr);

  protected abstract SymbolType GetNestedTypeCore(string name, BindingFlags bindingAttr) ;

  protected abstract SymbolType[] GetNestedTypesCore(BindingFlags bindingAttr);

  protected abstract SymbolType MakeArrayTypeCore();

  protected abstract SymbolType MakeArrayTypeCore(int rank);

  protected abstract SymbolType MakeByRefTypeCore();

  protected abstract SymbolType MakeGenericTypeCore(params Type[] typeArguments);

  protected abstract SymbolType MakePointerTypeCore();
}

#endregion