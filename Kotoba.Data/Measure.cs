using System.Diagnostics;

namespace Kotoba.Data;

public class Measure : IDisposable
{
	private readonly string name;
	private readonly Stopwatch watch = new Stopwatch();

	public Measure(string name, params object[] args)
	{
		this.name = args != null && args.Length > 0 ? String.Format(name, args) : name;
		this.watch.Start();
	}

	public void Dispose()
	{
		this.watch.Stop();
		var elapsed = this.watch.Elapsed;
		var elapsedString = elapsed.TotalSeconds < 1
			? String.Format("{0:0.000} ms", elapsed.TotalMilliseconds)
			: elapsed.ToString();
		Console.WriteLine("[measure] {0} took {1}", this.name, elapsedString);
	}
}
