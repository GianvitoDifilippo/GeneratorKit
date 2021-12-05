using System.Collections.Generic;

namespace SourceGenerator
{
  public class GeneratorConfiguration
  {
    private readonly HashSet<string> _classNames;

    public GeneratorConfiguration()
    {
      _classNames = new HashSet<string>();
    }

    public IEnumerable<string> ClassNames => _classNames;

    protected void AddClassName(string name)
    {
      _classNames.Add(name);
    }
  }
}