public class EntryTest
{
	[Fact]
	public void should_load_position()
	{
		var entry = Entries.ById(1264540);
		entry?.Position.Should().BeGreaterThan(0);
	}
}
