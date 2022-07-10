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

	[Fact]
	public async void returns_kanji()
	{
		const string query = @"{
			entries {
				byId(id: 1264540) {
					kanji { text }
				}
			}
		}";
		await Run(query, data =>
		{
			var kanji = data.SelectTokens("$.data.entries.byId.kanji[*].text").Select(x => (string?)x).ToList();
			kanji.Should().Contain("言葉");
			kanji.Should().Contain("詞");
			kanji.Should().Contain("辞");
		});
	}

	[Fact]
	public async void returns_reading()
	{
		const string query = @"{
			entries {
				byId(id: 1311125) {
					reading { text }
				}
			}
		}";
		await Run(query, data =>
		{
			var reading = data.SelectTokens("$.data.entries.byId.reading[*].text").Select(x => (string?)x).ToList();
			reading.Should().Contain("あたし");
			reading.Should().Contain("あたくし");
		});
	}

	[Fact]
	public async void returns_sense_glossary_text()
	{
		const string query = @"{
			entries {
				byId(id: 1264540) {
					sense {
						glossary { text }
					}
				}
			}
		}";
		await Run(query, data =>
		{
			var senses = data.SelectTokens("$.data.entries.byId.sense[*].glossary[*].text").Select(x => (string?)x).ToList();
			senses.Should().Contain("language");
			senses.Should().Contain("dialect");
			senses.Should().Contain("word");
			senses.Should().Contain("remark");
		});
	}
}
