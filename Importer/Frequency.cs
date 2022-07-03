namespace Importer;

/// <summary>
/// Support for loading frequency data sources.
/// </summary>
public class Frequency
{
	public record WorldLex
	{
		public struct Info
		{
			public long Frequency;
			public decimal PerMillion;
			public long ContextualDiversity;
			public decimal ContextualDiversityPercentage;
		}

		public string Word { get; init; } = "";

		public Info Blog { get; init; }

		public Info Twitter { get; init; }

		public Info News { get; init; }

		public static WorldLex FromLine(string line)
		{
			var fields = line.Trim().Split('\t');
			return new WorldLex
			{
				Word = fields[0],
				Blog = ParseInfo(fields[1], fields[2], fields[3], fields[4]),
				Twitter = ParseInfo(fields[5], fields[6], fields[7], fields[8]),
				News = ParseInfo(fields[9], fields[10], fields[11], fields[12]),
			};
		}

		private static Info ParseInfo(string freq, string pm, string cd, string cdPc)
		{
			var frequency = Int64.Parse(freq);
			var perMillion = Decimal.Parse(pm);
			var contextualDiversity = Int64.Parse(cd);
			var contextualDiversityPercentage = Decimal.Parse(cdPc);
			return new Info
			{
				Frequency = frequency,
				PerMillion = perMillion,
				ContextualDiversity = contextualDiversity,
				ContextualDiversityPercentage = contextualDiversityPercentage,
			};
		}
	}

	/// <summary>
	/// Open the `http://www.lexique.org` frequency file.
	/// </summary>
	/// <remarks>
	/// An additional empty entry will be returned containing the total
	/// frequency sum for all entries per category.
	/// </remarks>
	public static Dictionary<string, WorldLex> OpenWorldLex()
	{
		var totalBlog = 0L;
		var totalNews = 0L;
		var totalTwitter = 0L;

		var output = new Dictionary<string, WorldLex>();
		Data.ReadZippedLines("Jap.Freq.2.zip", "Jap.Freq.2.txt", (line, number) =>
		{
			if (number > 0 && !String.IsNullOrWhiteSpace(line))
			{
				// The file contains a trash line near the end followed by
				// useless entries.
				if (line.StartsWith("Word\t"))
				{
					return false;
				}
				var entry = WorldLex.FromLine(line);
				output.Add(entry.Word, entry);
				totalBlog += entry.Blog.Frequency;
				totalNews += entry.News.Frequency;
				totalTwitter += entry.Twitter.Frequency;
			}
			return true;
		});

		output.Add("", new WorldLex
		{
			Blog = new WorldLex.Info { Frequency = totalBlog },
			News = new WorldLex.Info { Frequency = totalNews },
			Twitter = new WorldLex.Info { Frequency = totalTwitter },
		});
		return output;
	}

	public static Dictionary<string, long> OpenInnocentCorpus()
	{
		var totals = 0L;

		var output = new Dictionary<string, long>();
		Data.ReadZippedLines(
			"Innocent_Novel_Analysis_120526.zip",
			"Innocent_Novel_Analysis_120526/word_freq_report_jparser.txt",
			(line, number) =>
			{
				if (!String.IsNullOrWhiteSpace(line))
				{
					var fields = line.Split('\t');
					var entry = fields[1];
					var count = Int64.Parse(fields[0]);
					output.Add(entry, count);
					totals += count;
				}
				return true;
			});

		output.Add("", totals);
		return output;
	}
}
