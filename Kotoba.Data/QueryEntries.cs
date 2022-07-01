namespace Kotoba.Data;

public class QueryEntries
{
	public long Count
	{
		get => Dictionary.Entries.Count;
	}
}

public class QueryEntriesType : ObjectGraphType<QueryEntries>
{
	public QueryEntriesType()
	{
		this.Name = "Entries";
		Field(x => x.Count).Description("Total number of entries in the dictionary.");
	}
}
