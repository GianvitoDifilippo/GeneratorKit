using GeneratorKit.Comparers;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace GeneratorKit.Reflection;

internal sealed class SymbolModule : SymbolModuleBase
{
  private readonly GeneratorRuntime _runtime;

  public SymbolModule(GeneratorRuntime runtime, IModuleSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public IModuleSymbol Symbol { get; }


  // System.Reflection.Module overrides

  public override string FullyQualifiedName => "<Unknown>";

  public override int MDStreamVersion => throw new NotSupportedException();

  public override int MetadataToken => throw new NotSupportedException();

  public override Guid ModuleVersionId => throw new NotSupportedException();

  public override string Name => "<Unknown>";

  public override string ScopeName => Symbol.Name;

  public override object[] GetCustomAttributes(bool inherit)
  {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override IList<CustomAttributeData> GetCustomAttributesData()
  {
    List<CustomAttributeData> result = Symbol
      .GetAttributes()
      .Select(x => (CustomAttributeData)CompilationCustomAttributeData.FromAttributeData(_runtime, x))
      .ToList();
    return new ReadOnlyCollection<CustomAttributeData>(result);
  }

  public override Type[] FindTypes(TypeFilter filter, object filterCriteria)
  {
    throw new NotImplementedException();
  }

  public override FieldInfo GetField(string name, BindingFlags bindingAttr)
  {
    throw new NotSupportedException();
  }

  public override FieldInfo[] GetFields(BindingFlags bindingFlags)
  {
    throw new NotSupportedException();
  }

  protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
  {
    throw new NotSupportedException();
  }

  public override MethodInfo[] GetMethods(BindingFlags bindingFlags)
  {
    throw new NotSupportedException();
  }

  public override void GetObjectData(SerializationInfo info, StreamingContext context)
  {
    throw new NotSupportedException();
  }

  public override void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
  {
    throw new NotSupportedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override bool IsResource()
  {
    throw new NotImplementedException();
  }

  public override FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
  {
    throw new NotSupportedException();
  }

  public override MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
  {
    throw new NotSupportedException();
  }

  public override MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
  {
    throw new NotSupportedException();
  }

  public override byte[] ResolveSignature(int metadataToken)
  {
    throw new NotSupportedException();
  }

  public override string ResolveString(int metadataToken)
  {
    throw new NotSupportedException();
  }

  public override Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
  {
    throw new NotSupportedException();
  }


  // SymbolModuleBase overrides

  protected override SymbolAssembly AssemblyCore => _runtime.CreateAssemblyDelegator(Symbol.ContainingAssembly);

  protected override SymbolType? GetTypeCore(string className)
  {
    return GetTypeCore(className, false, false);
  }

  protected override SymbolType? GetTypeCore(string className, bool ignoreCase)
  {
    return GetTypeCore(className, false, ignoreCase);
  }

  protected override SymbolType? GetTypeCore(string className, bool throwOnError, bool ignoreCase)
  {
    GetTypeVisitor visitor = new GetTypeVisitor(_runtime, className, ignoreCase);
    SymbolType? type = visitor.VisitNamespace(Symbol.GlobalNamespace);
    return type is not null
      ? type
      : throwOnError ? throw new TypeLoadException() : null;
  }

  protected override SymbolType[] GetTypesCore()
  {
    HashSet<SymbolType> types = new HashSet<SymbolType>(TypeEqualityComparer.Default);
    GetAllTypesVisitor visitor = new GetAllTypesVisitor(_runtime, types);
    visitor.VisitNamespace(Symbol.GlobalNamespace);
    return types.ToArray();
  }


  // System.Object overrides

  public override bool Equals(object? obj)
  {
    if (obj is not Module module)
      return false;

    return ModuleEqualityComparer.Default.Equals(this, module);
  }

  public override int GetHashCode()
  {
    return ModuleEqualityComparer.Default.GetHashCode(this);
  }

  public override string ToString()
  {
    return ScopeName;
  }


  // New members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolAssembly Assembly => AssemblyCore;

  public new SymbolType? GetType(string className) => GetTypeCore(className);

  public new SymbolType? GetType(string className, bool ignoreCase) => GetTypeCore(className, ignoreCase);

  public new SymbolType? GetType(string className, bool throwOnError, bool ignoreCase) => GetTypeCore(className, throwOnError, ignoreCase);

  public new SymbolType[] GetTypes() => GetTypesCore();
}

internal abstract class SymbolModuleBase : Module
{
  private protected SymbolModuleBase() { }


  // System.Reflection.Module overrides

  public sealed override Assembly Assembly => AssemblyCore;

  public sealed override Type? GetType(string className) => GetTypeCore(className);

  public sealed override Type? GetType(string className, bool ignoreCase) => GetTypeCore(className, ignoreCase);

  public sealed override Type? GetType(string className, bool throwOnError, bool ignoreCase) => GetTypeCore(className, throwOnError, ignoreCase);

  public sealed override Type[] GetTypes() => GetTypesCore();

  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolAssembly AssemblyCore { get; }

  protected abstract SymbolType? GetTypeCore(string className);

  protected abstract SymbolType? GetTypeCore(string className, bool ignoreCase);

  protected abstract SymbolType? GetTypeCore(string className, bool throwOnError, bool ignoreCase);

  protected abstract SymbolType[] GetTypesCore();
}