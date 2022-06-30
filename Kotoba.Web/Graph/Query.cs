namespace Graph;

using GraphQL;
using GraphQL.Types;

public class Query : ObjectGraphType
{

	public Query() : base()
	{
		Field<StringGraphType>("hello",
			description: "Says hello!",
			arguments: new QueryArguments(
				new QueryArgument<StringGraphType>
				{
					Name = "name",
					Description = "Name to include in the message.",
				}
			),
			resolve: context =>
			{
				var name = context.GetArgument<string>("name");
				return this.Hello(name);
			}
		);
	}

	public string Hello(string name) => String.Format("Hello, {0}!!!", name);
}
