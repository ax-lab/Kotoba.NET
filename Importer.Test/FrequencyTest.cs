public class FrequencyTest
{

	[Fact]
	public void loads_world_lex_frequency_data()
	{
		var frequency = Frequency.OpenWorldLex();

		var entry = frequency["感じ"];
		entry.Word.Should().Be("感じ");
		entry.Blog.Should().Be(new Frequency.WorldLex.Info
		{
			Frequency = 15174,
			PerMillion = 1021.13M,
			ContextualDiversity = 14293,
			ContextualDiversityPercentage = 2.1516M,
		});

		entry.Twitter.Should().Be(new Frequency.WorldLex.Info
		{
			Frequency = 11884,
			PerMillion = 997.82M,
			ContextualDiversity = 11387,
			ContextualDiversityPercentage = 1.7069M,
		});

		entry.News.Should().Be(new Frequency.WorldLex.Info
		{
			Frequency = 3970,
			PerMillion = 281.16M,
			ContextualDiversity = 3809,
			ContextualDiversityPercentage = 1.2173M,
		});

		// Sanity check the frequency totals using the approximate count from
		// the file.
		var totals = frequency[""];
		totals.Blog.Frequency.Should().BeGreaterThan(14_000_000L);
		totals.Twitter.Frequency.Should().BeGreaterThan(11_000_000L);
		totals.News.Frequency.Should().BeGreaterThan(14_000_000L);
	}

	[Fact]
	public void load_innocent_corpus_frequency_data()
	{
		var frequency = Frequency.OpenInnocentCorpus();
		frequency["分かる"].Should().Be(376798);
		frequency[""].Should().BeGreaterThan(100_000_000L);
	}
}
