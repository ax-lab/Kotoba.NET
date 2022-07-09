namespace Dictionary;

using Microsoft.Data.Sqlite;

public class EntryDatabase : Database
{
	/// <summary>
	/// Separator for glossary entries in the sense table.
	/// </summary>
	internal const string GLOSSARY_ENTRY_SEPARATOR = ";;";

	/// <summary>
	/// Separator for glossary fields within a glossary.
	/// </summary>
	internal const string GLOSSARY_FIELD_SEPARATOR = "::";

	internal EntryDatabase() : base("entries.db")
	{
	}

	public IEnumerable<Frequency.Entry> QueryFrequency(SqliteCommand command)
	{
		using (var reader = command.ExecuteReader())
		{
			var colEntry = reader.GetOrdinal("entry");
			var colInnocent = reader.GetOrdinal("innocent");
			var colBlog = reader.GetOrdinal("blog");
			var colNews = reader.GetOrdinal("news");
			var colTwitter = reader.GetOrdinal("twitter");
			while (reader.Read())
			{
				var entry = reader.GetString(colEntry);
				var innocent = reader.GetIntOrNull(colInnocent);
				var blog = reader.GetIntOrNull(colBlog);
				var news = reader.GetIntOrNull(colNews);
				var twitter = reader.GetIntOrNull(colTwitter);

				var worldLex = blog != null ? new Frequency.WorldLex
				{
					Blog = blog ?? 0,
					News = news ?? 0,
					Twitter = twitter ?? 0,
				} : null;

				var row = new Frequency.Entry
				{
					InnocentCorpus = innocent,
					WorldLex = worldLex,
				};
				yield return row;
			}
		}
	}

	public IEnumerable<Entry> QueryEntries(string sql, params (string, object)[] args)
	{
		using (var cmd = this.CreateCommand(sql))
		{
			foreach (var (key, val) in args)
			{
				cmd.Parameters.AddWithValue(key, val);
			}
			return this.QueryEntries(cmd).ToList();
		}
	}

	public IEnumerable<Entry> QueryEntries(SqliteCommand command)
	{
		using (var reader = command.ExecuteReader())
		{
			// sequence comes from the source XML and is the entry's id
			var colSequence = reader.GetOrdinal("sequence");
			// position is the entry's position after being sorted during import
			// and corresponds to the order entries are saved in the database
			var colPosition = reader.GetOrdinal("position");
			while (reader.Read())
			{
				var sequence = reader.GetInt64(colSequence);
				var position = reader.GetInt64(colPosition);
				var kanji = this.GetEntryKanji(sequence);
				var reading = this.GetEntryReading(sequence);
				var sense = this.GetEntrySense(sequence);
				yield return new Entry
				{
					Id = sequence,
					Position = position,
					Kanji = kanji,
					Reading = reading,
					Sense = sense,
				};
			}
		}
	}

	private List<EntryKanji> GetEntryKanji(long sequence)
	{
		var output = new List<EntryKanji>();
		using (var command = this.CreateCommand("SELECT * FROM entries_kanji WHERE sequence = $sequence ORDER BY position"))
		{
			command.Parameters.AddWithValue("$sequence", sequence);
			using (var reader = command.ExecuteReader())
			{
				var colText = reader.GetOrdinal("text");
				var colPriority = reader.GetOrdinal("priority");
				while (reader.Read())
				{
					var text = reader.GetString(colText);
					var priority = reader.GetString(colPriority);
					var kanji = new EntryKanji
					{
						Text = text,
						Priority = priority.Split(",", StringSplitOptions.RemoveEmptyEntries),
					};
					output.Add(kanji);
				}
			}
		}
		return output;
	}

	private List<EntryReading> GetEntryReading(long sequence)
	{
		var output = new List<EntryReading>();
		using (var command = this.CreateCommand("SELECT * FROM entries_reading WHERE sequence = $sequence ORDER BY position"))
		{
			command.Parameters.AddWithValue("$sequence", sequence);
			using (var reader = command.ExecuteReader())
			{
				var colText = reader.GetOrdinal("text");
				var colPriority = reader.GetOrdinal("priority");
				while (reader.Read())
				{
					var text = reader.GetString(colText);
					var priority = reader.GetString(colPriority);
					var reading = new EntryReading
					{
						Text = text,
						Priority = priority.Split(",", StringSplitOptions.RemoveEmptyEntries),
					};
					output.Add(reading);
				}
			}
		}
		return output;
	}

	private List<EntrySense> GetEntrySense(long sequence)
	{
		var output = new List<EntrySense>();
		using (var command = this.CreateCommand("SELECT * FROM entries_sense WHERE sequence = $sequence ORDER BY position"))
		{
			command.Parameters.AddWithValue("$sequence", sequence);
			using (var reader = command.ExecuteReader())
			{
				var colText = reader.GetOrdinal("glossary");
				var colTagsMisc = reader.GetOrdinal("tags_misc");
				while (reader.Read())
				{
					var glossary = reader.GetString(colText)
						.Split(GLOSSARY_ENTRY_SEPARATOR, StringSplitOptions.RemoveEmptyEntries)
						.Select(x =>
						{
							var fields = x.Split(GLOSSARY_FIELD_SEPARATOR, 2);
							var glossary = new EntrySenseGlossary
							{
								Type = fields[0],
								Text = fields[1],
							};
							return glossary;
						})
						.ToList();

					var misc = LoadTags(reader.GetString(colTagsMisc));
					var sense = new EntrySense
					{
						Glossary = glossary,
						Misc = misc,
					};
					output.Add(sense);
				}
			}
		}
		return output;
	}

	private static List<Tag> LoadTags(string tagList)
	{
		return tagList.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => Entries.GetTag(x)).ToList();
	}
}
