namespace Kotoba.Data;

public class QueryEntry
{
	public long Id { get; init; }

	public long Position { get; init; }

	public string Text { get; init; }

	public IReadOnlyList<QueryEntryKanji> Kanji { get; init; }

	public IReadOnlyList<QueryEntryReading> Reading { get; init; }

	public IReadOnlyList<QueryEntrySense> Sense { get; init; }

	public QueryEntry(Dictionary.Entry entry)
	{
		this.Id = entry.Id;
		this.Position = entry.Position;
		this.Text = entry.Kanji.Select(x => x.Text).Concat(entry.Reading.Select(x => x.Text)).First();
		this.Kanji = entry.Kanji.Select(x => new QueryEntryKanji(x)).ToList().AsReadOnly();
		this.Reading = entry.Reading.Select(x => new QueryEntryReading(x)).ToList().AsReadOnly();
		this.Sense = entry.Sense.Select(x => new QueryEntrySense(x)).ToList().AsReadOnly();
	}
}

public class QueryEntryType : ObjectGraphType<QueryEntry>
{
	public QueryEntryType()
	{
		this.Name = "Entry";
		this.Description = "Main entry in the dictionary.";
		Field(x => x.Id).Description("Unique identifier for the entry.");
		Field(x => x.Position).Description("Position for this entry across the entire database, sorted by relevance.");
		Field(x => x.Text).Description("Primary kanji or reading for the entry.");
		Field(x => x.Kanji).Description("Kanji elements for this entry.");
		Field(x => x.Reading).Description("Reading elements for this entry.");
		Field(x => x.Sense).Description("Sense elements for this entry.");
	}
}
