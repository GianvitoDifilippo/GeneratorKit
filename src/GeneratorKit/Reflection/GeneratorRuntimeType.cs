using System;

namespace GeneratorKit.Reflection;

internal abstract class GeneratorRuntimeType : Type, IRuntimeType
{
  SymbolNamedType IRuntimeType.Definition => RuntimeDefinition;

  Type[] IRuntimeType.TypeArguments => RuntimeTypeArguments;

  IRuntimeType? IRuntimeType.BaseType => RuntimeBaseType;

  bool IRuntimeType.IsSource => IsSource;

  protected abstract SymbolNamedType RuntimeDefinition { get; }

  protected abstract Type[] RuntimeTypeArguments { get; }

  protected abstract IRuntimeType? RuntimeBaseType { get; }

  protected abstract bool IsSource { get; }
}
