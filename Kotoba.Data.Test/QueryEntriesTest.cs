public class QueryEntriesTest : TestHelper
{
	public QueryEntriesTest(ITestOutputHelper output) : base(output) { }

	[Fact]
	public async void can_load_entry_by_id()
	{
		const string query = @"query {
			entries {
				byId(id: 1264540) {
					id text
				}
			}
		}";
		await Run(query, data =>
		{
			var entry = data.SelectToken("$.data.entries.byId");
			var id = (long?)entry?.SelectToken("id");
			var text = (string?)entry?.SelectToken("text");
			id.Should().Be(1264540);
			text.Should().Be("言葉");
		});
	}

	[Fact]
	public async void can_load_entry_by_ids()
	{
		const string query = @"query {
			entries {
				byIds(ids: [1264540, 1417330]) {
					id text
				}
			}
		}";
		await Run(query, data =>
		{
			var entryA = data.SelectToken("$.data.entries.byIds[0]");
			var entryB = data.SelectToken("$.data.entries.byIds[1]");

			var idA = (long?)entryA?.SelectToken("id");
			var idB = (long?)entryB?.SelectToken("id");
			idA.Should().Be(1264540);
			idB.Should().Be(1417330);

			var textA = (string?)entryA?.SelectToken("text");
			var textB = (string?)entryB?.SelectToken("text");
			textA.Should().Be("言葉");
			textB.Should().Be("単語");
		});
	}
}
