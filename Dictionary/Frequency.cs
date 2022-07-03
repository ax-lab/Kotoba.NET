namespace Dictionary;

public static class Frequency
{
	public record Entry
	{
		public long? InnocentCorpus { get; init; }
		public WorldLex? WorldLex { get; init; }
	}

	public record WorldLex
	{
		public long Blog { get; init; }
		public long News { get; init; }
		public long Twitter { get; init; }

		public decimal BlogPerMillion { get; init; }
		public decimal NewsPerMillion { get; init; }
		public decimal TwitterPerMillion { get; init; }
	}

	public static Frequency.Entry? Get(string entry)
	{
		using (var db = new EntryDatabase())
		{
			using (var cmd = db.CreateCommand("SELECT * FROM frequency WHERE entry = $entry"))
			{
				cmd.Parameters.AddWithValue("$entry", entry);
				return db.QueryFrequency(cmd).FirstOrDefault();
			}
		}
	}
}
