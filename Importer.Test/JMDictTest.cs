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

	[Fact]
	public void Entry_Priority_returns_highest_priority()
	{
		var a = new JMDict.Entry
		{
			Kanji = {
				new JMDict.Kanji { Priority = { "news2" } },
				new JMDict.Kanji { Priority = { "x", "news1" } },
			},
			Reading = {
				new JMDict.Reading { Priority = { "spec2" } },
			},
		};
		var b = new JMDict.Entry
		{
			Reading = {
				new JMDict.Reading { Priority = { "news2" } },
				new JMDict.Reading { Priority = { "y", "news1" } },
			},
			Kanji = {
				new JMDict.Kanji { Priority = { "spec2" } },
			}
		};

		a.Priority.Should().Equal("x", "news1");
		b.Priority.Should().Equal("y", "news1");
	}

	[Fact]
	public void Entry_IsUsuallyKana_should_return_true_for_sense_with_uk_misc_tag()
	{
		var a = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { } },
		};
		var b = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { } },
			Sense = {
				new JMDict.Sense {
					Misc = { "x", "y" },
				},
			},
		};
		var c = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { } },
			Sense = {
				new JMDict.Sense {
					Misc = { "x", "y" },
				},
				new JMDict.Sense {
					Misc = { "x", JMDict.UsuallyKanaTag },
				},
			},
		};

		a.IsUsuallyKana.Should().BeFalse();
		b.IsUsuallyKana.Should().BeFalse();
		c.IsUsuallyKana.Should().BeTrue();

		Check("1000225", x =>
		{
			x.IsUsuallyKana.Should().BeTrue();
		});
		Check("1000220", x =>
		{
			x.IsUsuallyKana.Should().BeFalse();
		});
	}

	[Fact]
	public void Entry_IsUsuallyKana_should_return_true_for_entry_with_no_kanji()
	{
		var a = new JMDict.Entry
		{
			Reading = { new JMDict.Reading { } },
		};
		a.IsUsuallyKana.Should().BeTrue();
	}

	[Fact]
	public void Entry_GetFrequency_should_return_available_frequency()
	{
		var a1 = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { Text = "text" } },
		};
		var a2 = new JMDict.Entry
		{
			Kanji = {
				new JMDict.Kanji { Text = "x"},
				new JMDict.Kanji { Text = "text" },
			},
		};
		var b1 = new JMDict.Entry
		{
			Reading = { new JMDict.Reading { Text = "text" } },
		};
		var b2 = new JMDict.Entry
		{
			Kanji = {
				new JMDict.Kanji { Text = "none" },
			},
			Reading = {
				new JMDict.Reading { Text = "x" },
				new JMDict.Reading { Text = "text" },
			},
		};
		var c1 = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { Text = "x" } },
			Reading = { new JMDict.Reading { Text = "x" } },
		};
		var c2 = new JMDict.Entry { };

		var freq = new Dictionary.Frequency.Entry { InnocentCorpus = 123 };
		var mapper = (string x) => x == "text" ? freq : null;
		a1.GetFrequency(mapper)?.Item1.Should().Be(freq);
		a2.GetFrequency(mapper)?.Item1.Should().Be(freq);
		b1.GetFrequency(mapper)?.Item1.Should().Be(freq);
		b2.GetFrequency(mapper)?.Item1.Should().Be(freq);
		c1.GetFrequency(mapper).Should().BeNull();
		c2.GetFrequency(mapper).Should().BeNull();
	}

	[Fact]
	public void Entry_GetFrequency_should_return_highest_frequency()
	{
		const long MAX = 30;
		var freq = new Dictionary<string, Dictionary.Frequency.Entry>
		{
			["p1"] = new Dictionary.Frequency.Entry { InnocentCorpus = 30 },
			["p2"] = new Dictionary.Frequency.Entry { InnocentCorpus = 20 },
			["p3"] = new Dictionary.Frequency.Entry { InnocentCorpus = 10 },
		};

		var mapper = (string x) => freq.GetValueOrDefault(x);

		// Note: all entries should have the `uk` tag so that kanji and reading
		// have the same relative precedence

		var a = new JMDict.Entry
		{
			Kanji = {
				new JMDict.Kanji { Text = "p3"},
				new JMDict.Kanji { Text = "p2"},
				new JMDict.Kanji { Text = "p1"},
			},
			Sense = { UsuallyKana },
		};
		var b = new JMDict.Entry
		{
			Reading = {
				new JMDict.Reading { Text = "p3"},
				new JMDict.Reading { Text = "p2"},
				new JMDict.Reading { Text = "p1"},
			},
			Sense = { UsuallyKana },
		};
		var c = new JMDict.Entry
		{
			Kanji = {
				new JMDict.Kanji { Text = "p3"},
				new JMDict.Kanji { Text = "p2"},
			},
			Reading = {
				new JMDict.Reading { Text = "p2"},
				new JMDict.Reading { Text = "p1"},
			},
			Sense = { UsuallyKana },
		};
		var d = new JMDict.Entry
		{
			Kanji = {
				new JMDict.Kanji { Text = "p2"},
				new JMDict.Kanji { Text = "p1"},
			},
			Reading = {
				new JMDict.Reading { Text = "p3"},
				new JMDict.Reading { Text = "p2"},
			},
			Sense = { UsuallyKana },
		};

		a.GetFrequency(mapper)?.Item1.InnocentCorpus.Should().Be(MAX);
		b.GetFrequency(mapper)?.Item1.InnocentCorpus.Should().Be(MAX);
		c.GetFrequency(mapper)?.Item1.InnocentCorpus.Should().Be(MAX);
		d.GetFrequency(mapper)?.Item1.InnocentCorpus.Should().Be(MAX);
	}

	[Fact]
	public void Entry_GetFrequency_should_return_reliability()
	{
		var mapper = (string x) => x != "" ? new Dictionary.Frequency.Entry { InnocentCorpus = 1 } : null;

		// kanji is reliable
		var a = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { Text = "x" } },
		};
		// usually kana reading is reliable
		var b = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { } },
			Reading = { new JMDict.Reading { Text = "x" } },
			Sense = { UsuallyKana },
		};
		// reading alone is not reliable
		var c = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { } },
			Reading = { new JMDict.Reading { Text = "x" } },
		};

		a.GetFrequency(mapper)?.Item2.Should().BeTrue();
		b.GetFrequency(mapper)?.Item2.Should().BeTrue();
		c.GetFrequency(mapper)?.Item2.Should().BeFalse();
	}

	[Fact]
	public void Entry_GetFrequency_should_return_reliable_if_available()
	{
		var freq = new Dictionary<string, Dictionary.Frequency.Entry>
		{
			["p1"] = new Dictionary.Frequency.Entry { InnocentCorpus = 30 },
			["p2"] = new Dictionary.Frequency.Entry { InnocentCorpus = 20 },
			["p3"] = new Dictionary.Frequency.Entry { InnocentCorpus = 10 },
		};

		// always p2
		var expected = new Dictionary.Frequency.Entry { InnocentCorpus = 20 };

		var mapper = (string x) => freq.GetValueOrDefault(x);

		// reading has a higher precedence, but is not reliable
		var a = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { Text = "p2" } },
			Reading = { new JMDict.Reading { Text = "p1" } },
		};
		// no kanji returns the highest reading as unreliable
		var b = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { } },
			Reading = {
				new JMDict.Reading { Text = "p3" },
				new JMDict.Reading { Text = "p2" },
			},
		};
		// reliable reading returns the highest precedence
		var c = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { Text = "p3" } },
			Reading = { new JMDict.Reading { Text = "p2" } },
			Sense = { UsuallyKana },
		};
		var d = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { Text = "p2" } },
			Reading = { new JMDict.Reading { Text = "p3" } },
			Sense = { UsuallyKana },
		};

		a.GetFrequency(mapper).Should().Be((expected, true));
		b.GetFrequency(mapper).Should().Be((expected, false));
		c.GetFrequency(mapper).Should().Be((expected, true));
	}

	[Fact]
	public void Entry_GetSorterArgs_should_return_sort_arguments()
	{
		var innocent = new Dictionary<string, long> { ["p1"] = 30, ["p2"] = 20 };
		var worldlex = new Dictionary<string, Frequency.WorldLex>
		{
			["w1"] = new Frequency.WorldLex
			{
				Blog = new Frequency.WorldLex.Info { Frequency = 3 },
				News = new Frequency.WorldLex.Info { Frequency = 4 },
				Twitter = new Frequency.WorldLex.Info { Frequency = 5 },
			},
			["w2"] = new Frequency.WorldLex
			{
				Blog = new Frequency.WorldLex.Info { Frequency = 2 },
			},
		};

		var a = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { Text = "p1", Priority = { "news1" } } },
			Reading = { new JMDict.Reading { Priority = { "news2" } } },
		};
		var b = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { Priority = { "news2" } } },
			Reading = { new JMDict.Reading { Text = "w1", Priority = { "news1" } } },
		};

		var c = new JMDict.Entry
		{
			Kanji = { new JMDict.Kanji { Text = "none", Priority = { "news2" } } },
			Reading = { new JMDict.Reading { Text = "none", Priority = { "news1" } } },
		};

		a.GetSorterArgs(innocent, worldlex).Should().BeEquivalentTo(new Dictionary.Sorter.Args
		{
			Priority = new List<string> { "news1" },
			Frequency = new Dictionary.Frequency.Entry
			{
				InnocentCorpus = 30,
			},
			IsFrequencyReliable = true,
		});

		b.GetSorterArgs(innocent, worldlex).Should().BeEquivalentTo(new Dictionary.Sorter.Args
		{
			Priority = new List<string> { "news1" },
			Frequency = new Dictionary.Frequency.Entry
			{
				WorldLex = new Dictionary.Frequency.WorldLex { Blog = 3, News = 4, Twitter = 5 },
			},
			IsFrequencyReliable = false,
		});

		c.GetSorterArgs(innocent, worldlex).Should().BeEquivalentTo(new Dictionary.Sorter.Args
		{
			Priority = new List<string> { "news1" },
		});
	}

	private static JMDict.Sense UsuallyKana
	{
		get => new JMDict.Sense { Misc = { JMDict.UsuallyKanaTag } };
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
