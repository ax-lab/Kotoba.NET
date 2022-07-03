namespace Dictionary;

public record Entry
{
	public long Id { get; init; }

	public IReadOnlyList<EntryKanji> Kanji { get; init; } = new List<EntryKanji>();

	public IReadOnlyList<EntryReading> Reading { get; init; } = new List<EntryReading>();

	public IReadOnlyList<EntrySense> Sense { get; init; } = new List<EntrySense>();
}

public record EntryKanji
{
	public string Text { get; init; } = "";

	public IReadOnlyList<string> Priority { get; init; } = new List<string>();
}

public record EntryReading
{
	public string Text { get; init; } = "";

	public IReadOnlyList<string> Priority { get; init; } = new List<string>();
}

public record EntrySense
{
	public IReadOnlyList<EntrySenseGlossary> Glossary { get; init; } = new List<EntrySenseGlossary>();
}

public record EntrySenseGlossary
{
	public string Text { get; init; } = "";

	public string Type { get; init; } = "";
}
