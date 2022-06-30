namespace Graph;

using GraphQL.Types;

public class Query : ObjectGraphType<Query>
{

	public Query() : base()
	{
		Field<StringGraphType>("hello", description: "Says hello!", resolve: context => this.Hello());
	}

	public string Hello() => "Hello from GraphQL!!!";
}
