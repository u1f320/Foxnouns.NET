using System.ComponentModel.DataAnnotations.Schema;
using Foxnouns.Backend.Utils;
using Newtonsoft.Json;
using NodaTime;

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
    public Dictionary<Snowflake, CustomPreference> CustomPreferences { get; set; } = [];

    public UserRole Role { get; set; } = UserRole.User;
    public string? Password { get; set; } // Password may be null if the user doesn't authenticate with an email address

    public List<Member> Members { get; } = [];
    public List<AuthMethod> AuthMethods { get; } = [];
    public UserSettings Settings { get; set; } = new();

    public required Instant LastActive { get; set; }

    public bool Deleted { get; set; }
    public Instant? DeletedAt { get; set; }
    public Snowflake? DeletedBy { get; set; }
    [NotMapped] public bool? SelfDelete => Deleted ? DeletedBy != null : null;

    public class CustomPreference
    {
        public required string Icon { get; set; }
        public required string Tooltip { get; set; }
        public bool Muted { get; set; }
        public bool Favourite { get; set; }

        // This type is generally serialized directly, so the converter is applied here.
        [JsonConverter(typeof(ScreamingSnakeCaseEnumConverter))]
        public PreferenceSize Size { get; set; }
    }
}

public enum UserRole
{
    User,
    Moderator,
    Admin,
}

public enum PreferenceSize
{
    Large,
    Normal,
    Small,
}

public class UserSettings
{
    public bool? DarkMode { get; set; } = null;
}