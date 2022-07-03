namespace Importer;

using System.IO.Compression;
using System.Xml;

internal static class Data
{
	/// <summary>
	/// This is the directory in the project that contains the data files to
	/// import.
	/// </summary>
	public const string SourceDirectory = "source-data";

	/// <summary>
	/// Read a compressed text file inside a Zip line by line.
	/// </summary>
	/// <param name="zipFileName">Filename to <see cref="OpenFile"/>.</param>
	/// <param name="innerFile">Text file name, inside the zip file.</param>
	/// <param name="parser">Line by line parser. Receives the line text and number.</param>
	public static void ReadZippedLines(string zipFileName, string innerFile, Func<string, long, bool> parser)
	{
		using (var input = OpenFile(zipFileName))
		{
			using (var zip = new ZipArchive(input, ZipArchiveMode.Read))
			{
				var entry = zip.GetEntry(innerFile) ?? throw new FileNotFoundException(
					String.Format("file {0} not found in {1}", innerFile, zipFileName));
				using (var reader = new StreamReader(entry.Open()))
				{
					var lineCount = 0;
					while (true)
					{
						var next = reader.ReadLine();
						if (next != null)
						{
							if (!parser(next, lineCount))
							{
								break;
							}
						}
						else
						{
							break;
						}
						lineCount++;
					}
				}
			}
		}
	}

	/// <summary>
	/// Helper to <see cref="OpenFile"/> a gzip XML file.
	/// </summary>
	public static XmlReader OpenXmlGZip(string fileName)
	{
		var input = OpenFile(fileName);
		var unzip = new GZipStream(input, CompressionMode.Decompress);

		var settings = new XmlReaderSettings();
		settings.DtdProcessing = DtdProcessing.Parse;

		return XmlReader.Create(unzip, settings);
	}

	/// <summary>
	/// Opens a file from the data directory <see cref="SourceDirectory"/>.
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
			var dataDirectory = Path.Join(currentDir, SourceDirectory);
			if (Directory.Exists(dataDirectory))
			{
				var filePath = Path.Join(dataDirectory, fileName);
				return File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			}

			var parentDir = Path.GetFullPath(Path.Join(currentDir, ".."));
			if (parentDir == currentDir)
			{
				throw new DirectoryNotFoundException(String.Format("could not find the data directory (`{0}` is missing -- {1})", SourceDirectory, Directory.GetCurrentDirectory()));
			}
			currentDir = parentDir;
		}
	}
}
