using GeneratorKit.Comparers;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Reflection;

namespace GeneratorKit.Reflection;

internal sealed class SymbolEventInfo : SymbolEventInfoBase
{
  private readonly IReflectionRuntime _runtime;
  private readonly IGeneratorContext _context;
  private readonly SymbolType? _reflectedType;

  public SymbolEventInfo(IReflectionRuntime runtime, IGeneratorContext context, IEventSymbol symbol, SymbolType? reflectedType)
  {
    throw new NotSupportedException("Events are not supported");
    _runtime = runtime;
    _context = context;
    OriginalSymbol = symbol;
    _reflectedType = reflectedType;
  }

  public IEventSymbol OriginalSymbol { get; }


  // System.Reflection.EventInfo overrides

  public override EventAttributes Attributes => throw new NotImplementedException();

  public override string Name => OriginalSymbol.Name;

  protected override SymbolMethodInfo? GetAddMethodCore(bool nonPublic)
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


  // SymbolEventInfoBase overrides

  protected override SymbolType DeclaringTypeCore => _context.CreateTypeDelegator(OriginalSymbol.ContainingType);

  protected override SymbolType EventHandlerTypeCore => throw new NotImplementedException();

  protected override SymbolModule ModuleCore => throw new NotImplementedException();

  protected override SymbolType ReflectedTypeCore => _reflectedType ?? DeclaringTypeCore;

  protected override SymbolMethodInfo? GetRaiseMethodCore(bool nonPublic)
  {
    throw new NotImplementedException();
  }

  protected override SymbolMethodInfo? GetRemoveMethodCore(bool nonPublic)
  {
    throw new NotImplementedException();
  }


  // System.Object overrides

  public override bool Equals(object? obj)
  {
    if (obj is not EventInfo eventInfo)
      return false;

    return EventInfoEqualityComparer.Default.Equals(this, eventInfo);
  }

  public override int GetHashCode()
  {
    return EventInfoEqualityComparer.Default.GetHashCode(this);
  }
}

internal abstract class SymbolEventInfoBase : EventInfo
{
  // System.Reflection.EventInfo overrides

  public sealed override Type DeclaringType => DeclaringTypeCore;

  public sealed override Type EventHandlerType => EventHandlerTypeCore;

  public sealed override Module Module => ModuleCore;

  public sealed override Type ReflectedType => ReflectedTypeCore;

  public sealed override MethodInfo? GetAddMethod(bool nonPublic) => GetAddMethodCore(nonPublic);

  public sealed override MethodInfo? GetRaiseMethod(bool nonPublic) => GetRaiseMethodCore(nonPublic);

  public sealed override MethodInfo? GetRemoveMethod(bool nonPublic) => GetRemoveMethodCore(nonPublic);


  // Abstract members

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType DeclaringTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType EventHandlerTypeCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolModule ModuleCore { get; }

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected abstract SymbolType ReflectedTypeCore { get; }

  protected abstract SymbolMethodInfo? GetAddMethodCore(bool nonPublic);

  protected abstract SymbolMethodInfo? GetRaiseMethodCore(bool nonPublic);

  protected abstract SymbolMethodInfo? GetRemoveMethodCore(bool nonPublic);
}