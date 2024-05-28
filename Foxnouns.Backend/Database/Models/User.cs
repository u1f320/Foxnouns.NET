namespace Foxnouns.Backend.Database.Models;

public class User : BaseModel
{
    public required string Username { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? MemberTitle { get; set; }
    public string? Avatar { get; set; }
    public string[] Links { get; set; } = [];
    public bool ListHidden { get; set; }

    public List<FieldEntry> Names { get; set; } = [];
    public List<Pronoun> Pronouns { get; set; } = [];
    public List<Field> Fields { get; set; } = [];

    public UserRole Role { get; set; } = UserRole.User;

    public List<Member> Members { get; } = [];
    public List<AuthMethod> AuthMethods { get; } = [];
}

public enum UserRole
{
    User,
    Moderator,
    Admin,
}