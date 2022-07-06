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
		/// <summary>
		/// Priority tags for the entry.
		/// </summary>
		public IEnumerable<string>? Priority;

		/// <summary>
		/// Frequency information when available.
		/// </summary>
		public Frequency.Entry? Frequency;
	}

	static readonly Regex reNFrequencyTag = new Regex(@"^nf\d{2}$");

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
		int? priA = null;
		int? priB = null;

		int? nfA = null;
		int? nfB = null;

		var parseTag = (string tag, ref int? pri, ref int? nf) =>
		{
			var tagPri = GetPriorityGroup(tag);
			if (tagPri != null)
			{
				if (pri == null || tagPri < pri)
				{
					pri = tagPri;
				}
			}
			else if (reNFrequencyTag.IsMatch(tag))
			{
				nf = int.Parse(tag.Substring(2));
			}
		};

		if (a.Priority != null)
		{
			foreach (var tag in a.Priority)
			{
				parseTag(tag, ref priA, ref nfA);
			}
		}

		if (b.Priority != null)
		{
			foreach (var tag in b.Priority)
			{
				parseTag(tag, ref priB, ref nfB);
			}
		}

		if (priA != priB)
		{
			return (priA ?? int.MaxValue).CompareTo(priB ?? int.MaxValue);
		}

		if (nfA != nfB)
		{
			return (nfA ?? int.MaxValue).CompareTo(nfB ?? int.MaxValue);
		}

		if (a.Frequency != null)
		{
			return a.Frequency.CompareTo(b.Frequency);
		}
		else if (b.Frequency != null)
		{
			return -b.Frequency.CompareTo(a.Frequency);
		}

		return 0;
	}

	#region Priority tags

	public static int? GetPriorityGroup(string tag)
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
		return null;
	}

	#endregion
}
