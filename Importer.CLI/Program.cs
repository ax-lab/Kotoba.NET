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
		Console.WriteLine("-> Importing data to {0} ...", targetDirectory.FullName);

		using (var xml = Importer.JMDict.Open())
		{
			var importTime = Measure.Start();
			var entries = Importer.JMDict.ReadEntries(xml).Take(10000).ToList();
			importTime.Stop();

			Console.WriteLine("== Imported {0} entries in {1}", entries.Count, importTime.Elapsed);

			var fileName = Path.Join(targetDirectory.FullName, "entries.db");
			using (var db = new DatabaseWriter(fileName))
			{
				Console.WriteLine("-> Creating {0} ...", db.Name);
			}
		}

		return 0;
	}

	static void PrintUsage()
	{
		Console.WriteLine("Usage:");
		Console.WriteLine();
		Console.WriteLine("    Importer.CLI.exe import {{DATA DIRECTORY}}");
		Console.WriteLine();
	}
}
