namespace Kotoba.Data;

public class QueryEntrySenseGlossary
{
	public string Text { get; init; }

	public QueryEntrySenseGlossary(Dictionary.EntrySenseGlossary glossary)
	{
		this.Text = glossary.Text;
	}
}

public class QueryEntrySenseGlossaryType : ObjectGraphType<QueryEntrySenseGlossary>
{
	public QueryEntrySenseGlossaryType()
	{
		this.Name = "EntrySenseGlossary";
		this.Description = "Glossary entry for a Sense.";
		Field(x => x.Text).Description("An English definition for this glossary.");
	}
}
