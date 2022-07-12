namespace Dictionary;

public static class Frequency
{
	public record Entry : IComparable<Entry>
	{
		public long? InnocentCorpus { get; init; }
		public WorldLex? WorldLex { get; init; }

		public bool IsEmpty
		{
			get
			{
				var count =
					(InnocentCorpus ?? 0) +
					(WorldLex?.Blog ?? 0) +
					(WorldLex?.News ?? 0) +
					(WorldLex?.Twitter ?? 0);
				return count == 0;
			}
		}

		public int CompareTo(Entry? other)
		{
			var icA = this.InnocentCorpus;
			var icB = other?.InnocentCorpus;
			var icCmp = (icA ?? 0).CompareTo(icB ?? 0);
			if (icCmp != 0)
			{
				// highest values come first
				return -icCmp;
			}

			var lexA =
				(this.WorldLex?.Blog ?? 0) +
				(this.WorldLex?.News ?? 0) +
				(this.WorldLex?.Twitter ?? 0);
			var lexB =
				(other?.WorldLex?.Blog ?? 0) +
				(other?.WorldLex?.News ?? 0) +
				(other?.WorldLex?.Twitter ?? 0);

			var lexCmp = lexA.CompareTo(lexB);
			if (lexCmp != 0)
			{
				return -lexCmp;
			}

			return 0;
		}
	}

	public record WorldLex
	{
		public long Blog { get; init; }
		public long News { get; init; }
		public long Twitter { get; init; }
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
