namespace Importer;

public class EntriesWriter : DatabaseWriter
{
	public EntriesWriter(string fileName) : base(fileName)
	{
		this.ExecuteCommand(@"
			CREATE TABLE IF NOT EXISTS entries (
				sequence INTEGER PRIMARY KEY
			);

			CREATE TABLE IF NOT EXISTS entries_kanji(
				sequence INTEGER,
				position INTEGER,
				text     TEXT,
				PRIMARY KEY(sequence ASC, position ASC)
			) WITHOUT ROWID;

			CREATE TABLE IF NOT EXISTS entries_reading(
				sequence INTEGER,
				position INTEGER,
				text     TEXT,
				PRIMARY KEY(sequence ASC, position ASC)
			) WITHOUT ROWID;
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
			using (var cmdInsertEntry = this.db.CreateCommand())
			{
				cmdInsertEntry.CommandText = @"
					INSERT INTO entries(sequence) VALUES ($sequence)
				";

				var sequence = cmdInsertEntry.CreateParameter();
				sequence.ParameterName = "$sequence";
				cmdInsertEntry.Parameters.Add(sequence);

				foreach (var entry in entries)
				{
					sequence.Value = entry.Sequence;
					cmdInsertEntry.ExecuteNonQuery();
				}
			}

			using (var cmdInsertKanji = this.db.CreateCommand())
			{
				cmdInsertKanji.CommandText = @"
					INSERT INTO entries_kanji(sequence, position, text)
					VALUES ($sequence, $position, $text)
				";

				var paramSequence = cmdInsertKanji.CreateParameter();
				paramSequence.ParameterName = "$sequence";
				cmdInsertKanji.Parameters.Add(paramSequence);

				var paramPosition = cmdInsertKanji.CreateParameter();
				paramPosition.ParameterName = "$position";
				cmdInsertKanji.Parameters.Add(paramPosition);

				var paramText = cmdInsertKanji.CreateParameter();
				paramText.ParameterName = "$text";
				cmdInsertKanji.Parameters.Add(paramText);

				foreach (var entry in entries)
				{
					paramSequence.Value = entry.Sequence;

					var position = 0;
					foreach (var kanji in entry.Kanji)
					{
						paramPosition.Value = ++position;
						paramText.Value = kanji.Text;
						cmdInsertKanji.ExecuteNonQuery();
					}
				}
			}

			using (var cmdInsertReading = this.db.CreateCommand())
			{
				cmdInsertReading.CommandText = @"
					INSERT INTO entries_reading(sequence, position, text)
					VALUES ($sequence, $position, $text)
				";

				var paramSequence = cmdInsertReading.CreateParameter();
				paramSequence.ParameterName = "$sequence";
				cmdInsertReading.Parameters.Add(paramSequence);

				var paramPosition = cmdInsertReading.CreateParameter();
				paramPosition.ParameterName = "$position";
				cmdInsertReading.Parameters.Add(paramPosition);

				var paramText = cmdInsertReading.CreateParameter();
				paramText.ParameterName = "$text";
				cmdInsertReading.Parameters.Add(paramText);

				foreach (var entry in entries)
				{
					paramSequence.Value = entry.Sequence;

					var position = 0;
					foreach (var reading in entry.Reading)
					{
						paramPosition.Value = ++position;
						paramText.Value = reading.Text;
						cmdInsertReading.ExecuteNonQuery();
					}
				}
			}

			trans.Commit();
		}
	}
}
