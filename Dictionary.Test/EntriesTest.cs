public class EntriesTest
{
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
		var entries = Entries.ByIds(1264540, 88888888888, 1417330);
		entries.Count.Should().Be(2);
		entries[0].Id.Should().Be(1264540);
		entries[0].Kanji[0].Text.Should().Be("言葉");
		entries[1].Id.Should().Be(1417330);
		entries[1].Kanji[0].Text.Should().Be("単語");
	}

	[Fact]
	public void Count_should_be_greater_than_zero()
	{
		Entries.Count.Should().BeGreaterThan(0);
	}

	[Fact]
	public void loads_glossary_type()
	{
		var entry = GetEntry(1010290);
		entry.Sense[0].Glossary.First(x => x.Type == "lit").Text.Should().Contain("one x mark");
	}

	[Fact]
	public void does_not_have_empty_senses()
	{
		var entry = GetEntry(1016140);
		entry.Sense.Where(x => x.Glossary.Count == 0).Should().BeEmpty();
	}

	[Fact]
	public void loads_priority()
	{
		var entry = GetEntry(1001670);
		entry.Kanji[0].Priority.Should().Equal("news1", "nf23");
		entry.Kanji[1].Priority.Should().Equal("ichi2");
		entry.Reading[0].Priority.Should().Equal("ichi2", "news1", "nf23");
	}

	[Fact]
	public void loads_misc_info_for_senses()
	{
		var entry = GetEntry(1000320);
		entry.Sense[0].Misc.Select(x => x.Name).Should().Equal("uk");
		entry.Sense[1].Misc.Select(x => x.Name).Should().Equal("col", "uk");
	}

	[Fact]
	public void GetTag_returns_non_existent_tag()
	{
		Entries.GetTag("this-is-not-a-tag").Should().Be(new Tag("this-is-not-a-tag", ""));
	}

	[Fact]
	public void loads_tags_for_sense_misc()
	{
		Entries.Tags.Should().Contain(x => x.Name == "uk" && x.Info.Contains("written using kana"));
		Entries.Tags.Should().Contain(x => x.Name == "abbr" && x.Info == "abbreviation");
		Entries.GetTag("col").Info.Should().Be("colloquialism");
	}

	private Entry GetEntry(long id)
	{
		return Entries.ById(id) ?? throw new Exception(String.Format("entry {0} not found", id));
	}
}
