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
		Assert.Equal(entry.Id, 1264540ul);
		Assert.Equal(entry.Kanji[0].Text, "言葉");
	}
}
