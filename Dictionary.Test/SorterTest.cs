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

	[Theory]
	[InlineData("news1", 0)]
	[InlineData("ichi1", 0)]
	[InlineData("spec1", 0)]
	[InlineData("spec2", 1)]
	[InlineData("gai1", 2)]
	[InlineData("gai2", 3)]
	[InlineData("news2", 4)]
	[InlineData("ichi2", 5)]
	[InlineData("nf01", 101)]
	[InlineData("nf02", 102)]
	[InlineData("nf99", 199)]
	[InlineData("x", 999)]
	[InlineData("y", 999)]
	[InlineData("z", 999)]
	public void GetPriorityGroup_returns_tag_group(string tag, int expectedPriority)
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
	}
}
