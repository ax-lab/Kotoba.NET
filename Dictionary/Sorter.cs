using System.Text.RegularExpressions;

namespace Dictionary;

/// <summary>
/// Implements the underlying sorting order logic for two dictionary entries.
/// </summary>
public static class Sorter
{
	/// <summary>
	/// Encapsulates what the sorter cares about each entry.
	/// </summary>
	public class Args
	{
		public IEnumerable<string>? Priority = null;
	}

	/// <summary>
	/// Compares information about two entries and returns the sort order
	/// between them considering higher relevance first.
	/// </summary>
	/// <returns>
	/// The relative order of <paramref name="a"/> and <paramref name="b"/> as
	/// -1 for <paramref name="a"/> first,
	///  0 for both equal, or
	/// +1 for <paramref name="b"/> first.
	/// </returns>
	public static int Compare(Args a, Args b)
	{
		var aPri = NoPriorityTagGroup;
		var bPri = NoPriorityTagGroup;

		if (a.Priority != null)
		{
			foreach (var tag in a.Priority)
			{
				aPri = Math.Min(aPri, GetPriorityGroup(tag));
			}
		}

		if (b.Priority != null)
		{
			foreach (var tag in b.Priority)
			{
				bPri = Math.Min(bPri, GetPriorityGroup(tag));
			}
		}

		if (aPri != bPri)
		{
			return aPri.CompareTo(bPri);
		}

		return 0;
	}

	#region Priority tags

	static readonly Regex reNFrequencyTag = new Regex(@"^nf\d{2}$");

	const int NoPriorityTagGroup = 999;

	public static int GetPriorityGroup(string tag)
	{
		switch (tag)
		{
			case "news1":
			case "ichi1":
			case "spec1":
				return 0;
			case "spec2":
				return 1;
			case "gai1":
				return 2;
			case "gai2":
				return 3;
			case "news2":
				return 4;
			case "ichi2":
				return 5;
		}

		if (reNFrequencyTag.IsMatch(tag))
		{
			return 100 + int.Parse(tag.Substring(2));
		}

		return NoPriorityTagGroup;
	}

	#endregion
}
