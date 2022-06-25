namespace Importer.Test;

public class JMDict_Open
{
	[Fact]
	public void should_open_file()
	{
		using (var xml = JMDict.Open())
		{
			Assert.NotNull(xml);
			Assert.True(xml.Read());
		}
	}

	[Fact]
	public void should_read_entries()
	{
		using (var xml = JMDict.Open())
		{
			// avoid reading the entire file to keep the test fast
			var entries = JMDict.ReadEntries(xml).TakeWhile(x => String.Compare(x.Sequence, "1009999") < 0).ToList();

			var check = (string id, Action<JMDict.Entry> assertions) =>
			{
				var entry = entries.First(x => x.Sequence == id);
				Assert.Equal(entry.Sequence, id);
				assertions(entry);
			};

			// first entry in the file
			check("1000000", x =>
			{
				Assert.Equal(1, x.Reading.Count);
				Assert.Equal(new JMDict.Reading { Text = "ヽ" }, x.Reading[0]);
			});

			// entry with multiple readings
			check("1000040", x =>
			{
				Assert.Equal(2, x.Reading.Count);
				Assert.Equal(new JMDict.Reading { Text = "おなじ" }, x.Reading[0]);
				Assert.Equal(new JMDict.Reading { Text = "おなじく" }, x.Reading[1]);
			});

			// entry with multiple kanji
			check("1000110", x =>
			{
				Assert.Equal(2, x.Kanji.Count);
				Assert.Equal(new JMDict.Kanji { Text = "ＣＤプレーヤー" }, x.Kanji[0]);
				Assert.Equal(new JMDict.Kanji { Text = "ＣＤプレイヤー" }, x.Kanji[1]);
			});

			// entry with a kanji with priority and info
			check("1003810", x =>
			{
				Assert.Equal(1, x.Kanji.Count);
				Assert.Equal(new JMDict.Kanji { Text = "草臥れる" }, x.Kanji[0]);
			});
		}
	}
}
