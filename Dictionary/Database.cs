namespace Dictionary;

using Microsoft.Data.Sqlite;

public class Database : IDisposable
{
	const string DataDirectory = "data";

	private readonly SqliteConnection db;

	internal protected Database(string name)
	{
		string databaseFilePath;
		lock (databaseFilePathCache)
		{
			if (!databaseFilePathCache.TryGetValue(name, out databaseFilePath!))
			{
				databaseFilePath = FindFilePathForDatabase(name) ??
					throw new FileNotFoundException(String.Format("database file for {0} not found", name));
				databaseFilePathCache.Add(name, databaseFilePath);
			}
		}

		var config = new SqliteConnectionStringBuilder();
		config.DataSource = databaseFilePath;
		config.Mode = SqliteOpenMode.ReadOnly;

		this.db = new SqliteConnection(config.ConnectionString);
		this.db.Open();
	}

	public void Dispose()
	{
		this.db.Dispose();
	}

	internal protected SqliteCommand CreateCommand(string sql)
	{
		var cmd = this.db.CreateCommand();
		cmd.CommandText = sql;
		return cmd;
	}

	#region Database file name handling

	private static readonly Dictionary<string, string> databaseFilePathCache = new Dictionary<string, string>();

	private static string? FindFilePathForDatabase(string databaseName)
	{
		var currentDir = Directory.GetCurrentDirectory();
		while (true)
		{
			var databaseFile = Path.Join(currentDir, DataDirectory, databaseName);
			if (File.Exists(databaseFile))
			{
				return databaseFile;
			}

			var parentDir = Path.GetFullPath(Path.Join(currentDir, ".."));
			if (parentDir == currentDir)
			{
				return null;
			}

			currentDir = parentDir;
		}
	}

	#endregion
}
