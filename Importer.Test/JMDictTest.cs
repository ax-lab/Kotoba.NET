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
}
