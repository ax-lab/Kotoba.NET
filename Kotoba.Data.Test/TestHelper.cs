public abstract class TestHelper
{
	protected readonly ITestOutputHelper Output;

	public TestHelper(ITestOutputHelper output)
	{
		this.Output = output;
	}

	protected async Task Run(string query, Action<JToken> check)
	{
		var data = await Schema.Execute(query.Replace("'", "\""));
		var json = JToken.Parse(data);
		try
		{
			check(json);
		}
		catch
		{
			Output.WriteLine("");
			Output.WriteLine("---------------- JSON ----------------");
			Output.WriteLine(data);
			Output.WriteLine("--------------------------------------");
			Output.WriteLine("");
			throw;
		}
	}
}
