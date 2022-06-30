using System.Diagnostics;

internal static class Node
{
	public static string AppRoot { get; set; } = ".";

	public static bool HasPackageJson
	{
		get => File.Exists(Path.Combine(AppRoot, "package.json"));
	}

	public static void BeginNpmCommand(string args)
	{
		var npmExe = NpmExe;
		if (npmExe == "")
		{
			return;
		}

		var npm = new Process();
		npm.StartInfo.FileName = npmExe;
		npm.StartInfo.Arguments = args;
		npm.StartInfo.WorkingDirectory = AppRoot;
		npm.StartInfo.RedirectStandardError = true;
		npm.StartInfo.RedirectStandardOutput = true;

		// Ctrl+C handling in `npm` and `dotnet watch` conflict, causing issues
		// in the terminal when the application closes the console. This solves
		// the problem.
		npm.StartInfo.RedirectStandardInput = true;

		npm.OutputDataReceived += (s, e) =>
		{
			if (!String.IsNullOrWhiteSpace(e.Data)) Console.WriteLine("[npm] inf: {0}", e.Data);
		};

		npm.ErrorDataReceived += (s, e) =>
		{
			if (!String.IsNullOrWhiteSpace(e.Data)) Console.Error.WriteLine("[npm] err: {0}", e.Data);
		};

		npm.Start();
		npm.BeginErrorReadLine();
		npm.BeginOutputReadLine();
	}

	private static string? _npmExe;

	private static string NpmExe
	{
		get
		{
			_npmExe = _npmExe ?? GetExecutable("npm");
			return _npmExe;
		}
	}

	private static string GetExecutable(string fileName)
	{
		string FindExe(string name)
		{
			var suffixes = new string[] { ".exe", ".cmd", "" };
			foreach (var suffix in suffixes)
			{
				var fileName = Path.GetFullPath(name + suffix);
				if (File.Exists(fileName))
				{
					return fileName;
				}
			}
			return "";
		}

		var exePath = FindExe(fileName);
		if (exePath == "")
		{
			var values = Environment.GetEnvironmentVariable("PATH") ?? "";
			foreach (var path in values.Split(Path.PathSeparator))
			{
				var basePath = Path.Combine(path, fileName);
				exePath = FindExe(basePath);
				if (exePath != "")
				{
					break;
				}
			}
		}

		return exePath;
	}
}
