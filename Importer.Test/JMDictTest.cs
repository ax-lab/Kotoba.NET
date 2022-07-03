public class JMDictTest : IClassFixture<JMDictTest.Fixture>
{
	private readonly Fixture fixture;

	public JMDictTest(Fixture fixture)
	{
		this.fixture = fixture;
	}

	[Fact]
	public void opens_file()
	{
		using (var dict = JMDict.Open())
		{
			var list = dict.ReadEntries().Take(10).ToList();
			list.Count.Should().Be(10);
		}
	}

	[Fact]
	public void reads_entries()
	{
		Check("1000000", x =>
		{
			x.Reading[0].Text.Should().Be("ヽ");
		});

		Check("1000040", x =>
		{
			x.Reading[0].Text.Should().Be("おなじ");
			x.Reading[1].Text.Should().Be("おなじく");
		});

		Check("1000110", x =>
		{
			x.Kanji[0].Text.Should().Be("ＣＤプレーヤー");
			x.Kanji[1].Text.Should().Be("ＣＤプレイヤー");
		});

		Check("1003810", x =>
		{
			x.Kanji[0].Text.Should().Be("草臥れる");
		});
	}

	[Fact]
	public void reads_sense_glossary()
	{
		Check("1000300", x =>
		{
			x.Sense[0].Glossary.Select(x => x.Text).Should().Equal("to treat", "to handle", "to deal with");
		});
		Check("1000440", x =>
		{
			x.Sense[1].Glossary.Select(x => x.Text).Should().Equal("you");
		});
	}

	[Fact]
	public void reads_sense_glossary_language()
	{
		Check("1000160", x =>
		{
			x.Sense.First(x => x.Lang == "spa").Glossary[0].Text.Should().Be("camiseta");
		});
	}

	[Fact]
	public void reads_sense_glossary_type()
	{
		Check("1000020", x =>
		{
			x.Sense[0].Glossary[0].Should().Be(new JMDict.Glossary
			{
				Type = "expl",
				Text = "repetition mark in hiragana"
			});
		});
	}

	[Fact]
	public void read_sense_glossary_misc()
	{
		Check("1000130", x =>
		{
			x.Sense[0].Misc.Should().Equal("abbr");
		});

		Check("1000320", x =>
		{
			x.Sense[0].Misc.Should().Equal("uk");
			x.Sense[1].Misc.Should().Equal("col", "uk");
		});

		fixture.Tags.Should().ContainKey("uk").WhoseValue.Contains("written using kana");
		fixture.Tags.Should().ContainKey("abbr").WhoseValue.Equals("abbreviation");
	}

	[Fact]
	public void reads_priority()
	{
		Check("1001670", x =>
		{
			x.Kanji[0].Priority.Should().Equal("news1", "nf23");
			x.Kanji[1].Priority.Should().Equal("ichi2");
			x.Reading[0].Priority.Should().Equal("ichi2", "news1", "nf23");
		});
	}

	private void Check(string id, Action<JMDict.Entry> assertions)
	{
		var entry = fixture.Get(id);
		entry.Sequence.Should().Be(id);
		assertions(entry);
	}

	public class Fixture
	{
		private readonly Dictionary<string, JMDict.Entry> entries;

		public readonly IReadOnlyDictionary<string, string> Tags;

		public Fixture()
		{
			using (var dict = JMDict.Open())
			{
				// avoid reading the entire file to keep the tests fast
				entries = dict
					.ReadEntries()
					.TakeWhile(x => String.Compare(x.Sequence, "1009999") < 0)
					.ToDictionary(x => x.Sequence);
				this.Tags = dict.Tags;
			}
		}

		public JMDict.Entry Get(string id)
		{
			JMDict.Entry? entry;
			if (entries.TryGetValue(id, out entry))
			{
				return entry;
			}
			throw new KeyNotFoundException(String.Format("Entry {0} not found in the test fixture", id));
		}
	}
}
