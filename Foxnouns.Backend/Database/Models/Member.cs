namespace Foxnouns.Backend.Database.Models;

public class Member : BaseModel
{
    public required string Name { get; set; }
    public string Sid { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? Avatar { get; set; }
    public string[] Links { get; set; } = [];
    public bool Unlisted { get; set; }

    public List<FieldEntry> Names { get; set; } = [];
    public List<Pronoun> Pronouns { get; set; } = [];
    public List<Field> Fields { get; set; } = [];

    public Snowflake UserId { get; init; }
    public User User { get; init; } = null!;
}