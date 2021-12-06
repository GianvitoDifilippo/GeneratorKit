using GeneratorKit.Comparers;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using TypeInfo = System.Reflection.TypeInfo;

namespace GeneratorKit.Reflection;

internal sealed class SymbolAssembly : SymbolAssemblyBase
{
  private readonly GeneratorRuntime _runtime;
  private readonly IMethodSymbol? _entryPoint;

  public SymbolAssembly(GeneratorRuntime runtime, IAssemblySymbol symbol, IMethodSymbol? entryPoint)
  {
    _runtime = runtime;
    Symbol = symbol;
    _entryPoint = entryPoint;
  }

  public IAssemblySymbol Symbol { get; }


  // System.Reflection.Assembly overrides

  public override string? CodeBase => throw new NotSupportedException();

  public override IEnumerable<TypeInfo> DefinedTypes => throw new NotSupportedException();

  public override string EscapedCodeBase => throw new NotSupportedException();

  public override string FullName => Symbol.Identity.GetDisplayName();

  public override bool GlobalAssemblyCache => false;

  public override long HostContext => throw new NotSupportedException();

  public override string ImageRuntimeVersion => throw new NotSupportedException();

  public override bool IsDynamic => false;

  public override bool ReflectionOnly => false;

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

  public override FileStream GetFile(string name)
  {
    throw new FileNotFoundException("This assembly does not have a file table because it was loaded from memory.");
  }

  public override FileStream[] GetFiles()
  {
    throw new FileNotFoundException("This assembly does not have a file table because it was loaded from memory.");
  }

  public override FileStream[] GetFiles(bool getResourceModules)
  {
    throw new FileNotFoundException("This assembly does not have a file table because it was loaded from memory.");
  }

  public override AssemblyName GetName(bool copiedName)
  {
    return new AssemblyName(Symbol.Identity.GetDisplayName());
  }

  public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
  {
    throw new NotSupportedException();
  }

  public override string[] GetManifestResourceNames()
  {
    throw new NotSupportedException();
  }

  public override Stream GetManifestResourceStream(string name)
  {
    throw new NotSupportedException();
  }

  public override Stream GetManifestResourceStream(Type type, string name)
  {
    throw new NotSupportedException();
  }

  public override void GetObjectData(SerializationInfo info, StreamingContext context)
  {
    throw new NotSupportedException();
  }

  public override Assembly GetSatelliteAssembly(CultureInfo culture)
  {
    throw new NotSupportedException();
  }

  public override Assembly GetSatelliteAssembly(CultureInfo culture, Version version)
  {
    throw new NotSupportedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override Module LoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore)
  {
    throw new NotSupportedException();
  }


  // SymbolAssemblyBase overrides

  protected override SymbolMethodInfo? EntryPointCore => _entryPoint is not null
    ? _runtime.CreateMethodInfoDelegator(_entryPoint)
    : null;

  protected override SymbolModule ManifestModuleCore => _runtime.CreateModuleDelegator(Symbol.Modules.First());

  protected override SymbolType[] GetExportedTypesCore()
  {
    return GetTypes().Where(x => x.IsPublic).ToArray();
  }

  protected override SymbolModule[] GetLoadedModulesCore(bool getResourceModules)
  {
    return Symbol.Modules.Select(x => _runtime.CreateModuleDelegator(x)).ToArray();
  }

  protected override SymbolModule? GetModuleCore(string name)
  {
    return GetModulesCore(false).FirstOrDefault(x => x.Name == name);
  }

  protected override SymbolModule[] GetModulesCore(bool getResourceModules)
  {
    if (getResourceModules) throw new NotSupportedException();
    return Symbol.Modules.Select(x => _runtime.CreateModuleDelegator(x)).ToArray();
  }

  public override AssemblyName[] GetReferencedAssemblies()
  {
    throw new NotImplementedException();
  }

  protected override SymbolType? GetTypeCore(string name)
  {
    return GetTypeCore(name, false, false);
  }

  protected override SymbolType? GetTypeCore(string name, bool throwOnError)
  {
    return GetTypeCore(name, throwOnError, false);
  }

  protected override SymbolType? GetTypeCore(string name, bool throwOnError, bool ignoreCase)
  {
    GetTypeVisitor visitor = new GetTypeVisitor(_runtime, name, ignoreCase);
    SymbolType? type = visitor.VisitNamespace(Symbol.GlobalNamespace);
    return type is not null
      ? type
      : throwOnError ? throw new TypeLoadException() : null;
  }


  // System.Object overrides

  public override bool Equals(object? obj)
  {
    if (obj is not Assembly assembly)
      return false;

    return AssemblyEqualityComparer.Default.Equals(this, assembly);
  }

  public override int GetHashCode()
  {
    return AssemblyEqualityComparer.Default.GetHashCode(this);
  }

  public override string ToString()
  {
    return FullName;
  }


  // New members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolMethodInfo? EntryPoint => EntryPointCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new IEnumerable<SymbolType> ExportedTypes => (IEnumerable<SymbolType>)base.ExportedTypes;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolModule ManifestModule => ManifestModuleCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolModule[] Modules => (SymbolModule[])base.Modules;

  public new SymbolModule[] GetModules() => (SymbolModule[])base.GetModules();

  public new SymbolType[] GetTypes() => (SymbolType[])base.GetTypes();
}

internal abstract class SymbolAssemblyBase : Assembly
{
  private protected SymbolAssemblyBase() { }


  // System.Reflection.Assembly overrides

  public sealed override MethodInfo? EntryPoint => EntryPointCore;

  public sealed override Module ManifestModule => ManifestModuleCore;

  public sealed override Module[] GetLoadedModules(bool getResourceModules) => GetLoadedModulesCore(getResourceModules);

  public sealed override Module? GetModule(string name) => GetModuleCore(name);

  public sealed override Module[] GetModules(bool getResourceModules) => GetModulesCore(getResourceModules);

  public sealed override Type[] GetExportedTypes() => GetExportedTypesCore();

  public sealed override Type? GetType(string name) => GetTypeCore(name);

  public sealed override Type? GetType(string name, bool throwOnError) => GetTypeCore(name, throwOnError);

  public sealed override Type? GetType(string name, bool throwOnError, bool ignoreCase) => GetTypeCore(name, throwOnError, ignoreCase);


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolMethodInfo? EntryPointCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ManifestModuleCore { get; }

  protected abstract SymbolModule[] GetLoadedModulesCore(bool getResourceModules);

  protected abstract SymbolModule? GetModuleCore(string name);

  protected abstract SymbolModule[] GetModulesCore(bool getResourceModules);

  protected abstract SymbolType[] GetExportedTypesCore();

  protected abstract SymbolType? GetTypeCore(string name);

  protected abstract SymbolType? GetTypeCore(string name, bool throwOnError);

  protected abstract SymbolType? GetTypeCore(string name, bool throwOnError, bool ignoreCase);
}