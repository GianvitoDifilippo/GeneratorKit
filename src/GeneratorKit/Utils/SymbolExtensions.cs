using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.CodeAnalysis;

internal static class SymbolExtensions
{
  public static bool IsSource(this ISymbol symbol)
  {
    return symbol.ContainingAssembly is ISourceAssemblySymbol;
  }

  public static IEnumerable<ISymbol> GetMembers(this ITypeSymbol symbol, BindingFlags bindingAttr, bool isReflectedType)
  {
    bool hasPublicFlag = bindingAttr.HasFlag(BindingFlags.Public);
    bool hasNonPublicFlag = bindingAttr.HasFlag(BindingFlags.NonPublic);
    bool hasInstanceFlag = bindingAttr.HasFlag(BindingFlags.Instance);
    bool hasStaticFlag = bindingAttr.HasFlag(BindingFlags.Static);
    bool hasFlattenHierarchyFlag = bindingAttr.HasFlag(BindingFlags.FlattenHierarchy);
    bool hasDeclaredOnlyFlag = bindingAttr.HasFlag(BindingFlags.DeclaredOnly);

    foreach (ISymbol member in symbol.GetMembers())
    {
      bool isPublic = member.DeclaredAccessibility is Accessibility.Public;
      bool isInstance = !member.IsStatic;
      bool isNestedType = member.Kind is SymbolKind.NamedType;
      bool isConstructor = member.Kind is SymbolKind.Method && ((IMethodSymbol)member).MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor;

      if (!isReflectedType && (isConstructor || isNestedType))
      {
        continue;
      }

      if (isPublic && hasPublicFlag)
      {
        if (isInstance && hasInstanceFlag || isNestedType)
        {
          yield return member;
        }
        else if (!isInstance && hasStaticFlag)
        {
          if (isReflectedType || (hasFlattenHierarchyFlag && member.DeclaredAccessibility is not Accessibility.Private))
          {
            yield return member;
          }
        }
      }
      else if (!isPublic && hasNonPublicFlag)
      {
        if (isInstance && hasInstanceFlag || isNestedType)
        {
          if (isReflectedType || member.DeclaredAccessibility is not Accessibility.Private)
          {
            yield return member;
          }
        }
        else if (!isInstance && hasStaticFlag)
        {
          if (isReflectedType || (hasFlattenHierarchyFlag && member.DeclaredAccessibility is not Accessibility.Private))
          {
            yield return member;
          }
        }
      }
    }

    if (!hasDeclaredOnlyFlag && symbol.BaseType is ITypeSymbol baseType)
    {
      foreach (ISymbol baseMember in baseType.GetMembers(bindingAttr, false))
      {
        yield return baseMember;
      }
    }
  }
}
