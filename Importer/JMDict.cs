namespace Importer;

public struct JMDict : IDisposable
{
	const string SourceFile = "JMdict.gz";

	/// <summary>
	/// Size of the source file.
	/// </summary>
	public long Size { get; init; }

	private JMDict(FileStream input)
	{
		this.Size = input.Length;
		input.Dispose();
	}

	/// <summary>
	/// Opens the default file.
	/// </summary>
	public static JMDict Open()
	{
		var inputFile = Util.OpenFile(SourceFile);
		return new JMDict(inputFile);
	}

	public void Dispose()
	{
	}
}
