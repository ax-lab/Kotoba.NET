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

	public IEnumerable<Entry> QueryEntries(SqliteCommand command)
	{
		using (var reader = command.ExecuteReader())
		{
			var colSequence = reader.GetOrdinal("sequence");
			while (reader.Read())
			{
				var sequence = reader.GetInt64(colSequence);
				var kanji = this.GetEntryKanji(sequence);
				var reading = this.GetEntryReading(sequence);
				var sense = this.GetEntrySense(sequence);
				yield return new Entry
				{
					Id = sequence,
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
