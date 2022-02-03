﻿using System;
using System.Collections.Generic;

namespace GeneratorKit.Reflection;

internal interface IRuntimeType
{
  IRuntimeType? BaseType { get; }
  IRuntimeType ElementType { get; }
  IRuntimeType DeclaringType { get; }
  IEnumerable<IRuntimeType> Interfaces { get; }
  SymbolType Definition { get; }
  Type[] TypeArguments { get; }
  Type[] TypeParameters { get; }
  string AssemblyQualifiedName { get; }
  bool HasElementType { get; }
  bool IsConstructedGenericType { get; }
  bool IsGenericParameter { get; }
  Type RuntimeType { get; }
  bool IsArray { get; }
  int ArrayRank { get; }
  bool IsPointer { get; }
  bool IsByRef { get; }
  bool ContainsGenericParameters { get; }
  int GenericParameterPosition { get; }
  string? FullName { get; }
}
