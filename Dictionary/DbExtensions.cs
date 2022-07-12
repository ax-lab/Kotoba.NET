using Microsoft.Data.Sqlite;

internal static class DbExtensions
{
	public static SqliteParameter AddParam(this SqliteCommand command, string name)
	{
		var param = command.CreateParameter();
		param.ParameterName = name;
		command.Parameters.Add(param);
		return param;
	}

	public static Decimal? GetDecimalOrNull(this SqliteDataReader reader, int ordinal)
	{
		if (reader.IsDBNull(ordinal))
		{
			return null;
		}

		var value = reader.GetString(ordinal);
		return Decimal.Parse(value);
	}

	public static long? GetIntOrNull(this SqliteDataReader reader, int ordinal)
	{
		if (reader.IsDBNull(ordinal))
		{
			return null;
		}

		var value = reader.GetString(ordinal);
		return long.Parse(value);
	}
}
