namespace Kotoba.Data;

public class QueryEntryKanji
{
	public string Text { get; init; }

	public QueryEntryKanji(Dictionary.EntryKanji kanji)
	{
		this.Text = kanji.Text;
	}
}

public class QueryEntryKanjiType : ObjectGraphType<QueryEntryKanji>
{
	public QueryEntryKanjiType()
	{
		this.Name = "EntryKanji";
		this.Description = "Kanji element for an entry.";
		Field(x => x.Text).Description("Kanji text.");
	}
}
