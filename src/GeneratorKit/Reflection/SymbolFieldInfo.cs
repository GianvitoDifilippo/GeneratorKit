using GeneratorKit.Comparers;
using GeneratorKit.Reflection.Binders;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolFieldInfo : SymbolFieldInfoBase
{
  private readonly IReflectionRuntime _runtime;
  private readonly IGeneratorContext _context;
  private readonly SymbolType? _reflectedType;
  private FieldInfo? _underlyingSystemField;

  public SymbolFieldInfo(IReflectionRuntime runtime, IGeneratorContext context, IFieldSymbol symbol, SymbolType? reflectedType)
  {
    _runtime = runtime;
    _context = context;
    OriginalSymbol = symbol;
    _reflectedType = reflectedType;
  }

  public IFieldSymbol OriginalSymbol { get; }

  public bool IsSource => OriginalSymbol.IsSource();

  public FieldInfo UnderlyingSystemField
  {
    get
    {
      Debug.Assert(!OriginalSymbol.IsSource());
      return _underlyingSystemField ??= DelegatorBinder.ResolveField(ReflectedType.UnderlyingSystemType, this);
    }
  }


  // System.Reflection.FieldInfo overrides

  public override FieldAttributes Attributes
  {
    get
    {
      FieldAttributes result = default;

      switch (OriginalSymbol.DeclaredAccessibility)
      {
        case Accessibility.Public:
          result |= FieldAttributes.Public;
          break;
        case Accessibility.ProtectedOrInternal:
          result |= FieldAttributes.FamORAssem;
          break;
        case Accessibility.Protected:
          result |= FieldAttributes.Family;
          break;
        case Accessibility.Internal:
          result |= FieldAttributes.Assembly;
          break;
        case Accessibility.ProtectedAndInternal:
          result |= FieldAttributes.FamANDAssem;
          break;
        case Accessibility.Private:
          result |= FieldAttributes.Private;
          break;
      }

      if (OriginalSymbol.IsStatic)
        result |= FieldAttributes.Static;
      if (OriginalSymbol.IsConst)
        result |= FieldAttributes.Literal;
      if (OriginalSymbol.IsReadOnly)
        result |= FieldAttributes.InitOnly;
      if (OriginalSymbol.HasConstantValue)
        result |= FieldAttributes.HasDefault;
      if (OriginalSymbol.GetAttributes().Any(x => x.AttributeClass!.ContainingNamespace.Name == "System" && x.AttributeClass!.Name == "NonSerializedAttribute"))
        result |= FieldAttributes.NotSerialized;

      return result;
    }
  }

  public override RuntimeFieldHandle FieldHandle => UnderlyingSystemField.FieldHandle;

  public override Type FieldType => _context.GetContextType(OriginalSymbol.Type);

  public override bool IsSecurityCritical => true;

  public override bool IsSecuritySafeCritical => false;

  public override bool IsSecurityTransparent => false;

  public override MemberTypes MemberType => MemberTypes.Field;

  public override int MetadataToken => throw new NotSupportedException();

  public override string Name => OriginalSymbol.Name;

  public override IList<CustomAttributeData> GetCustomAttributesData()
  {
    List<CustomAttributeData> result = OriginalSymbol
      .GetAttributes()
      .Select(data => CompilationCustomAttributeData.FromAttributeData(_context, data) as CustomAttributeData)
      .ToList();

    if (OriginalSymbol.IsImplicitlyDeclared)
    {
      INamedTypeSymbol compilerGeneratedAttributeSymbol = _runtime.Compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.CompilerGeneratedAttribute")!;
      result.Add(CompilationCustomAttributeData.FromParameterlessAttribute(_context, compilerGeneratedAttributeSymbol));

      INamedTypeSymbol debuggerBrowsableAttributeSymbol = _runtime.Compilation.GetTypeByMetadataName("System.Diagnostics.DebuggerBrowsableAttribute")!;
      IMethodSymbol constructor = debuggerBrowsableAttributeSymbol.Constructors[0];
      CustomAttributeTypedArgument[] arguments = new[] { new CustomAttributeTypedArgument(DebuggerBrowsableState.Never) };
      result.Add(CompilationCustomAttributeData.FromSymbol(_context, constructor, arguments, Array.Empty<CustomAttributeNamedArgument>()));
    }

    return new ReadOnlyCollection<CustomAttributeData>(result);
  }

  public override object[] GetCustomAttributes(bool inherit)
  {
    throw new NotSupportedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit)
  {
    throw new NotSupportedException();
  }

  public override Type[] GetOptionalCustomModifiers()
  {
    throw new NotSupportedException();
  }

  public override Type[] GetRequiredCustomModifiers()
  {
    throw new NotSupportedException();
  }

  public override object GetValue(object? obj)
  {
    return UnderlyingSystemField.GetValue(obj);
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
  {
    UnderlyingSystemField.SetValue(obj, value, invokeAttr, binder, culture);
  }


  // SymbolFieldInfoBase overrides

  protected override SymbolType DeclaringTypeCore => _context.CreateTypeDelegator(OriginalSymbol.ContainingType);

  protected override SymbolModule ModuleCore => _context.CreateModuleDelegator(OriginalSymbol.ContainingModule);

  protected override SymbolType ReflectedTypeCore => _reflectedType ?? DeclaringTypeCore;


  // System.Object overrides

  public override bool Equals(object? obj)
  {
    if (obj is not FieldInfo fieldInfo)
      return false;

    return FieldInfoEqualityComparer.Default.Equals(this, fieldInfo);
  }

  public override int GetHashCode()
  {
    return FieldInfoEqualityComparer.Default.GetHashCode(this);
  }

  public override string ToString()
  {
    return $"{FieldType.Name} {Name}";
  }


  // New members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType DeclaringType => DeclaringTypeCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolModule Module => ModuleCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType ReflectedType => ReflectedTypeCore;
}

internal abstract class SymbolFieldInfoBase : FieldInfo
{
  // System.Reflection.FieldInfo overrides

  public sealed override Type DeclaringType => DeclaringTypeCore;

  public sealed override Module Module => ModuleCore;

  public sealed override Type ReflectedType => ReflectedTypeCore;


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType DeclaringTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ReflectedTypeCore { get; }
}