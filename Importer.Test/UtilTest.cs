namespace Importer.Test;

public class Util_OpenFile
{
	[Fact]
	public void should_open_file_in_data_directory()
	{
		using (var input = Util.OpenFile("README.md"))
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
	public void should_throw_DirectoryNotFoundException_if_data_dir_is_not_found()
	{
		var baseDir = Path.Join(Path.GetTempPath(), "..");
		Assert.Throws<DirectoryNotFoundException>(() => Util.OpenFile("README.md", baseDir));
	}
}
