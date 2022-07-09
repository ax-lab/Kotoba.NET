public class QueryEntryTest : TestHelper
{
	public QueryEntryTest(ITestOutputHelper output) : base(output) { }

	[Fact]
	public async void returns_position()
	{
		const string query = @"{
			entries {
				byIds(ids: [1264540, 1311110, 1417330]) {
					position
				}
			}
		}";
		await Run(query, data =>
		{
			var positions = data.SelectTokens("$.data.entries.byIds[*].position").Select(x => (long)x).ToList();
			positions.Count.Should().Be(3);
			positions.All(pos => pos > 0).Should().BeTrue();
		});
	}
}
