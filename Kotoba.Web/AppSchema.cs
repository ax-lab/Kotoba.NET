using GraphQL.Types;

public class AppSchema : Schema
{
	public AppSchema() : base()
	{
		Query = new Graph.Query();
	}
}
