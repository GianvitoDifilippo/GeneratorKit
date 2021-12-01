using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Comparers;

internal class AssemblyEqualityComparer : IEqualityComparer<Assembly?>
{
  public static readonly AssemblyEqualityComparer Default = new AssemblyEqualityComparer();

  private AssemblyEqualityComparer() { }

  public bool Equals(Assembly? x, Assembly? y)
  {
    if (ReferenceEquals(x, y)) return true;

    if (x is null) return y is null;
    if (y is null) return false;

    return x.FullName == y.FullName;
  }

  public int GetHashCode(Assembly? obj)
  {
    if (obj is null) return 0;

    return obj.FullName.GetHashCode();
  }
}