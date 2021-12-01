using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Comparers;

internal class CustomAttributeDataEqualityComparer : IEqualityComparer<CustomAttributeData?>
{
  public static readonly CustomAttributeDataEqualityComparer Default = new CustomAttributeDataEqualityComparer();

  private CustomAttributeDataEqualityComparer() { }

  public bool Equals(CustomAttributeData? x, CustomAttributeData? y)
  {
    if (ReferenceEquals(x, y)) return true;

    if (x is null) return y is null;
    if (y is null) return false;

    if (x.ConstructorArguments.Count != y.ConstructorArguments.Count) return false;
    if (x.NamedArguments.Count != y.NamedArguments.Count) return false;

    if (!TypeEqualityComparer.Default.Equals(x.AttributeType, y.AttributeType)) return false;

    if (!ConstructorInfoEqualityComparer.Default.Equals(x.Constructor, y.Constructor)) return false;

    for (int i = 0; i < x.ConstructorArguments.Count; i++)
    {
      CustomAttributeTypedArgument arg1 = x.ConstructorArguments[i];
      CustomAttributeTypedArgument arg2 = y.ConstructorArguments[i];
      if (!TypeEqualityComparer.Default.Equals(arg1.ArgumentType, arg2.ArgumentType)) return false;
      if (!arg1.Value.Equals(arg2.Value)) return false;
    }

    for (int i = 0; i < x.NamedArguments.Count; i++)
    {
      CustomAttributeNamedArgument arg1 = x.NamedArguments[i];
      CustomAttributeNamedArgument arg2 = y.NamedArguments[i];

      if (!MemberInfoEqualityComparer.Default.Equals(arg1.MemberInfo, arg2.MemberInfo)) return false;
      if (!TypeEqualityComparer.Default.Equals(arg1.TypedValue.ArgumentType, arg2.TypedValue.ArgumentType)) return false;
      if (!arg1.TypedValue.Value.Equals(arg2.TypedValue.Value)) return false;
    }

    return true;
  }

  public int GetHashCode(CustomAttributeData? obj)
  {
    if (obj is null) return 0;

    unchecked
    {
      int hashCode = TypeEqualityComparer.Default.GetHashCode(obj.AttributeType);
      foreach (CustomAttributeTypedArgument arg in obj.ConstructorArguments)
      {
        hashCode = hashCode * 23 + TypeEqualityComparer.Default.GetHashCode(arg.ArgumentType);
        hashCode = hashCode * 23 + arg.Value?.GetHashCode() ?? 0;
      }
      foreach (CustomAttributeNamedArgument arg in obj.NamedArguments)
      {
        hashCode = hashCode * 23 + MemberInfoEqualityComparer.Default.GetHashCode(arg.MemberInfo);
        hashCode = hashCode * 23 + TypeEqualityComparer.Default.GetHashCode(arg.TypedValue.ArgumentType);
        hashCode = hashCode * 23 + arg.TypedValue.Value?.GetHashCode() ?? 0;
      }

      return hashCode;
    }
  }
}