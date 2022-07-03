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
	private const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";

	public record Entry
	{
		public string Sequence { get; internal set; } = "";

		public IList<Kanji> Kanji { get; } = new List<Kanji>();

		public IList<Reading> Reading { get; } = new List<Reading>();

		public IList<Sense> Sense { get; } = new List<Sense>();
	}

	public record Reading
	{
		public string Text { get; internal set; } = "";
	}

	public record Kanji
	{
		public string Text { get; internal set; } = "";
	}

	public record Sense
	{
		public string Lang { get; internal set; } = "eng";

		/// <summary>
		/// Returns if the sense is completely empty.
		/// </summary>
		/// <remarks>
		/// This is used to filter out empty senses that are present in the
		/// source data.
		/// </remarks>
		public bool IsEmpty
		{
			get
			{
				var count = 0;
				count += Glossary.Count;
				return count == 0;
			}
		}

		public IList<Glossary> Glossary { get; } = new List<Glossary>();
	}

	public record Glossary
	{
		public string Lang { get; internal set; } = "eng";

		public string Type { get; internal set; } = "";

		public string Text { get; internal set; } = "";
	}

	public static XmlReader Open()
	{
		return Data.OpenXmlGZip("JMdict.gz");
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
							entry.Kanji.Add(ParseKanji(xml.ReadSubtree()));
							break;

						case TagEntryReading:
							entry.Reading.Add(ParseReading(xml.ReadSubtree()));
							break;

						case TagEntrySense:
							entry.Sense.Add(ParseSense(xml.ReadSubtree()));
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
		var kanji = new Kanji { };
		while (xml.Read())
		{
			switch (xml.NodeType)
			{
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
		var reading = new Reading { };
		while (xml.Read())
		{
			switch (xml.NodeType)
			{
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

	private static Sense ParseSense(XmlReader xml)
	{
		var sense = new Sense { };
		while (xml.Read())
		{
			switch (xml.NodeType)
			{
				case XmlNodeType.Element:
					if (xml.LocalName == TagEntrySenseGlossary)
					{
						var glossary = ParseSenseGlossary(xml);
						if (sense.Glossary.Count == 0)
						{
							sense.Lang = glossary.Lang;
						}
						else
						{
							Debug.Assert(sense.Lang == glossary.Lang);
						}
						sense.Glossary.Add(glossary);
					}
					break;
			}
		}
		return sense;
	}

	private static Glossary ParseSenseGlossary(XmlReader xml)
	{
		var glossary = new Glossary { };
		var lang = xml.GetAttribute("lang", XmlNamespace);
		if (!String.IsNullOrEmpty(lang))
		{
			glossary.Lang = lang;
		}
		glossary.Type = xml.GetAttribute("g_type") ?? "";
		glossary.Text = xml.ReadElementContentAsString();
		return glossary;
	}

	#region Schema

	const string TagEntry = "entry";
	const string TagEntrySequence = "ent_seq";

	const string TagEntryKanji = "k_ele";
	const string TagEntryKanjiText = "keb";

	// spell-checker: ignore nokanji, restr

	const string TagEntryReading = "r_ele";
	const string TagEntryReadingText = "reb";

	const string TagEntrySense = "sense";
	const string TagEntrySenseGlossary = "gloss";

	#endregion
}
