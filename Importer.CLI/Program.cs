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
		Console.WriteLine(">>> Importing dictionary entries...");
		using (var db = new EntriesWriter(fileName))
		{
			if (db.HasEntries)
			{
				Console.WriteLine("=== {0} already has entries, skipping!", db.Name);
				return;
			}

			Console.WriteLine("... Generating new {0}", db.Name);
			using (var xml = Importer.JMDict.Open())
			{
				var importTime = Measure.Start();
				var entries = Importer.JMDict.ReadEntries(xml).ToList();
				importTime.Stop();

				Console.WriteLine("... Imported {0} entries in {1}, writing database...", entries.Count, importTime.Elapsed);

				var writeTime = Measure.Start();
				db.InsertEntries(entries);
				writeTime.Stop();

				Console.WriteLine("=== Database write took {0}", writeTime.Elapsed);
			}
		}
	}
}
