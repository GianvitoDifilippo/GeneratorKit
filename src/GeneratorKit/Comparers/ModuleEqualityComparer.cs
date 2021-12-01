using System.Collections.Generic;
using System.Reflection;

namespace GeneratorKit.Comparers;

internal class ModuleEqualityComparer : IEqualityComparer<Module?>
{
  public static readonly ModuleEqualityComparer Default = new ModuleEqualityComparer();

  public bool Equals(Module? x, Module? y)
  {
    if (ReferenceEquals(x, y)) return true;

    if (x is null) return y is null;
    if (y is null) return false;

    if (!AssemblyEqualityComparer.Default.Equals(x.Assembly, y.Assembly)) return false;

    return x.ScopeName == y.ScopeName;
  }

  public int GetHashCode(Module? obj)
  {
    if (obj is null) return 0;

    int hashCode = AssemblyEqualityComparer.Default.GetHashCode(obj.Assembly);
    hashCode = hashCode * 23 + obj.Name.GetHashCode();

    return hashCode;
  }
}