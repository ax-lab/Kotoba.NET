namespace Dictionary;

public static class Entries
{
	const string DatabaseName = "entries.db";

	public static long Count
	{
		get
		{
			using (var db = new EntryDatabase())
			{
				using (var cmd = db.CreateCommand("SELECT COUNT(*) FROM entries"))
				{
					return (long)cmd.ExecuteScalar()!;
				}
			}
		}
	}


	private static List<Tag>? tags;

	public static IReadOnlyList<Tag> Tags
	{
		get
		{
			if (tags == null)
			{
				tags = new List<Tag>();
				using (var db = new EntryDatabase())
				{
					using (var cmd = db.CreateCommand("SELECT name, info FROM tags ORDER BY name"))
					{
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								var name = reader.GetString(0);
								var info = reader.GetString(1);
								var tag = new Tag(name, info);
								tags.Add(tag);
							}
						}
					}
				}
			}
			return tags;
		}
	}

	private static Dictionary<string, Tag>? tagByName;

	public static Tag GetTag(string name)
	{
		if (tagByName == null)
		{
			tagByName = new Dictionary<string, Tag>();
			foreach (var tag in Tags)
			{
				tagByName.Add(tag.Name, tag);
			}
		}

		return tagByName.GetValueOrDefault(name) ?? new Tag(name, "");
	}

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
