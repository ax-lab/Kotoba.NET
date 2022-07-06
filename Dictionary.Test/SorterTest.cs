public class SorterTest
{
	[Fact]
	public void empty_entries_compare_as_zero()
	{
		var result = Sorter.Compare(new Sorter.Args(), new Sorter.Args());
		result.Should().Be(0);
	}

	[Fact]
	public void sorts_by_tag_group()
	{
		// Plain sort
		CheckSorted(1,
			new Sorter.Args
			{
				Priority = new string[] { "news1" }
			},
			new Sorter.Args
			{
				Priority = new string[] { "spec2" }
			},
			new Sorter.Args
			{
				Priority = new string[] { "news2" }
			},
			new Sorter.Args
			{
				Priority = new string[] { "none" }
			}
		);

		// Sorts by highest tag only
		CheckSorted(1,
			new Sorter.Args
			{
				Priority = new string[] { "x", "ichi2", "news1" }
			},
			new Sorter.Args
			{
				Priority = new string[] { "gai2", "spec2" }
			},
			new Sorter.Args
			{
				Priority = new string[] { "ichi2", "news2" }
			},
			new Sorter.Args
			{
				Priority = new string[] { "none" }
			}
		);

		// Ignores unknown tags
		CheckSorted(0,
			new Sorter.Args
			{
				Priority = new string[] { "x", "y", "z" }
			},
			new Sorter.Args
			{
				Priority = null,
			}
		);
	}

	[Fact]
	public void sort_by_frequency_tags()
	{
		CheckSorted(1,
			new Sorter.Args
			{
				Priority = new string[] { "news1", "nf10" }
			},
			new Sorter.Args
			{
				Priority = new string[] { "news1", "nf11" }
			},
			new Sorter.Args
			{
				Priority = new string[] { "spec1", "nf12" }
			},
			new Sorter.Args
			{
				Priority = new string[] { "spec2", "nf01" }
			},
			new Sorter.Args
			{
				Priority = new string[] { "spec2", "nf02" }
			},
			new Sorter.Args
			{
				Priority = new string[] { "x", "nf01" }
			},
			new Sorter.Args
			{
				Priority = new string[] { "x", "nf02" }
			},
			new Sorter.Args
			{
				Priority = new string[] { "nf03" }
			},
			new Sorter.Args
			{
				Priority = new string[] { "x", "nf04" }
			},
			new Sorter.Args
			{
				Priority = null
			}
		);
	}

	[Fact]
	public void sort_by_innocent_corpus()
	{
		CheckSorted(-1,
			// from lowest to highest
			new Sorter.Args
			{
				Priority = new string[] { "spec2" },
				Frequency = new Frequency.Entry { InnocentCorpus = 1 },
			},
			new Sorter.Args
			{
				Priority = new string[] { "spec2" },
				Frequency = new Frequency.Entry { InnocentCorpus = 2 },
			},
			new Sorter.Args
			{
				Priority = new string[] { "spec1" },
				Frequency = new Frequency.Entry { InnocentCorpus = null },
			},
			new Sorter.Args
			{
				Priority = new string[] { "spec1" },
				Frequency = new Frequency.Entry { InnocentCorpus = 1 },
			},
			new Sorter.Args
			{
				Priority = new string[] { "spec1" },
				Frequency = new Frequency.Entry { InnocentCorpus = 2 },
			}
		);
	}

	[Fact]
	public void sort_by_world_lex()
	{
		var F = (Frequency.WorldLex lex) => new Frequency.Entry { WorldLex = lex };
		CheckSorted(-1,
			// from lowest to highest
			new Sorter.Args
			{
				Priority = new string[] { "spec2" },
				Frequency = F(new Frequency.WorldLex { Blog = 1 }),
			},
			new Sorter.Args
			{
				Priority = new string[] { "spec2" },
				Frequency = F(new Frequency.WorldLex { Blog = 2 }),
			},
			new Sorter.Args
			{
				Priority = new string[] { "spec1" },
				Frequency = null,
			},
			new Sorter.Args
			{
				Priority = new string[] { "spec1" },
				Frequency = F(new Frequency.WorldLex { Blog = 1 }),
			},
			new Sorter.Args
			{
				Priority = new string[] { "spec1" },
				Frequency = F(new Frequency.WorldLex { Blog = 2 }),
			}
		);
	}

	[Theory]
	[InlineData("news1", 0)]
	[InlineData("ichi1", 0)]
	[InlineData("spec1", 0)]
	[InlineData("spec2", 1)]
	[InlineData("gai1", 2)]
	[InlineData("gai2", 3)]
	[InlineData("news2", 4)]
	[InlineData("ichi2", 5)]
	[InlineData("nf01", null)]
	[InlineData("x", null)]
	[InlineData("y", null)]
	[InlineData("z", null)]
	public void GetPriorityGroup_returns_tag_group(string tag, int? expectedPriority)
	{
		Sorter.GetPriorityGroup(tag).Should().Be(expectedPriority);
	}

	private static void CheckSorted(int order, params Sorter.Args[] args)
	{
		var msg = (int i, int j) =>
			String.Format("comparing #{0} and #{1}{2}", i, j, i > j ? " (rev)" : "");
		var shouldBeEqual = (int actual, int i, int j) =>
			actual.Should().Be(0, "both should be equal - " + msg(i, j));
		var shouldBeHigher = (int actual, int i, int j) =>
			actual.Should().BeLessThan(0, "first should have higher precedence - " + msg(i, j));
		var shouldBeLower = (int actual, int i, int j) =>
			actual.Should().BeGreaterThan(0, "first should have lower precedence - " + msg(i, j));
		var check = order == 0 ? shouldBeEqual :
			(order < 0 ? shouldBeLower : shouldBeHigher);
		var revCheck = order == 0 ? shouldBeEqual :
			(order < 0 ? shouldBeHigher : shouldBeLower);

		for (var i = 0; i < args.Length - 1; i++)
		{
			for (var j = i + 1; j < args.Length; j++)
			{
				check(Sorter.Compare(args[i], args[j]), i, j);
				revCheck(Sorter.Compare(args[j], args[i]), j, i);
			}
		}

		// sanity check that comparing any entry to itself returns as equal
		for (var i = 0; i < args.Length; i++)
		{
			shouldBeEqual(Sorter.Compare(args[i], args[i]), i, i);
		}
	}
}
