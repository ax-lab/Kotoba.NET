namespace Dictionary.Test;

public class Entries_ById
{
	[Fact]
	public void returns_null_for_non_existing_id()
	{
		var entry = Entries.ById(88888888888);
		Assert.Null(entry);
	}

	[Fact]
	public void loads_entry()
	{
		var entry = Entries.ById(1264540) ?? throw new Exception("entry not found");
		Assert.Equal(1264540, entry.Id);
		Assert.Equal("言葉", entry.Kanji[0].Text);
	}
}
