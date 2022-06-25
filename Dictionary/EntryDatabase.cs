namespace Dictionary;

using Microsoft.Data.Sqlite;

public class EntryDatabase : Database
{
	internal EntryDatabase() : base("entries.db")
	{
	}

	public IEnumerable<Entry> QueryRows(SqliteCommand command)
	{
		using (var reader = command.ExecuteReader())
		{
			var colSequence = reader.GetOrdinal("sequence");
			while (reader.Read())
			{
				var id = reader.GetInt64(colSequence);
				yield return new Entry { Id = id };
			}
		}
	}
}
