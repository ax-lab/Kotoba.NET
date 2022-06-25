namespace Dictionary;

public record Entry
{
	public ulong Id { get; init; }

	public IReadOnlyList<EntryKanji> Kanji { get; init; } = new List<EntryKanji>();

	public IReadOnlyList<EntryReading> Reading { get; init; } = new List<EntryReading>();
}

public record EntryKanji
{
	public string Text { get; init; } = "";
}

public record EntryReading
{
	public string Text { get; init; } = "";
}
