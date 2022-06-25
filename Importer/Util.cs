namespace Importer;

internal static class Util
{
	/// <summary>
	/// This is the directory in the project that contains the data files to
	/// import.
	/// </summary>
	public const string DataDirectory = "source-data";

	/// <summary>
	/// Opens a file from the import data directory <see cref="DataDirectory"/>.
	/// </summary>
	///
	/// <remarks>
	/// This method will look for the import data directory starting at the
	/// given <paramref name="baseDir"/> (which defaults to the current dir)
	/// and searching in all parent directories.
	/// </remarks>
	///
	/// <param name="fileName">
	/// Name of the file relative to the data directory.
	/// </param>
	///
	/// <param name="baseDir">
	/// Base directory to start the lookup for the import data. Defaults to
	/// the current directory if null or empty.	///
	/// </param>
	///
	/// <exception cref="DirectoryNotFoundException">
	/// If the data directory is not found.
	/// </exception>
	public static FileStream OpenFile(string fileName, string baseDir = "")
	{
		if (String.IsNullOrEmpty(baseDir))
		{
			baseDir = Directory.GetCurrentDirectory();
		}

		var currentDir = baseDir;
		while (true)
		{
			var dataDirectory = Path.Join(currentDir, DataDirectory);
			if (Directory.Exists(dataDirectory))
			{
				var filePath = Path.Join(dataDirectory, fileName);
				return File.Open(filePath, FileMode.Open);
			}

			var parentDir = Path.GetFullPath(Path.Join(currentDir, ".."));
			if (parentDir == currentDir)
			{
				throw new DirectoryNotFoundException(String.Format("could not find the data directory (`{0}` is missing -- {1})", DataDirectory, Directory.GetCurrentDirectory()));
			}
			currentDir = parentDir;
		}
	}
}
