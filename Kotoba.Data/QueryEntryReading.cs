namespace Kotoba.Data;

public class QueryEntryReading
{
	public string Text { get; init; }

	public QueryEntryReading(Dictionary.EntryReading reading)
	{
		this.Text = reading.Text;
	}
}

public class QueryEntryReadingType : ObjectGraphType<QueryEntryReading>
{
	public QueryEntryReadingType()
	{
		this.Name = "EntryReading";
		this.Description = "Reading element for an entry.";
		Field(x => x.Text).Description("Reading text.");
	}
}
