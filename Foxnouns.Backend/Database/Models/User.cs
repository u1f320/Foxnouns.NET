// Copyright (C) 2023-present sam/u1f320 (vulpine.solutions)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

// ReSharper disable UnusedAutoPropertyAccessor.Global
using System.ComponentModel.DataAnnotations.Schema;
using Foxnouns.Backend.Utils;
using Newtonsoft.Json;
using NodaTime;

namespace Foxnouns.Backend.Database.Models;

public class User : BaseModel
{
    public required string Username { get; set; }
    public string Sid { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? MemberTitle { get; set; }
    public string? Avatar { get; set; }
    public string[] Links { get; set; } = [];
    public bool ListHidden { get; set; }
    public string? Timezone { get; set; }

    public List<FieldEntry> Names { get; set; } = [];
    public List<Pronoun> Pronouns { get; set; } = [];
    public List<Field> Fields { get; set; } = [];
    public Dictionary<Snowflake, CustomPreference> CustomPreferences { get; set; } = [];

    public List<PrideFlag> Flags { get; set; } = [];
    public List<UserFlag> ProfileFlags { get; set; } = [];

    public UserRole Role { get; set; } = UserRole.User;
    public string? Password { get; set; } // Password may be null if the user doesn't authenticate with an email address

    public List<Member> Members { get; } = [];
    public List<AuthMethod> AuthMethods { get; } = [];
    public List<DataExport> DataExports { get; } = [];
    public UserSettings Settings { get; set; } = new();

    public required Instant LastActive { get; set; }
    public Instant LastSidReroll { get; set; }

    public bool Deleted { get; set; }
    public Instant? DeletedAt { get; set; }
    public Snowflake? DeletedBy { get; set; }

    [NotMapped]
    public bool? SelfDelete => Deleted ? DeletedBy != null : null;

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

    public static readonly Duration DeleteAfter = Duration.FromDays(30);
    public static readonly Duration DeleteSuspendedAfter = Duration.FromDays(180);
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
    public bool? DarkMode { get; set; }
}
