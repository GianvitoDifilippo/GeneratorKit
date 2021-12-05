using SourceGenerator;

namespace GeneratorKit.Sample.Client
{
  public class MyGeneratorConfiguration : GeneratorConfiguration
  {
    public MyGeneratorConfiguration()
    {
      AddClassName("SampleClass1");
      AddClassName("SampleClass2");
    }
  }
}