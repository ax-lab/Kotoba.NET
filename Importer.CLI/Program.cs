namespace Importer.CLI;

public static class Program
{
	static int Main(string[] args)
	{
		Console.WriteLine("Dictionary data importer...\n");

		bool isHelp = args.Length > 0 && args[0] == "help";
		if (args.Length != 2 || isHelp)
		{
			if (!isHelp)
			{
				Console.Error.WriteLine("error: invalid arguments\n");
			}

			PrintUsage();
			return isHelp ? 0 : 1;
		}

		if (args[0] != "import")
		{
			Console.Error.WriteLine("error: invalid command: {0}\n", args[0]);
			PrintUsage();
			return 1;
		}

		var targetDirectory = Directory.CreateDirectory(args[1]);
		Console.WriteLine("[i] Data will be imported to {0}", targetDirectory.FullName);

		ImportEntries(targetDirectory.FullName);

		return 0;
	}

	static void PrintUsage()
	{
		Console.WriteLine("Usage:");
		Console.WriteLine();
		Console.WriteLine("    Importer.CLI.exe import {{DATA DIRECTORY}}");
		Console.WriteLine();
	}

	static void ImportEntries(string targetDirectory)
	{
		var fileName = Path.Join(targetDirectory, "entries.db");
		Console.WriteLine(">>> Importing dictionary entries <<<");
		using (var db = new EntriesWriter(fileName))
		{
			if (db.HasEntries)
			{
				Console.WriteLine("=== {0} already has entries, skipping!", db.Name);
				return;
			}

			Console.WriteLine("--> Generating new {0}...", db.Name);
			using (var dict = Importer.JMDict.Open())
			{
				var importTime = Measure.Start();
				var entries = dict.ReadEntries().ToList();
				importTime.Stop();

				Console.WriteLine("... Imported {0} entries in {1}...", entries.Count, importTime.Elapsed);

				var frequencyTime = Measure.Start();
				var innocentCorpus = Frequency.OpenInnocentCorpus();
				var worldLex = Frequency.OpenWorldLex();
				frequencyTime.Stop();
				// count below is -1 due to the total count entry
				Console.WriteLine("... Imported {0} and {1} frequency entries in {2}...",
					innocentCorpus.Count - 1, worldLex.Count - 1, frequencyTime.Elapsed);

				var entryComparer = Comparer<(JMDict.Entry, Dictionary.Sorter.Args)>.Create((a, b) =>
				{
					var entryA = a.Item1;
					var entryB = b.Item1;

					// First sort by priority and frequency...
					var argsA = a.Item2;
					var argsB = b.Item2;
					var argsCmp = Dictionary.Sorter.Compare(argsA, argsB);
					if (argsCmp != 0)
					{
						return argsCmp;
					}

					// ...failing that, sort by the entry text...
					var textA = entryA.Kanji
						.Select(x => x.Text)
						.Concat(entryA.Reading.Select(x => x.Text))
						.First();
					var textB = entryB.Kanji
						.Select(x => x.Text)
						.Concat(entryB.Reading.Select(x => x.Text))
						.First();
					var textCmp = textA.CompareTo(textB);
					if (textCmp != 0)
					{
						return textCmp;
					}

					// ...otherwise, just keep it in the input order.
					return entryA.Sequence.CompareTo(entryB.Sequence);
				});

				var sortTime = Measure.Start();
				var sortedEntries = entries
					.Select(x => (x, x.GetSorterArgs(innocentCorpus, worldLex)))
					.OrderBy(x => x, entryComparer)
					.Select(x => x.Item1)
					.ToList();
				sortTime.Stop();
				Console.WriteLine("... Sorted entries in {0}...", sortTime.Elapsed);

				Console.WriteLine("... Creating database...");
				var writeTime = Measure.Start();
				db.InsertEntries(sortedEntries, dict.Tags, new EntriesWriter.FrequencyData
				{
					WorldLex = worldLex,
					InnocentCorpus = innocentCorpus,
				});
				writeTime.Stop();

				Console.WriteLine("=== Database write took {0}", writeTime.Elapsed);
			}
		}
	}
}
