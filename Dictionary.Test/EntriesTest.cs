namespace Dictionary.Test;

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
	public void should_not_have_empty_senses()
	{
		var entry = GetEntry(1016140);
		entry.Sense.Where(x => x.Glossary.Count == 0).Should().BeEmpty();
	}

	private Entry GetEntry(long id)
	{
		return Entries.ById(id) ?? throw new Exception(String.Format("entry {0} not found", id));
	}
}
