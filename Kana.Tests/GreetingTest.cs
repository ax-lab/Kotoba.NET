namespace Kana.Tests;

public class Greeting_GetHello
{
	[Fact]
	public void should_return_non_empty() => Assert.NotEmpty(Greeting.GetHello());

	[Fact]
	public void should_greet() => Assert.Contains("こんにちは", Greeting.GetHello());
}
