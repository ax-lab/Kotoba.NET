namespace Importer.Test;

public class JMDict_Open
{
	[Fact]
	public void should_open_file()
	{
		using (var xml = JMDict.Open())
		{
			Assert.NotNull(xml);
			Assert.True(xml.Read());
		}
	}

	[Fact]
	public void should_read_entry()
	{
		using (var xml = JMDict.Open())
		{
			var entries = JMDict.ReadEntries(xml);
			var a = entries.First(x => x.Sequence == "1000000");
			Assert.Equal("1000000", a.Sequence);

			var b = entries.First(x => x.Sequence == "1000020");
			Assert.Equal("1000020", b.Sequence);

			var c = entries.First(x => x.Sequence == "1000060");
			Assert.Equal("1000060", c.Sequence);
		}
	}
}
