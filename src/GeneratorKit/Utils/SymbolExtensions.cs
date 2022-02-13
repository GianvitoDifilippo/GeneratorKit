using GeneratorKit.Reflection;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.CodeAnalysis;

internal static class SymbolExtensions
{
  public static bool IsSource(this ISymbol symbol)
  {
    return symbol.ContainingAssembly is ISourceAssemblySymbol;
  }

  public static IEnumerable<ISymbol> GetMembers(this ITypeSymbol symbol, BindingFlags bindingAttr, MemberFlags memberFlags, bool isReflectedType)
  {
    bool hasPublicFlag = bindingAttr.HasFlag(BindingFlags.Public);
    bool hasNonPublicFlag = bindingAttr.HasFlag(BindingFlags.NonPublic);
    bool hasInstanceFlag = bindingAttr.HasFlag(BindingFlags.Instance);
    bool hasStaticFlag = bindingAttr.HasFlag(BindingFlags.Static);
    bool hasFlattenHierarchyFlag = bindingAttr.HasFlag(BindingFlags.FlattenHierarchy);
    bool hasDeclaredOnlyFlag = bindingAttr.HasFlag(BindingFlags.DeclaredOnly);
    bool deepSearch = memberFlags.HasFlag(MemberFlags.DeepSearch);

    foreach (ISymbol member in symbol.GetMembers())
    {
      bool isField = member.Kind is SymbolKind.Field;
      if (isField && !memberFlags.HasFlag(MemberFlags.Fields))
        continue;
      bool isProperty = member.Kind is SymbolKind.Property;
      if (isProperty && !memberFlags.HasFlag(MemberFlags.Properties))
        continue;
      bool isConstructor = member.Kind is SymbolKind.Method && ((IMethodSymbol)member).MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor;
      if (isConstructor && !memberFlags.HasFlag(MemberFlags.Constructors))
        continue;
      bool isMethod = member.Kind is SymbolKind.Method && !isConstructor;
      if (isMethod && !memberFlags.HasFlag(MemberFlags.Methods))
        continue;
      bool isEvent = member.Kind is SymbolKind.Event;
      if (isEvent && !memberFlags.HasFlag(MemberFlags.Events))
        continue;
      bool isNestedType = member.Kind is SymbolKind.NamedType;
      if (isNestedType && !memberFlags.HasFlag(MemberFlags.NestedTypes))
        continue;

      bool isPublic = member.DeclaredAccessibility is Accessibility.Public;
      bool isInstance = !member.IsStatic;

      if (!isReflectedType && (isConstructor || isNestedType))
      {
        continue;
      }

      if (isPublic && hasPublicFlag)
      {
        if (isInstance && hasInstanceFlag || isNestedType || deepSearch)
        {
          yield return member;
        }
        else if (!isInstance && hasStaticFlag)
        {
          if (isReflectedType || (hasFlattenHierarchyFlag && member.DeclaredAccessibility is not Accessibility.Private) || deepSearch)
          {
            yield return member;
          }
        }
      }
      else if (!isPublic && hasNonPublicFlag)
      {
        if (isInstance && hasInstanceFlag || isNestedType)
        {
          if (isReflectedType || member.DeclaredAccessibility is not Accessibility.Private || deepSearch)
          {
            yield return member;
          }
        }
        else if (!isInstance && hasStaticFlag)
        {
          if (isReflectedType || (hasFlattenHierarchyFlag && member.DeclaredAccessibility is not Accessibility.Private) || deepSearch)
          {
            yield return member;
          }
        }
      }
    }

    if (!hasDeclaredOnlyFlag && symbol.BaseType is ITypeSymbol baseType)
    {
      foreach (ISymbol baseMember in baseType.GetMembers(bindingAttr, memberFlags, false))
      {
        yield return baseMember;
      }
    }
  }
}
