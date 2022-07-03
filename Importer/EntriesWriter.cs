using System.Text;

namespace Importer;

public class EntriesWriter : DatabaseWriter
{
	public EntriesWriter(string fileName) : base(fileName)
	{
		/*
			A few notes about the database format:

			- The `priority` columns are a CSV list of priorities as documented
			in the JMDict code.

			- Each of the `tags` columns are a CSV list referencing the `tags`
			table.

			- The `glossary` entries are separated by `EntryDatabase.GLOSSARY_ENTRY_SEPARATOR`.
			- Each `glossary` entry has fields separated by `EntryDatabase.GLOSSARY_FIELD_SEPARATOR`.
		*/

		this.ExecuteCommand(@"
			CREATE TABLE IF NOT EXISTS tags (
				name TEXT,
				info TEXT,
				PRIMARY KEY(name ASC)
			) WITHOUT ROWID;

			CREATE TABLE IF NOT EXISTS entries (
				sequence INTEGER PRIMARY KEY
			);

			CREATE TABLE IF NOT EXISTS entries_kanji(
				sequence INTEGER,
				position INTEGER,
				text     TEXT,
				priority TEXT,
				PRIMARY KEY(sequence ASC, position ASC)
			) WITHOUT ROWID;

			CREATE TABLE IF NOT EXISTS entries_reading(
				sequence INTEGER,
				position INTEGER,
				text     TEXT,
				priority TEXT,
				PRIMARY KEY(sequence ASC, position ASC)
			) WITHOUT ROWID;

			CREATE TABLE IF NOT EXISTS entries_sense(
				sequence  INTEGER,
				position  INTEGER,
				glossary  TEXT,
				tags_misc TEXT,
				PRIMARY KEY(sequence ASC, position ASC)
			) WITHOUT ROWID;
		");
	}

	public bool HasEntries
	{
		get => this.ExecuteScalar<int>("SELECT COUNT(*) FROM entries") > 0;
	}

	public void InsertEntries(IList<JMDict.Entry> entries, IReadOnlyDictionary<string, string> tags)
	{
		using (var trans = this.db.BeginTransaction())
		{
			using (var cmdInsertTag = this.db.CreateCommand())
			{
				cmdInsertTag.CommandText = @"
					INSERT INTO tags(name, info) VALUES ($name, $info)
				";

				var name = cmdInsertTag.CreateParameter();
				name.ParameterName = "$name";
				cmdInsertTag.Parameters.Add(name);

				var info = cmdInsertTag.CreateParameter();
				info.ParameterName = "$info";
				cmdInsertTag.Parameters.Add(info);

				foreach (var (key, val) in tags)
				{
					name.Value = key;
					info.Value = val;
					cmdInsertTag.ExecuteNonQuery();
				}
			}

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
					INSERT INTO entries_kanji(sequence, position, text, priority)
					VALUES ($sequence, $position, $text, $priority)
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

				var paramPriority = cmdInsertKanji.CreateParameter();
				paramPriority.ParameterName = "$priority";
				cmdInsertKanji.Parameters.Add(paramPriority);

				foreach (var entry in entries)
				{
					paramSequence.Value = entry.Sequence;

					var position = 0;
					foreach (var kanji in entry.Kanji)
					{
						paramPosition.Value = ++position;
						paramText.Value = kanji.Text;
						paramPriority.Value = String.Join(",", kanji.Priority);
						cmdInsertKanji.ExecuteNonQuery();
					}
				}
			}

			using (var cmdInsertReading = this.db.CreateCommand())
			{
				cmdInsertReading.CommandText = @"
					INSERT INTO entries_reading(sequence, position, text, priority)
					VALUES ($sequence, $position, $text, $priority)
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

				var paramPriority = cmdInsertReading.CreateParameter();
				paramPriority.ParameterName = "$priority";
				cmdInsertReading.Parameters.Add(paramPriority);

				foreach (var entry in entries)
				{
					paramSequence.Value = entry.Sequence;

					var position = 0;
					foreach (var reading in entry.Reading)
					{
						paramPosition.Value = ++position;
						paramText.Value = reading.Text;
						paramPriority.Value = String.Join(",", reading.Priority);
						cmdInsertReading.ExecuteNonQuery();
					}
				}
			}

			using (var cmdInsertSense = this.db.CreateCommand())
			{
				cmdInsertSense.CommandText = @"
					INSERT INTO entries_sense(sequence, position, tags_misc, glossary)
					VALUES ($sequence, $position, $tags_misc, $glossary)
				";

				var paramSequence = cmdInsertSense.CreateParameter();
				paramSequence.ParameterName = "$sequence";
				cmdInsertSense.Parameters.Add(paramSequence);

				var paramPosition = cmdInsertSense.CreateParameter();
				paramPosition.ParameterName = "$position";
				cmdInsertSense.Parameters.Add(paramPosition);

				var paramTagsMisc = cmdInsertSense.CreateParameter();
				paramTagsMisc.ParameterName = "$tags_misc";
				cmdInsertSense.Parameters.Add(paramTagsMisc);

				var paramGlossary = cmdInsertSense.CreateParameter();
				paramGlossary.ParameterName = "$glossary";
				cmdInsertSense.Parameters.Add(paramGlossary);

				foreach (var entry in entries)
				{
					paramSequence.Value = entry.Sequence;

					var position = 0;
					var glossaryText = new StringBuilder();
					var filteredSenses = entry.Sense.Where(x => x.Lang == "eng" && !x.IsEmpty);
					foreach (var sense in filteredSenses)
					{
						paramPosition.Value = ++position;
						paramTagsMisc.Value = String.Join(",", sense.Misc);

						glossaryText.Clear();
						foreach (var glossary in sense.Glossary)
						{
							if (glossaryText.Length > 0)
							{
								glossaryText.Append(Dictionary.EntryDatabase.GLOSSARY_ENTRY_SEPARATOR);
							}
							glossaryText.AppendFormat("{0}{1}{2}",
								glossary.Type,
								Dictionary.EntryDatabase.GLOSSARY_FIELD_SEPARATOR,
								glossary.Text);
						}
						paramGlossary.Value = glossaryText.ToString();
						cmdInsertSense.ExecuteNonQuery();
					}
				}
			}

			trans.Commit();
		}
	}
}
