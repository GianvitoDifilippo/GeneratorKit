﻿using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratorKit.Interpret.Context;

internal interface IInterpreterContext
{
  Compilation Compilation { get; }
  object CreateInstance(Type type, object?[] arguments);

  Assembly GetAssembly(IAssemblySymbol symbol);
  ConstructorInfo GetConstructorInfo(IMethodSymbol symbol);
  FieldInfo GetFieldInfo(IFieldSymbol symbol);
  MethodInfo GetMethodInfo(IMethodSymbol symbol);
  Module GetModule(IModuleSymbol symbol);
  PropertyInfo GetPropertyInfo(IPropertySymbol symbol);
  Type GetType(ITypeSymbol symbol);
}