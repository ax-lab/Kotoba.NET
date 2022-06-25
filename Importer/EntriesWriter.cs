namespace Importer;

public class EntriesWriter : DatabaseWriter
{
	public EntriesWriter(string fileName) : base(fileName)
	{
		this.ExecuteCommand(@"
			CREATE TABLE IF NOT EXISTS entries (
				sequence INTEGER PRIMARY KEY
			);
		");
	}

	public bool HasEntries
	{
		get => this.ExecuteScalar<int>("SELECT COUNT(*) FROM entries") > 0;
	}

	public void InsertEntries(IList<JMDict.Entry> entries)
	{
		using (var trans = this.db.BeginTransaction())
		{
			using (var cmd = this.db.CreateCommand())
			{
				cmd.CommandText = @"
					INSERT INTO entries(sequence) VALUES ($sequence)
				";

				var sequence = cmd.CreateParameter();
				sequence.ParameterName = "$sequence";
				cmd.Parameters.Add(sequence);

				foreach (var entry in entries)
				{
					sequence.Value = entry.Sequence;
					cmd.ExecuteNonQuery();
				}

				trans.Commit();
			}
		}
	}
}
