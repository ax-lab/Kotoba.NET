public class FrequencyTest
{
	[Fact]
	public void Get_returns_null_for_entry_not_available()
	{
		Frequency.Get("not-available").Should().BeNull();
	}

	[Fact]
	public void Get_returns_frequency_information()
	{
		var expectedFrequency = new Frequency.Entry
		{
			InnocentCorpus = 146448,
			WorldLex = new Frequency.WorldLex
			{
				Blog = 14786,
				BlogPerMillion = 995.02M,
				Twitter = 17859,
				TwitterPerMillion = 1499.50M,
				News = 10829,
				NewsPerMillion = 766.93M,
			}
		};

		Frequency.Get("時間").Should().Be(expectedFrequency);
	}

	[Fact]
	public void handles_nullable_frequency_data()
	{
		var freqA = Frequency.Get("◎");
		freqA.Should().NotBeNull();
		freqA!.InnocentCorpus.Should().BeNull();
		freqA.WorldLex.Should().NotBeNull();

		var freqB = Frequency.Get("ああいう");
		freqB.Should().NotBeNull();
		freqB!.InnocentCorpus.Should().NotBeNull();
		freqB.WorldLex.Should().BeNull();
	}
}
