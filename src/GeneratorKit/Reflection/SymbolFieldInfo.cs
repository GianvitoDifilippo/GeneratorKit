using GeneratorKit.Comparers;
using GeneratorKit.Reflection.Binders;
using GeneratorKit.Reflection.Context;
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
  private readonly IReflectionContext _context;
  private readonly SymbolType? _reflectedType;
  private FieldInfo? _underlyingSystemField;

  public SymbolFieldInfo(IReflectionContext context, IFieldSymbol symbol, SymbolType? reflectedType)
  {
    _context = context;
    Symbol = symbol;
    _reflectedType = reflectedType;
  }

  public IFieldSymbol Symbol { get; }

  public bool IsSource => Symbol.IsSource();

  public FieldInfo UnderlyingSystemField
  {
    get
    {
      Debug.Assert(!Symbol.IsSource(), "Field must not be source.");
      return _underlyingSystemField ??= DelegatorBinder.ResolveField(ReflectedType.UnderlyingSystemType, this);
    }
  }


  // System.Reflection.FieldInfo overrides

  public override FieldAttributes Attributes
  {
    get
    {
      FieldAttributes result = default;

      switch (Symbol.DeclaredAccessibility)
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

      if (Symbol.IsStatic)
        result |= FieldAttributes.Static;
      if (Symbol.IsConst)
        result |= FieldAttributes.Literal;
      if (Symbol.IsReadOnly)
        result |= FieldAttributes.InitOnly;
      if (Symbol.HasConstantValue)
        result |= FieldAttributes.HasDefault;
      if (Symbol.GetAttributes().Any(x => x.AttributeClass!.ContainingNamespace.Name == "System" && x.AttributeClass!.Name == "NonSerializedAttribute"))
        result |= FieldAttributes.NotSerialized;

      return result;
    }
  }

  public override RuntimeFieldHandle FieldHandle => UnderlyingSystemField.FieldHandle;

  public override Type FieldType => _context.GetContextType(Symbol.Type);

  public override bool IsSecurityCritical => true;

  public override bool IsSecuritySafeCritical => false;

  public override bool IsSecurityTransparent => false;

  public override MemberTypes MemberType => MemberTypes.Field;

  public override int MetadataToken => throw new NotSupportedException();

  public override string Name => Symbol.Name;

  public override IList<CustomAttributeData> GetCustomAttributesData()
  {
    List<CustomAttributeData> result = Symbol
      .GetAttributes()
      .Select(data => CompilationCustomAttributeData.FromAttributeData(_context, data) as CustomAttributeData)
      .ToList();

    if (Symbol.IsImplicitlyDeclared)
    {
      INamedTypeSymbol compilerGeneratedAttributeSymbol = _context.Compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.CompilerGeneratedAttribute")!;
      result.Add(CompilationCustomAttributeData.FromParameterlessAttribute(_context, compilerGeneratedAttributeSymbol));

      INamedTypeSymbol debuggerBrowsableAttributeSymbol = _context.Compilation.GetTypeByMetadataName("System.Diagnostics.DebuggerBrowsableAttribute")!;
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

  public override object? GetValue(object? obj)
  {
    return _context.GetField(this, obj);
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
  {
    _context.SetField(this, obj, value);
  }


  // SymbolFieldInfoBase overrides

  protected override SymbolNamedType DeclaringTypeCore => _context.CreateTypeDelegator(Symbol.ContainingType);

  protected override SymbolModule ModuleCore => _context.CreateModuleDelegator(Symbol.ContainingModule);

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
  public new SymbolNamedType DeclaringType => DeclaringTypeCore;

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
  protected abstract SymbolNamedType DeclaringTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ReflectedTypeCore { get; }
}