public class FrequencyTest
{
	[Fact]
	public void Get_returns_null_for_entry_not_available()
	{
		Frequency.Get("not-available").Should().BeNull();
	}

	[Fact]
	public void Get_returns_frequency_information()
	{
		var expectedFrequency = new Frequency.Entry
		{
			InnocentCorpus = 146448,
			WorldLex = new Frequency.WorldLex
			{
				Blog = 14786,
				Twitter = 17859,
				News = 10829,
			}
		};

		Frequency.Get("時間").Should().Be(expectedFrequency);
	}

	[Fact]
	public void handles_nullable_frequency_data()
	{
		var freqA = Frequency.Get("◎");
		freqA.Should().NotBeNull();
		freqA!.InnocentCorpus.Should().BeNull();
		freqA.WorldLex.Should().NotBeNull();

		var freqB = Frequency.Get("ああいう");
		freqB.Should().NotBeNull();
		freqB!.InnocentCorpus.Should().NotBeNull();
		freqB.WorldLex.Should().BeNull();
	}

	[Fact]
	public void Entry_compares_by_InnocentCorpus()
	{
		// sorted by precedence
		var a = new Frequency.Entry { InnocentCorpus = 2 };
		var b = new Frequency.Entry { InnocentCorpus = 1 };
		var c = new Frequency.Entry { InnocentCorpus = 0 };
		var d = new Frequency.Entry { InnocentCorpus = null };
		CheckEntryCompare(a, b, -1);
		CheckEntryCompare(a, c, -1);
		CheckEntryCompare(a, d, -1);
		CheckEntryCompare(a, null, -1);
		CheckEntryCompare(b, c, -1);
		CheckEntryCompare(b, d, -1);
		CheckEntryCompare(b, null, -1);
		CheckEntryCompare(c, d, 0);
		CheckEntryCompare(c, null, 0);
		CheckEntryCompare(d, null, 0);
	}

	[Fact]
	public void Entry_compares_by_WorldLex()
	{
		var F = (Frequency.WorldLex lex) => new Frequency.Entry { WorldLex = lex };
		var a = F(new Frequency.WorldLex { Blog = 2 });
		var b = F(new Frequency.WorldLex { Blog = 1 });
		var c = F(new Frequency.WorldLex { Blog = 0 });
		var d = new Frequency.Entry { WorldLex = null };
		CheckEntryCompare(a, b, -1);
		CheckEntryCompare(a, c, -1);
		CheckEntryCompare(a, d, -1);
		CheckEntryCompare(a, null, -1);
		CheckEntryCompare(b, c, -1);
		CheckEntryCompare(b, d, -1);
		CheckEntryCompare(b, null, -1);
		CheckEntryCompare(c, d, 0);
		CheckEntryCompare(c, null, 0);
		CheckEntryCompare(d, null, 0);
	}

	[Fact]
	public void Entry_compares_by_WorldLex_using_all_fields()
	{
		var F = (Frequency.WorldLex lex) => new Frequency.Entry { WorldLex = lex };
		var a1 = F(new Frequency.WorldLex { Blog = 5 });
		var a2 = F(new Frequency.WorldLex { News = 5 });
		var a3 = F(new Frequency.WorldLex { Twitter = 5 });
		var a4 = F(new Frequency.WorldLex { Blog = 3, News = 2 });
		var a5 = F(new Frequency.WorldLex { Blog = 3, Twitter = 2 });
		var a6 = F(new Frequency.WorldLex { Blog = 1, Twitter = 2, News = 2 });
		var b1 = F(new Frequency.WorldLex { Blog = 4 });
		var b2 = F(new Frequency.WorldLex { News = 4 });
		var b3 = F(new Frequency.WorldLex { Twitter = 4 });

		CheckEntryCompare(a1, a2, 0);
		CheckEntryCompare(a1, a3, 0);
		CheckEntryCompare(a1, a4, 0);
		CheckEntryCompare(a1, a5, 0);
		CheckEntryCompare(a1, a6, 0);

		CheckEntryCompare(a2, a3, 0);
		CheckEntryCompare(a2, a4, 0);
		CheckEntryCompare(a2, a5, 0);
		CheckEntryCompare(a2, a6, 0);

		CheckEntryCompare(a3, a4, 0);
		CheckEntryCompare(a3, a5, 0);
		CheckEntryCompare(a3, a6, 0);

		CheckEntryCompare(a4, a5, 0);
		CheckEntryCompare(a4, a6, 0);

		CheckEntryCompare(a5, a6, 0);

		CheckEntryCompare(a1, b1, -1);
		CheckEntryCompare(a1, b2, -1);
		CheckEntryCompare(a1, b3, -1);

		CheckEntryCompare(a2, b1, -1);
		CheckEntryCompare(a2, b2, -1);
		CheckEntryCompare(a2, b3, -1);

		CheckEntryCompare(a3, b1, -1);
		CheckEntryCompare(a3, b2, -1);
		CheckEntryCompare(a3, b3, -1);

		CheckEntryCompare(a4, b1, -1);
		CheckEntryCompare(a4, b2, -1);
		CheckEntryCompare(a4, b3, -1);

		CheckEntryCompare(a5, b1, -1);
		CheckEntryCompare(a5, b2, -1);
		CheckEntryCompare(a5, b3, -1);

		CheckEntryCompare(a6, b1, -1);
		CheckEntryCompare(a6, b2, -1);
		CheckEntryCompare(a6, b3, -1);
	}

	[Fact]
	public void Entry_comparison_prioritize_InnocentCorpus()
	{
		var a = new Frequency.Entry { InnocentCorpus = 2 };
		var b = new Frequency.Entry
		{
			InnocentCorpus = 1,
			WorldLex = new Frequency.WorldLex { Blog = 2 }
		};
		var c = new Frequency.Entry
		{
			InnocentCorpus = 1,
			WorldLex = new Frequency.WorldLex { Blog = 1 }
		};
		var d = new Frequency.Entry
		{
			InnocentCorpus = null,
			WorldLex = new Frequency.WorldLex { Blog = 99 }
		};

		CheckEntryCompare(a, b, -1);
		CheckEntryCompare(a, c, -1);
		CheckEntryCompare(a, d, -1);
		CheckEntryCompare(b, c, -1);
		CheckEntryCompare(b, d, -1);
		CheckEntryCompare(c, d, -1);
	}

	[Fact]
	public void Entry_IsEmpty_is_true_for_no_frequency_data()
	{
		var a = new Frequency.Entry { InnocentCorpus = null };
		var b = new Frequency.Entry { InnocentCorpus = 0 };
		var c = new Frequency.Entry { WorldLex = new Frequency.WorldLex { } };
		var d = new Frequency.Entry { WorldLex = new Frequency.WorldLex { }, InnocentCorpus = 0 };
		a.IsEmpty.Should().BeTrue();
		b.IsEmpty.Should().BeTrue();
		c.IsEmpty.Should().BeTrue();
		d.IsEmpty.Should().BeTrue();
	}

	[Fact]
	public void Entry_IsEmpty_is_false_for_any_frequency_data()
	{
		var a = new Frequency.Entry { InnocentCorpus = 1 };
		var b = new Frequency.Entry { WorldLex = new Frequency.WorldLex { Blog = 1 } };
		var c = new Frequency.Entry { WorldLex = new Frequency.WorldLex { News = 1 } };
		var d = new Frequency.Entry { WorldLex = new Frequency.WorldLex { Twitter = 1 } };
		a.IsEmpty.Should().BeFalse();
		b.IsEmpty.Should().BeFalse();
		c.IsEmpty.Should().BeFalse();
		d.IsEmpty.Should().BeFalse();
	}

	private static void CheckEntryCompare(Frequency.Entry a, Frequency.Entry? b, int expected)
	{
		a.CompareTo(b).Should().Be(expected);
		a.CompareTo(a).Should().Be(0, "compared with itself should be zero");
		if (b != null)
		{
			b.CompareTo(a).Should().Be(-expected, "reverse compare should be the reverse");
		}
	}
}
