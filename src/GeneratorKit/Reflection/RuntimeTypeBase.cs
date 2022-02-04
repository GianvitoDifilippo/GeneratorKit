using System;
using System.Collections.Generic;

namespace GeneratorKit.Reflection;

internal abstract class RuntimeTypeBase : Type, IRuntimeType
{
  IRuntimeType? IRuntimeType.BaseType => RuntimeBaseType;
  IRuntimeType IRuntimeType.ElementType => RuntimeElementType;
  IRuntimeType? IRuntimeType.DeclaringType => RuntimeDeclaringType;
  IEnumerable<IRuntimeType> IRuntimeType.Interfaces => RuntimeInterfaces;
  SymbolType IRuntimeType.Definition => RuntimeDefinition;
  Type[] IRuntimeType.TypeArguments => IsGenericType ? GetGenericArguments() : EmptyTypes;
  Type[] IRuntimeType.TypeParameters => RuntimeTypeParameters;
  Type IRuntimeType.RuntimeType => RuntimeType;
  int IRuntimeType.ArrayRank => GetArrayRank();

  protected abstract IRuntimeType? RuntimeBaseType { get; }
  protected abstract IRuntimeType RuntimeElementType { get; }
  protected abstract IRuntimeType? RuntimeDeclaringType { get; }
  protected abstract IEnumerable<IRuntimeType> RuntimeInterfaces { get; }
  protected abstract SymbolType RuntimeDefinition { get; }
  protected abstract Type[] RuntimeTypeParameters { get; }
  protected abstract Type RuntimeType { get; }
}
