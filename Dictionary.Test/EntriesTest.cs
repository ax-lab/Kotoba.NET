public class EntriesTest
{
	[Fact]
	public void Count_is_greater_than_zero()
	{
		Entries.Count.Should().BeGreaterThan(0);
	}

	[Fact]
	public void ById_returns_null_for_inexistent_id()
	{
		var entry = Entries.ById(88888888888);
		entry.Should().BeNull();
	}

	[Fact]
	public void ById_returns_entry_with_id()
	{
		var entry = Entries.ById(1264540);
		entry.Should().NotBeNull();
		entry!.Id.Should().Be(1264540);
		entry.Kanji[0].Text.Should().Be("言葉");
		entry.Reading[0].Text.Should().Be("ことば");
		entry.Sense[0].Glossary.Select(x => x.Text).Should().Equal("language", "dialect");
		entry.Sense[1].Glossary.Select(x => x.Text).Should().Equal("word", "phrase", "expression", "term");
	}

	[Fact]
	public void ByIds_returns_entries_with_ids()
	{
		var entries = Entries.ByIds(1264540, 88888888888, 1417330).ToList();
		entries.Count.Should().Be(2);
		entries[0].Id.Should().Be(1264540);
		entries[0].Kanji[0].Text.Should().Be("言葉");
		entries[1].Id.Should().Be(1417330);
		entries[1].Kanji[0].Text.Should().Be("単語");
	}

	[Fact]
	public void ByIds_sorts_by_position()
	{
		var entries = Entries.ByIds(1264540, 1311110, 1417330).ToList();
		entries.Count.Should().Be(3);
		entries[0].Position.Should().BeGreaterThan(0);
		entries[1].Position.Should().BeGreaterThan(entries[0].Position);
		entries[2].Position.Should().BeGreaterThan(entries[1].Position);
	}

	[Fact]
	public void GetTag_returns_non_existent_tag()
	{
		Entries.GetTag("this-is-not-a-tag").Should().Be(new Tag("this-is-not-a-tag", ""));
	}

	[Fact]
	public void Tags_contain_entries_for_sense_misc()
	{
		Entries.Tags.Should().Contain(x => x.Name == "uk" && x.Info.Contains("written using kana"));
		Entries.Tags.Should().Contain(x => x.Name == "abbr" && x.Info == "abbreviation");
		Entries.GetTag("col").Info.Should().Be("colloquialism");
	}

	[Fact]
	public void List_returns_entries_at_the_right_offset()
	{
		var get = (long limit, long offset) =>
		{
			return Entries.List(limit, offset).Select(x => x.Position);
		};

		get(5, 0).Should().Equal(1, 2, 3, 4, 5);
		get(5, 1).Should().Equal(2, 3, 4, 5, 6);
		get(3, 99).Should().Equal(100, 101, 102);
	}
}
