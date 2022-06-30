namespace Kotoba.Data;

using GraphQL;

public class Schema : GraphQL.Types.Schema
{
	public Schema()
	{
		Query = new QueryType();
	}

	public static async Task<string> Execute(string query)
	{
		const bool indent = true;
		var schema = new Schema();
		var writer = new GraphQL.SystemTextJson.GraphQLSerializer(indent, new DetailedErrorInfoProvider());
		return await schema.ExecuteAsync(writer, options =>
		{
			options.Query = query;
			options.Root = new Query();
		});
	}
}
