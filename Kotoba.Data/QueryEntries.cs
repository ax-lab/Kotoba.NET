namespace Kotoba.Data;

public class QueryEntries
{
	public const long DefaultPageSize = 25;

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

	public List<QueryEntry> List(long limit, long offset)
	{
		using (new Measure("Queries.List({0}, {1})", limit, offset))
		{
			return Dictionary.Entries.List(limit, offset).Select(x => new QueryEntry(x)).ToList();
		}
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

		const string argListLimit = "limit";
		const string argListOffset = "offset";
		Field<ListGraphType<QueryEntryType>>("list",
			arguments: new QueryArguments(
				new QueryArgument<IntGraphType>
				{
					Name = argListLimit,
					DefaultValue = QueryEntries.DefaultPageSize,
					Description = "Maximum number of entries to return.",
				},
				new QueryArgument<IntGraphType>
				{
					Name = argListOffset,
					DefaultValue = 0,
					Description = "Offset of the first entry in the list to return."
				}),
			description: "Query entries by their list position. Entries are sorted by frequency and relevance.",
			resolve: context =>
			{
				var limit = context.GetArgument<long>(argListLimit);
				var offset = context.GetArgument<long>(argListOffset);
				return context.Source.List(limit, offset);
			}
		);
	}
}
