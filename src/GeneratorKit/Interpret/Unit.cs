using System.Diagnostics;

namespace GeneratorKit.Interpret;

[DebuggerDisplay("<undefined>")]
internal sealed class Unit
{
  private Unit() { }

  public static readonly Unit Instance = new Unit();
}