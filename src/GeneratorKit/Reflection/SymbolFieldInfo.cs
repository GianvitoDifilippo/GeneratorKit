﻿using Microsoft.CodeAnalysis;
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
  private readonly GeneratorRuntime _runtime;
  private readonly SymbolType? _reflectedType;
  private FieldInfo? _runtimeField;

  public SymbolFieldInfo(GeneratorRuntime runtime, IFieldSymbol symbol)
  {
    _runtime = runtime;
    Symbol = symbol;
  }

  public SymbolFieldInfo(GeneratorRuntime runtime, IFieldSymbol symbol, SymbolType reflectedType)
    : this(runtime, symbol)
  {
    _reflectedType = reflectedType;
  }

  public IFieldSymbol Symbol { get; }

  public FieldInfo RuntimeField
  {
    get
    {
      if (_runtimeField is null)
      {
        BindingFlags bindingAttr =
          (Symbol.DeclaredAccessibility is Accessibility.Public ? BindingFlags.Public : BindingFlags.NonPublic) |
          (Symbol.IsStatic ? BindingFlags.Static : BindingFlags.Instance);

        _runtimeField = ReflectedTypeCore.UnderlyingSystemType.GetField(Name, bindingAttr);
      }
      return _runtimeField;
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

  public override RuntimeFieldHandle FieldHandle => throw new NotImplementedException();

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
      .Select(x => (CustomAttributeData)CompilationCustomAttributeData.FromAttributeData(_runtime, x))
      .ToList();
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

  public override object GetValue(object obj)
  {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit)
  {
    throw new NotImplementedException();
  }

  public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
  {
    throw new NotImplementedException();
  }


  // SymbolFieldInfoBase overrides

  protected override SymbolType DeclaringTypeCore => _runtime.CreateTypeDelegator(Symbol.ContainingType);

  protected override SymbolType FieldTypeCore => _runtime.CreateTypeDelegator(Symbol.Type);

  protected override SymbolModule ModuleCore => _runtime.CreateModuleDelegator(Symbol.ContainingModule);

  protected override SymbolType ReflectedTypeCore => _reflectedType ?? DeclaringTypeCore;


  // New members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType DeclaringType => DeclaringTypeCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType FieldType => FieldTypeCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolModule Module => ModuleCore;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public new SymbolType ReflectedType => ReflectedTypeCore;
}

internal abstract class SymbolFieldInfoBase : FieldInfo
{
  private protected SymbolFieldInfoBase() { }


  // System.Reflection.FieldInfo overrides

  public sealed override Type DeclaringType => DeclaringTypeCore;

  public sealed override Type FieldType => FieldTypeCore;

  public sealed override Module Module => ModuleCore;

  public sealed override Type ReflectedType => ReflectedTypeCore;


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType DeclaringTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType FieldTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ReflectedTypeCore { get; }
}