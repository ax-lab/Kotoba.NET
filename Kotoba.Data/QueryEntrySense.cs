namespace Kotoba.Data;

public class QueryEntrySense
{
	public IReadOnlyList<QueryEntrySenseGlossary> Glossary { get; init; }

	public QueryEntrySense(Dictionary.EntrySense sense)
	{
		this.Glossary = sense.Glossary.Select(x => new QueryEntrySenseGlossary(x)).ToList().AsReadOnly();
	}
}

public class QueryEntrySenseType : ObjectGraphType<QueryEntrySense>
{
	public QueryEntrySenseType()
	{
		this.Name = "EntrySense";
		this.Description = "Sense element for a dictionary Entry.";
		Field(x => x.Glossary).Description("Glossary elements for this sense.");
	}
}
