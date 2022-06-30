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
		var entry = Entries.ById(1264540) ?? throw new Exception("entry not found");
		entry.Id.Should().Be(1264540);
		entry.Kanji[0].Text.Should().Be("言葉");
		entry.Reading[0].Text.Should().Be("ことば");
	}

	[Fact]
	public void Count_should_be_greater_than_zero()
	{
		Entries.Count.Should().BeGreaterThan(0);
	}
}
