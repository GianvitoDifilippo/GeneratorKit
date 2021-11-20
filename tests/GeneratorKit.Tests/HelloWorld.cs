using Xunit;

namespace GeneratorKit;

public class HelloWorld
{
  [Fact]
  public void GithubAction_ShouldSucceed()
  {
    System.Console.WriteLine("Hello, GitHub!");
  }
}