namespace Importer.Test;

public class JMDict_Open
{
	[Fact]
	public void should_open_file()
	{
		using (var file = JMDict.Open())
		{
			Assert.True(file.Size > 0, "file size should not be empty");
		}
	}

	[Fact]
	public void should_read_entry()
	{
		using (var file = JMDict.Open())
		{
			var entries = file.ReadEntries();
			var a = entries.First(x => x.Sequence == "1000000");
			Assert.Equal("1000000", a.Sequence);

			var b = entries.First(x => x.Sequence == "1000020");
			Assert.Equal("1000020", b.Sequence);

			var c = entries.First(x => x.Sequence == "1000060");
			Assert.Equal("1000060", c.Sequence);
		}
	}
}
