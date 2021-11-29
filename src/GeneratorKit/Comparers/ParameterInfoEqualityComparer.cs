using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Comparers;

public class ParameterInfoEqualityComparer : IEqualityComparer<ParameterInfo?>
{
  public static readonly ParameterInfoEqualityComparer Default = new ParameterInfoEqualityComparer();

  private ParameterInfoEqualityComparer() { }

  public bool Equals(ParameterInfo? x, ParameterInfo? y)
  {
    if (ReferenceEquals(x, y)) return true;

    if (x is null) return y is null;
    if (y is null) return false;

    if (!MemberInfoEqualityComparer.Default.Equals(x.Member, y.Member)) return false;

    return x.Position == y.Position;
  }

  public int GetHashCode(ParameterInfo? obj)
  {
    if (obj is null) return 0;

    int hashCode = MemberInfoEqualityComparer.Default.GetHashCode(obj.Member);
    hashCode = hashCode * 23 + obj.Position.GetHashCode();

    return hashCode;
  }
}