namespace Dictionary;

using Microsoft.Data.Sqlite;

public class EntryDatabase : Database
{
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
				yield return new Entry
				{
					Id = sequence,
					Kanji = kanji,
					Reading = reading,
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
				while (reader.Read())
				{
					var text = reader.GetString(colText);
					var kanji = new EntryKanji { Text = text };
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
				while (reader.Read())
				{
					var text = reader.GetString(colText);
					var reading = new EntryReading { Text = text };
					output.Add(reading);
				}
			}
		}
		return output;
	}
}
