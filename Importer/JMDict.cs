namespace Importer;

using System.Diagnostics;
using System.Xml;

public static class JMDict
{
	public readonly struct Entry
	{
		public string Sequence { get; init; }
	}

	public static XmlReader Open()
	{
		return Data.OpenXmlZip("JMdict.gz");
	}

	public static IEnumerable<Entry> ReadEntries(XmlReader xml)
	{
		while (xml.Read())
		{
			switch (xml.NodeType)
			{
				case XmlNodeType.Element:
					if (xml.LocalName == TagEntry)
					{
						yield return ReadEntry(xml);
					}
					break;
			}
		}
	}

	private static Entry ReadEntry(XmlReader xml)
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

	#region Schema

	const string TagEntry = "entry";
	const string TagEntrySequence = "ent_seq";

	#endregion
}
