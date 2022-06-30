namespace Kotoba.Data;

using GraphQL;
using GraphQL.Types;

public class Query
{
	public long TotalEntries
	{
		get => Dictionary.Entries.Count;
	}
}

public class QueryType : ObjectGraphType<Query>
{
	public QueryType()
	{
		Field(x => x.TotalEntries).Description("Total number of entries in the dictionary.");
	}
}
