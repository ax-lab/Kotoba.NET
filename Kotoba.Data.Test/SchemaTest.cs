public class SchemaTest : TestHelper
{
	public SchemaTest(ITestOutputHelper output) : base(output) { }

	[Fact]
	public async void can_execute_a_simple_query()
	{
		await Run(@"query { entries { count } }", data =>
		{
			var actual = (long?)data.SelectToken("$.data.entries.count");
			actual.Should().NotBeNull().And.BeGreaterThan(0);
		});
	}
}
