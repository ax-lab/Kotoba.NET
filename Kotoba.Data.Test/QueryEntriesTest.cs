public class QueryEntriesTest : TestHelper
{
	public QueryEntriesTest(ITestOutputHelper output) : base(output) { }

	[Fact]
	public async void loads_by_id()
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
	public async void loads_by_ids()
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

	[Fact]
	public async void loads_paged()
	{
		const int defaultPageSize = (int)QueryEntries.DefaultPageSize;

		const string query = @"query {
			entries {
				listA: list {
					position
				}
				listB: list(limit: 5) {
					position
				}
				listC: list(offset: 99) {
					position
				}
				listD: list(limit: 3, offset: 99) {
					position
				}
			}
		}";
		await Run(query, data =>
		{
			var listA = data.SelectTokens("$.data.entries.listA[*].position").Select(x => (long)x);
			var listB = data.SelectTokens("$.data.entries.listB[*].position").Select(x => (long)x);
			var listC = data.SelectTokens("$.data.entries.listC[*].position").Select(x => (long)x);
			var listD = data.SelectTokens("$.data.entries.listD[*].position").Select(x => (long)x);
			listA.Should().StartWith(new long[] { 1, 2, 3, 4, 5 }).And.HaveCount(defaultPageSize);
			listB.Should().Equal(1, 2, 3, 4, 5);
			listC.Should().StartWith(new long[] { 100, 101, 102 }).And.HaveCount(defaultPageSize);
			listD.Should().Equal(100, 101, 102);
		});
	}
}
