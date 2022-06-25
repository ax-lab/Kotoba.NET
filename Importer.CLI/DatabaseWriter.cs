using Microsoft.Data.Sqlite;

public class DatabaseWriter : IDisposable
{
	public string Name { get; init; }

	public DatabaseWriter(string fileName)
	{
		this.Name = fileName;
		this.db = new SqliteConnection(String.Format("Data Source={0}", fileName));
		this.db.Open();
	}

	public void Dispose()
	{
		this.db.Dispose();
	}

	private readonly SqliteConnection db;
}
