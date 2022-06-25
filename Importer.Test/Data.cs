namespace Importer.Test;

public class Data_OpenFile
{
	[Fact]
	public void returns_file_from_data_directory()
	{
		using (var input = Data.OpenFile("README.md"))
		{
			Assert.NotNull(input);
			using (var sr = new StreamReader(input))
			{
				var text = sr.ReadToEnd();
				Assert.Contains("http://www.edrdg.org", text);
			}
		}
	}

	[Fact]
	public void throws_exception_if_data_directory_is_not_found()
	{
		var baseDir = Path.Join(Path.GetTempPath(), "..");
		Assert.Throws<DirectoryNotFoundException>(() => Data.OpenFile("README.md", baseDir));
	}
}
