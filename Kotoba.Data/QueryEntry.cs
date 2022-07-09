namespace Kotoba.Data;

public class QueryEntry
{
	public long Id { get; init; }

	public string Text { get; init; }

	public QueryEntry(Dictionary.Entry entry)
	{
		this.Id = entry.Id;
		this.Text = entry.Kanji.Select(x => x.Text).Concat(entry.Reading.Select(x => x.Text)).First();
	}
}

public class QueryEntryType : ObjectGraphType<QueryEntry>
{
	public QueryEntryType()
	{
		this.Name = "Entry";
		Field(x => x.Id).Description("Unique identifier for the entry");
		Field(x => x.Text).Description("Primary kanji or reading for the entry");
	}
}
