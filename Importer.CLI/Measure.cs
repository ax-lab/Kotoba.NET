using System.Diagnostics;

public static class Measure
{
	public static Stopwatch Start()
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		return stopwatch;
	}
}
