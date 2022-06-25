namespace Dictionary;

public static class Entries
{
	const string DatabaseName = "entries.db";

	public static Entry? ById(long id)
	{
		using (var db = new EntryDatabase())
		{
			using (var cmd = db.CreateCommand("SELECT * FROM entries WHERE sequence = $sequence"))
			{
				cmd.Parameters.AddWithValue("$sequence", id);
				return db.QueryEntries(cmd).FirstOrDefault();
			}
		}
	}
}
