public class SchemaTest
{
	private readonly ITestOutputHelper output;

	public SchemaTest(ITestOutputHelper output)
	{
		this.output = output;
	}

	[Fact]
	public async void can_execute_a_simple_query()
	{
		var data = await Schema.Execute(@"query { entries { count } }");
		try
		{
			var actual = (long?)JToken.Parse(data).SelectToken("$.data.entries.count");
			actual.Should().NotBeNull().And.BeGreaterThan(0);
		}
		catch
		{
			output.WriteLine("");
			output.WriteLine("---------------- JSON ----------------");
			output.WriteLine(data);
			output.WriteLine("--------------------------------------");
			output.WriteLine("");
			throw;
		}
	}
}
