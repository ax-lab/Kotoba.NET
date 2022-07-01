namespace Kotoba.Data;

public class Query : ObjectGraphType<Query>
{
	public Query()
	{
		Field(x => x.Entries)
			.Description("Entrypoint for querying dictionary entries.")
			.Resolve((_) => this.Entries);
	}

	public QueryEntries Entries
	{
		get => new QueryEntries();
	}
}
