namespace Importer;

using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

/*

	Priority of entries
	===================

	The priority of kanji and reading elements is given by the `<ke_pri>` and
	`<re_pri>` elements in the XML data. Possible values are documented in the
	file header, but here is a summary:

	- news1/2: first and last 12,000 words frequency compiled by Alexandre Girardi
	from the `Mainichi Shinbun`.

	- ichi1/2: appears in the `Ichimango goi bunruishuu` from 1998. The ichi2
	entries were demoted due to low observed frequency.

	- spec1/2: small number of words detected as common but not appearing in
	other lists.

	- gai1/2: common loanwords, based on the frequency file.

	- nfXX: indicator of frequency-of-use ranking in the word frequency file,
	where `XX` is the number of the set of 500 words in which the entry can
	be found, starting at `01`.

	Entries with `news1`, `ichi1`, `spec1`, `spec2`, and `gai1` are marked as
	popular in EDICT files.

	Sorting of entries
	==================

	We use the priority tags in conjunction with frequency data to sort entries
	by popularity. Tags take priority, since the frequency meaning is a lot
	fuzzier and lacks the context necessary to properly associate entries with
	their frequency.

	Entries in the input file are stably sorted, considering tags in the
	following order:

	- Popular entries (any of `news1`, `ichi1`, `spec1`, `spec2`, and `gai1`).
	- Entries with `nfXX` tags, sorted by their group number. This is also used
	as a tie-breaker for popular entries.
	- Other priority tags in order: `spec2`, `news2`, `gai2`, `ichi2`.
	- Entries with any frequency information at all.

	Within groups, the frequency information is used as a tie-breaker where
	available.

	Frequency information is divided by reliability. Frequency for an entry
	is considered reliable if:

	- It matches the kanji element of an entry;
	- Or it matches the reading element of an entry with `<misc>&uk;</misc>`
	in their sense elements.

	Entries with `uk` denote words that are usually written as kana, even if
	they have a kanji writing.

	Entries with reliable frequency take precedence, regardless of their
	value. Unreliable frequency is used as a last tie-breaker.

	For sorting purposes, frequency and priority for any given entry are taken
	from the highest value between the reading and kanji elements.

	Finally, after all other factors are considered, the entries are then
	sorted by the kanji/reading elements. We prioritize "usually kana" elements,
	and then sort by either the reading or kanji.

*/



/// <summary>
/// Support for parsing the XML for the <c>JMDict</c> data.
/// </summary>
///
/// <remarks>
/// For additional information about the file format and fields check the
/// documentation in the <c>JMdict.gz</c> readme.
/// </remarks>
public class JMDict : IDisposable
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

		public IList<string> Priority { get; } = new List<string>();
	}

	public record Kanji
	{
		public string Text { get; internal set; } = "";

		public IList<string> Priority { get; } = new List<string>();
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

		public IList<string> Misc { get; } = new List<string>();
	}

	public record Glossary
	{
		public string Lang { get; internal set; } = "eng";

		public string Type { get; internal set; } = "";

		public string Text { get; internal set; } = "";
	}

	public static JMDict Open()
	{
		var xml = Data.OpenXmlGZip("JMdict.gz");
		return new JMDict(xml);
	}

	private readonly XmlTextReader xml;
	private readonly Dictionary<string, string> tags = new Dictionary<string, string>();

	public IReadOnlyDictionary<string, string> Tags
	{
		get => tags;
	}

	private JMDict(XmlTextReader xml)
	{
		this.xml = xml;
		this.xml.EntityHandling = EntityHandling.ExpandCharEntities;
		this.xml.DtdProcessing = DtdProcessing.Parse;
	}

	public void Dispose()
	{
		xml.Dispose();
	}

	public IEnumerable<Entry> ReadEntries()
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

	private Entry ParseEntry(XmlReader xml)
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

	private Kanji ParseKanji(XmlReader xml)
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
						case TagEntryKanjiPriority:
							kanji.Priority.Add(ReadPriority(xml));
							break;
					}
					break;
			}
		}

		Debug.Assert(!String.IsNullOrEmpty(kanji.Text), "Kanji text is empty");
		return kanji;
	}

	private Reading ParseReading(XmlReader xml)
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
						case TagEntryReadingPriority:
							reading.Priority.Add(ReadPriority(xml));
							break;
					}
					break;
			}
		}

		Debug.Assert(!String.IsNullOrEmpty(reading.Text), "Reading text is empty");
		return reading;
	}

	private static readonly Regex rePriority = new Regex(@"^(news[12]|ichi[12]|spec[12]|gai[12]|nf\d{2})$");

	private string ReadPriority(XmlReader xml)
	{
		var priority = xml.ReadElementContentAsString();
		Debug.Assert(rePriority.IsMatch(priority));
		return priority;
	}

	private Sense ParseSense(XmlReader xml)
	{
		var sense = new Sense { };
		while (xml.Read())
		{
			switch (xml.NodeType)
			{
				case XmlNodeType.Element:
					switch (xml.LocalName)
					{
						case TagEntrySenseGlossary:
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
								break;
							}
						case TagEntrySenseMisc:
							{
								var tag = ReadElementAsTag(xml.ReadSubtree());
								sense.Misc.Add(tag);
								break;
							}
					}
					break;
			}
		}
		return sense;
	}

	private Glossary ParseSenseGlossary(XmlReader xml)
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

	/// <summary>
	/// Read an element that contains a tag content.
	/// </summary>
	private string ReadElementAsTag(XmlReader xml)
	{
		/*
			In `JMdict.xml` tags are declared as `<!ENTITY tag ...>` and are
			referenced in elements (e.g. `<something>&tag;</something>`).

			In this method we are assuming that this is always the case, i.e.,
			a tag element has a single entity reference in it. We use this to
			keep the entity as the tag name, but extract the expanded text.
		*/
		var tagName = "";
		var tagText = "";
		while (xml.Read())
		{
			switch (xml.NodeType)
			{
				case XmlNodeType.EntityReference:
					// We don't want to expand entities as tags
					Debug.Assert(tagName == "");
					tagName = xml.LocalName;
					xml.ResolveEntity();
					break;
				case XmlNodeType.Text:
					Debug.Assert(tagText == "");
					tagText = xml.Value;
					break;
			}
		}
		Debug.Assert(tagName != "" && tagText != "");
		this.tags.TryAdd(tagName, tagText);
		return tagName;
	}

	#region Schema

	const string TagEntry = "entry";
	const string TagEntrySequence = "ent_seq";

	const string TagEntryKanji = "k_ele";
	const string TagEntryKanjiText = "keb";
	const string TagEntryKanjiPriority = "ke_pri";

	// spell-checker: ignore nokanji, restr

	const string TagEntryReading = "r_ele";
	const string TagEntryReadingText = "reb";
	const string TagEntryReadingPriority = "re_pri";

	const string TagEntrySense = "sense";
	const string TagEntrySenseGlossary = "gloss";
	const string TagEntrySenseMisc = "misc";

	#endregion
}
