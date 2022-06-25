namespace Importer;

using System;
using System.Diagnostics;
using System.Xml;

/// <summary>
/// Support for parsing the XML for the <c>JMDict</c> data.
/// </summary>
///
/// <remarks>
/// For additional information about the file format and fields check the
/// documentation in the <c>JMdict.gz</c> readme.
/// </remarks>
public static class JMDict
{
	public record Entry
	{
		public string Sequence { get; internal set; } = "";

		public IList<Kanji> Kanji { get; } = new List<Kanji>();

		public IList<Reading> Reading { get; } = new List<Reading>();
	}

	public record Reading
	{
		public string Text { get; internal set; } = "";
	}

	public record Kanji
	{
		public string Text { get; internal set; } = "";
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
						yield return ParseEntry(xml);
					}
					break;
			}
		}
	}

	private static Entry ParseEntry(XmlReader xml)
	{
		bool parsing = true;

		var entry = new Entry { };

		while (parsing && xml.Read())
		{
			switch (xml.NodeType)
			{
				case XmlNodeType.EndElement:
					if (xml.LocalName == TagEntry)
					{
						parsing = false;
					}
					break;

				case XmlNodeType.Element:
					switch (xml.LocalName)
					{
						case TagEntrySequence:
							entry.Sequence = xml.ReadElementContentAsString();
							break;

						case TagEntryKanji:
							entry.Kanji.Add(ParseKanji(xml));
							break;

						case TagEntryReading:
							entry.Reading.Add(ParseReading(xml));
							break;
					}
					break;
			}
		}

		Debug.Assert(!String.IsNullOrEmpty(entry.Sequence), "Entry.sequence is empty");
		Debug.Assert(entry.Kanji.Count > 0 || entry.Reading.Count > 0, "Entry has no kanji or readings");

		return entry;
	}

	private static Kanji ParseKanji(XmlReader xml)
	{
		bool parsing = true;

		var kanji = new Kanji { };

		while (parsing && xml.Read())
		{
			switch (xml.NodeType)
			{
				case XmlNodeType.EndElement:
					if (xml.LocalName == TagEntryKanji)
					{
						parsing = false;
					}
					break;

				case XmlNodeType.Element:
					switch (xml.LocalName)
					{
						case TagEntryKanjiText:
							kanji.Text = xml.ReadElementContentAsString();
							break;
					}
					break;
			}
		}

		Debug.Assert(!String.IsNullOrEmpty(kanji.Text), "Kanji text is empty");
		return kanji;
	}

	private static Reading ParseReading(XmlReader xml)
	{
		bool parsing = true;

		var reading = new Reading { };

		while (parsing && xml.Read())
		{
			switch (xml.NodeType)
			{
				case XmlNodeType.EndElement:
					if (xml.LocalName == TagEntryReading)
					{
						parsing = false;
					}
					break;

				case XmlNodeType.Element:
					switch (xml.LocalName)
					{
						case TagEntryReadingText:
							reading.Text = xml.ReadElementContentAsString();
							break;
					}
					break;
			}
		}

		Debug.Assert(!String.IsNullOrEmpty(reading.Text), "Reading text is empty");
		return reading;
	}

	#region Schema

	const string TagEntry = "entry";
	const string TagEntrySequence = "ent_seq";

	const string TagEntryKanji = "k_ele";
	const string TagEntryKanjiText = "keb";
	const string TagEntryKanjiPriority = "ke_pri";
	const string TagEntryKanjiInfo = "ke_inf";

	// spell-checker: ignore nokanji, restr

	const string TagEntryReading = "r_ele";
	const string TagEntryReadingText = "reb";
	const string TagEntryReadingInfo = "re_inf";
	const string TagEntryReadingPriority = "re_pri";
	const string TagEntryReadingNoKanji = "re_nokanji";
	const string TagEntryReadingRestriction = "re_restr";

	#endregion
}
