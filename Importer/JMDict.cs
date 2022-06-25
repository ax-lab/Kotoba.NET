namespace Importer;

using System.Diagnostics;
using System.IO.Compression;
using System.Xml;

public struct JMDict : IDisposable
{
	const string SourceFile = "JMdict.gz";

	/// <summary>
	/// Size of the source file.
	/// </summary>
	public long Size { get; init; }

	private readonly XmlReader _xml;

	private JMDict(FileStream input)
	{
		this.Size = input.Length;

		var unzip = new GZipStream(input, CompressionMode.Decompress);

		var xmlSettings = new XmlReaderSettings();
		xmlSettings.DtdProcessing = DtdProcessing.Parse;

		this._xml = XmlReader.Create(unzip, xmlSettings);
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
		this._xml.Dispose();
	}

	public IEnumerable<Entry> ReadEntries()
	{
		var xml = this._xml;

		while (xml.Read())
		{
			switch (xml.NodeType)
			{
				case XmlNodeType.Element:
					if (xml.LocalName == TagEntry)
					{
						yield return this.ReadEntry(xml);
					}
					break;
			}
		}
	}

	public readonly struct Entry
	{
		public string Sequence { get; init; }
	}

	private Entry ReadEntry(XmlReader xml)
	{
		bool parsing = true;

		string? sequence = null;
		while (parsing && xml.Read())
		{
			switch (xml.NodeType)
			{
				case XmlNodeType.Element:
					switch (xml.LocalName)
					{
						case TagEntrySequence:
							sequence = xml.ReadElementContentAsString();
							break;
					}
					break;

				case XmlNodeType.EndElement:
					if (xml.LocalName == TagEntry)
					{
						parsing = false;
						break;
					}
					break;
			}
		}

		Debug.Assert(sequence != null, "Entry.sequence is empty");
		return new Entry { Sequence = sequence };
	}

	const string TagEntry = "entry";
	const string TagEntrySequence = "ent_seq";
}
