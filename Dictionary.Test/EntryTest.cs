public class EntryTest
{
	[Fact]
	public void loads_position()
	{
		var entry = Entries.ById(1264540);
		entry?.Position.Should().BeGreaterThan(0);
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
		// some senses are empty in the input XML, make sure they are not
		// imported
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

	private Entry GetEntry(long id)
	{
		return Entries.ById(id) ?? throw new Exception(String.Format("entry {0} not found", id));
	}
}
