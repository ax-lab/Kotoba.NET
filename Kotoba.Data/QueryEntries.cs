namespace Kotoba.Data;

public class QueryEntries
{
	public long Count
	{
		get => Dictionary.Entries.Count;
	}

	public QueryEntry? ById(long id)
	{
		var entry = Dictionary.Entries.ById(id);
		return entry != null ? new QueryEntry(entry) : null;
	}

	public List<QueryEntry> ByIds(long[] ids)
	{
		return Dictionary.Entries.ByIds(ids).Select(x => new QueryEntry(x)).ToList();
	}
}

public class QueryEntriesType : ObjectGraphType<QueryEntries>
{
	public QueryEntriesType()
	{
		const string argId = "id";
		const string argIdList = "ids";

		this.Name = "Entries";
		Field(x => x.Count).Description("Total number of entries in the dictionary.");

		Field<QueryEntryType>("byId",
			arguments: new QueryArguments(
				new QueryArgument<IntGraphType> { Name = argId, Description = "Unique identifier for the entry." }
			),
			description: "Query a single entry by its id.",
			resolve: context =>
			{
				var id = context.GetArgument<long>(argId);
				return context.Source.ById(id);
			}
		);

		Field<ListGraphType<QueryEntryType>>("byIds",
			arguments: new QueryArguments(
				new QueryArgument<ListGraphType<IntGraphType>> { Name = argIdList, Description = "Unique identifier for the entries." }
			),
			description: "Query multiple entries by their ids.",
			resolve: context =>
			{
				var ids = context.GetArgument<long[]>(argIdList);
				return context.Source.ByIds(ids);
			}
		);
	}
}
