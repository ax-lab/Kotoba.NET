namespace Dictionary.Test;

public class Database_Open
{
	[Fact]
	public void throws_exception_for_database_not_found()
	{
		Assert.Throws<FileNotFoundException>(() => new Database("invalid.db"));
	}

	[Fact]
	public void opens_existing_database()
	{
		using (var db = new Database("entries.db"))
		{
			using (var cmd = db.CreateCommand("SELECT COUNT(*) FROM entries"))
			{
				var count = Convert.ToInt64(cmd.ExecuteScalar());
				Assert.True(count > 0);
			}
		}
	}
}
