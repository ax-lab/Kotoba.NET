namespace Dictionary;

public record Tag
{
	public string Name { get; init; }
	public string Info { get; init; }

	public Tag(string name, string info)
	{
		this.Name = name;
		this.Info = info;
	}
}
