public class EntryTest
{
	[Fact]
	public void Position_loads()
	{
		var entry = Entries.ById(1264540);
		entry?.Position.Should().BeGreaterThan(0);
	}

	[Fact]
	public void Priority_loads_for_Kanji_and_Reading()
	{
		var entry = GetEntry(1001670);
		entry.Kanji[0].Priority.Should().Equal("news1", "nf23");
		entry.Kanji[1].Priority.Should().Equal("ichi2");
		entry.Reading[0].Priority.Should().Equal("ichi2", "news1", "nf23");
	}

	[Fact]
	public void SenseGlossary_Type_loads()
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
	public void EntrySense_loads_Misc()
	{
		var entry = GetEntry(1000320);
		entry.Sense[0].Misc.Select(x => x.Name).Should().Equal("uk");
		entry.Sense[1].Misc.Select(x => x.Name).Should().Equal("col", "uk");
	}

	[Fact]
	public void EntrySense_Glossary_loads_Text()
	{
		var entry = GetEntry(1264540);
		entry.Sense.Any(s => s.Glossary.Any(x => x.Text == "language")).Should().BeTrue();
		entry.Sense.Any(s => s.Glossary.Any(x => x.Text == "dialect")).Should().BeTrue();
		entry.Sense.Any(s => s.Glossary.Any(x => x.Text == "word")).Should().BeTrue();
		entry.Sense.Any(s => s.Glossary.Any(x => x.Text == "phrase")).Should().BeTrue();
	}

	[Fact]
	public void EntrySense_Glossary_loads_Type()
	{
		var entryA = GetEntry(1005340);
		entryA.Sense.Any(s => s.Glossary.Any(x => x.Type == "expl")).Should().BeTrue();

		var entryB = GetEntry(1010290);
		entryB.Sense.Any(s => s.Glossary.Any(x => x.Type == "lit")).Should().BeTrue();
	}

	private Entry GetEntry(long id)
	{
		return Entries.ById(id) ?? throw new Exception(String.Format("entry {0} not found", id));
	}
}
