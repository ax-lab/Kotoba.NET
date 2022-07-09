using System.Globalization;
using System.Text;

namespace Importer;

public class EntriesWriter : DatabaseWriter
{
	public EntriesWriter(string fileName) : base(fileName)
	{
		/*
			A few notes about the database format:

			- The `priority` columns are a CSV list of priorities as documented
			in the JMDict code.

			- Each of the `tags` columns are a CSV list referencing the `tags`
			table.

			- The `glossary` entries are separated by `EntryDatabase.GLOSSARY_ENTRY_SEPARATOR`.
			- Each `glossary` entry has fields separated by `EntryDatabase.GLOSSARY_FIELD_SEPARATOR`.
		*/

		this.ExecuteCommand(@"
			CREATE TABLE IF NOT EXISTS tags (
				name TEXT,
				info TEXT,
				PRIMARY KEY(name ASC)
			) WITHOUT ROWID;

			CREATE TABLE IF NOT EXISTS entries (
				sequence INTEGER PRIMARY KEY
			);

			CREATE TABLE IF NOT EXISTS entries_kanji(
				sequence INTEGER,
				position INTEGER,
				text     TEXT,
				priority TEXT,
				PRIMARY KEY(sequence ASC, position ASC)
			) WITHOUT ROWID;

			CREATE TABLE IF NOT EXISTS entries_reading(
				sequence INTEGER,
				position INTEGER,
				text     TEXT,
				priority TEXT,
				PRIMARY KEY(sequence ASC, position ASC)
			) WITHOUT ROWID;

			CREATE TABLE IF NOT EXISTS entries_sense(
				sequence  INTEGER,
				position  INTEGER,
				glossary  TEXT,
				tags_misc TEXT,
				PRIMARY KEY(sequence ASC, position ASC)
			) WITHOUT ROWID;

			CREATE TABLE IF NOT EXISTS frequency(
				entry      TEXT,
				innocent   INTEGER,
				blog       INTEGER,
				news       INTEGER,
				twitter    INTEGER,
				blog_pm    TEXT,
				news_pm    TEXT,
				twitter_pm TEXT,
				PRIMARY KEY(entry)
			) WITHOUT ROWID;
		");
	}

	public bool HasEntries
	{
		get => this.ExecuteScalar<int>("SELECT COUNT(*) FROM entries") > 0;
	}

	public record FrequencyData
	{
		public Dictionary<string, long>? InnocentCorpus { get; init; }
		public Dictionary<string, Frequency.WorldLex>? WorldLex { get; init; }
	}

	public void InsertEntries(
		IList<JMDict.Entry> entries,
		IReadOnlyDictionary<string, string> tags,
		FrequencyData frequency)
	{
		using (var trans = this.db.BeginTransaction())
		{
			InsertFrequency(entries, frequency);
			InsertTags(tags);
			InsertEntries(entries);
			InsertKanji(entries);
			InsertReading(entries);
			InsertSense(entries);

			trans.Commit();
		}
	}

	private void InsertTags(IReadOnlyDictionary<string, string> tags)
	{
		using (var cmd = this.db.CreateCommand())
		{
			cmd.CommandText = @"INSERT INTO tags(name, info) VALUES ($name, $info)";

			var name = cmd.AddParam("$name");
			var info = cmd.AddParam("$info");

			foreach (var (key, val) in tags)
			{
				name.Value = key;
				info.Value = val;
				cmd.ExecuteNonQuery();
			}
		}
	}

	private void InsertFrequency(IList<JMDict.Entry> entries, FrequencyData frequency)
	{
		using (var cmd = this.db.CreateCommand())
		{
			cmd.CommandText = @"
				INSERT INTO frequency
				(entry, innocent, blog, news, twitter, blog_pm, news_pm, twitter_pm)
				VALUES
				($entry, $innocent, $blog, $news, $twitter, $blog_pm, $news_pm, $twitter_pm)
			";
			var entry = cmd.AddParam("$entry");
			var innocent = cmd.AddParam("$innocent");
			var blog = cmd.AddParam("$blog");
			var news = cmd.AddParam("$news");
			var twitter = cmd.AddParam("$twitter");
			var blogPm = cmd.AddParam("$blog_pm");
			var newsPm = cmd.AddParam("$news_pm");
			var twitterPm = cmd.AddParam("$twitter_pm");

			var hasEntry = new HashSet<string>();
			foreach (var itEntry in entries)
			{
				foreach (var it in itEntry.Kanji)
				{
					hasEntry.Add(it.Text);
				}

				foreach (var it in itEntry.Reading)
				{
					hasEntry.Add(it.Text);
				}
			}

			var allEntries = hasEntry.ToList();
			allEntries.Sort();
			foreach (var it in allEntries)
			{
				var hasData = false;

				entry.Value = it;

				// Get the frequency from innocent corpus:
				long innocentValue;
				if (frequency.InnocentCorpus != null && frequency.InnocentCorpus.TryGetValue(it, out innocentValue))
				{
					hasData = true;
					innocent.Value = innocentValue;
				}
				else
				{
					innocent.Value = DBNull.Value;
				}

				// Get the frequency from WorldLex:
				Frequency.WorldLex? worldLex;
				if (frequency.WorldLex != null && frequency.WorldLex.TryGetValue(it, out worldLex))
				{
					hasData = true;
					blog.Value = worldLex.Blog.Frequency;
					news.Value = worldLex.News.Frequency;
					twitter.Value = worldLex.Twitter.Frequency;
					blogPm.Value = worldLex.Blog.PerMillion.ToString(CultureInfo.InvariantCulture);
					newsPm.Value = worldLex.News.PerMillion.ToString(CultureInfo.InvariantCulture);
					twitterPm.Value = worldLex.Twitter.PerMillion.ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					blog.Value = DBNull.Value;
					news.Value = DBNull.Value;
					twitter.Value = DBNull.Value;
					blogPm.Value = DBNull.Value;
					newsPm.Value = DBNull.Value;
					twitterPm.Value = DBNull.Value;
				}

				if (hasData)
				{
					cmd.ExecuteNonQuery();
				}
			}
		}
	}

	private void InsertEntries(IList<JMDict.Entry> entries)
	{
		using (var cmd = this.db.CreateCommand())
		{
			cmd.CommandText = @"INSERT INTO entries(sequence) VALUES ($sequence)";

			var sequence = cmd.AddParam("$sequence");
			foreach (var entry in entries)
			{
				sequence.Value = entry.Sequence;
				cmd.ExecuteNonQuery();
			}
		}
	}

	private void InsertKanji(IList<JMDict.Entry> entries)
	{
		using (var cmd = this.db.CreateCommand())
		{
			cmd.CommandText = @"
				INSERT INTO entries_kanji
					(sequence, position, text, priority)
				VALUES
					($sequence, $position, $text, $priority)
			";

			var sequence = cmd.AddParam("$sequence");
			var position = cmd.AddParam("$position");
			var text = cmd.AddParam("$text");
			var priority = cmd.AddParam("$priority");

			foreach (var entry in entries)
			{
				sequence.Value = entry.Sequence;

				var currentPosition = 0;
				foreach (var kanji in entry.Kanji)
				{
					position.Value = ++currentPosition;
					text.Value = kanji.Text;
					priority.Value = String.Join(",", kanji.Priority);
					cmd.ExecuteNonQuery();
				}
			}
		}
	}

	private void InsertReading(IList<JMDict.Entry> entries)
	{
		using (var cmd = this.db.CreateCommand())
		{
			cmd.CommandText = @"
				INSERT INTO entries_reading
					(sequence, position, text, priority)
				VALUES
					($sequence, $position, $text, $priority)
			";

			var sequence = cmd.AddParam("$sequence");
			var position = cmd.AddParam("$position");
			var text = cmd.AddParam("$text");
			var priority = cmd.AddParam("$priority");

			foreach (var entry in entries)
			{
				sequence.Value = entry.Sequence;

				var currentPosition = 0;
				foreach (var reading in entry.Reading)
				{
					position.Value = ++currentPosition;
					text.Value = reading.Text;
					priority.Value = String.Join(",", reading.Priority);
					cmd.ExecuteNonQuery();
				}
			}
		}
	}

	private void InsertSense(IList<JMDict.Entry> entries)
	{
		using (var cmd = this.db.CreateCommand())
		{
			cmd.CommandText = @"
				INSERT INTO entries_sense
					(sequence, position, tags_misc, glossary)
				VALUES
					($sequence, $position, $tags_misc, $glossary)
			";

			var sequence = cmd.AddParam("$sequence");
			var position = cmd.AddParam("$position");
			var tagsMisc = cmd.AddParam("$tags_misc");
			var glossary = cmd.AddParam("$glossary");

			foreach (var entry in entries)
			{
				sequence.Value = entry.Sequence;

				var currentPosition = 0;
				var glossaryText = new StringBuilder();
				var filteredSenses = entry.Sense.Where(x => x.Lang == "eng" && !x.IsEmpty);
				foreach (var sense in filteredSenses)
				{
					position.Value = ++currentPosition;
					tagsMisc.Value = String.Join(",", sense.Misc);

					glossaryText.Clear();
					foreach (var glossaryEntry in sense.Glossary)
					{
						if (glossaryText.Length > 0)
						{
							glossaryText.Append(Dictionary.EntryDatabase.GLOSSARY_ENTRY_SEPARATOR);
						}
						glossaryText.AppendFormat("{0}{1}{2}",
							glossaryEntry.Type,
							Dictionary.EntryDatabase.GLOSSARY_FIELD_SEPARATOR,
							glossaryEntry.Text);
					}
					glossary.Value = glossaryText.ToString();
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
