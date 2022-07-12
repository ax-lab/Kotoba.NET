namespace Importer;

using Microsoft.Data.Sqlite;

public class DatabaseWriter : IDisposable
{
	public string Name { get; init; }

	protected DatabaseWriter(string fileName)
	{
		this.Name = Path.GetFileName(fileName);
		this.db = new SqliteConnection(String.Format("Data Source={0}", fileName));
		this.db.Open();
	}

	public void Dispose()
	{
		this.db.Dispose();
	}

	protected record struct Argument(string name, object value);

	protected readonly SqliteConnection db;

	protected void ExecuteCommand(string sql, params Argument[] args)
	{
		using (var cmd = this.CreateCommand(sql, args))
		{
			cmd.ExecuteNonQuery();
		}
	}

	protected T ExecuteScalar<T>(string sql, params Argument[] args)
	{
		using (var cmd = this.CreateCommand(sql, args))
		{
			var result = cmd.ExecuteScalar();
			var output = Convert.ChangeType(result, typeof(T));
			return (T)(output ?? default(T))!;
		}
	}

	protected SqliteCommand CreateCommand(string sql, params Argument[] args)
	{
		var cmd = this.db.CreateCommand();
		cmd.CommandText = sql;
		foreach (var it in args)
		{
			cmd.Parameters.AddWithValue(it.name, it.value);
		}
		return cmd;
	}
}
