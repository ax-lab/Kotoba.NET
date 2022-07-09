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

	public static IEnumerable<Entry> ByIds(params long[] ids)
	{
		var idList = String.Join(",", ids.Select(x => x.ToString()));
		var sql = String.Format("SELECT * FROM entries WHERE sequence IN ({0}) ORDER BY position", idList);
		return QueryEntries(sql);
	}

	public static IEnumerable<Entry> List(long limit, long offset = 0)
	{
		return QueryEntries(
			"SELECT * FROM entries LIMIT $limit OFFSET $offset",
			("$limit", limit), ("$offset", offset));
	}

	private static IEnumerable<Entry> QueryEntries(string sql, params (string, object)[] args)
	{
		using (var db = new EntryDatabase())
		{
			foreach (var it in db.QueryEntries(sql, args))
			{
				yield return it;
			}
		}
	}
}
