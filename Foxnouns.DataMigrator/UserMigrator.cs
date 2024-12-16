using System.Diagnostics.CodeAnalysis;
using Dapper;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Foxnouns.DataMigrator.Models;
using NodaTime.Extensions;
using Npgsql;
using Serilog;

namespace Foxnouns.DataMigrator;

public class UserMigrator(
    NpgsqlConnection conn,
    DatabaseContext context,
    GoUser goUser,
    Dictionary<int, Snowflake> fediverseApplicationIds,
    List<GoUserField> userFields,
    List<GoMemberField> memberFields,
    List<GoPrideFlag> prideFlags,
    List<GoProfileFlag> userFlags,
    List<GoProfileFlag> memberFlags
)
{
    private readonly Dictionary<string, Snowflake> _preferenceIds = new();
    private readonly Dictionary<string, Snowflake> _flagIds = new();
    private User? _user;

    public async Task MigrateAsync()
    {
        CreateNewUser();
        MigrateFlags();
        await MigrateMembersAsync();
    }

    [MemberNotNull(nameof(_user))]
    private void CreateNewUser()
    {
        _user = new User
        {
            Id = goUser.SnowflakeId,
            Username = goUser.Username,
            DisplayName = goUser.DisplayName,
            Bio = goUser.Bio,
            Links = goUser.Links ?? [],

            Deleted = goUser.DeletedAt != null,
            DeletedAt = goUser.DeletedAt?.ToInstant(),
            DeletedBy = goUser.SelfDelete == true ? null : goUser.SnowflakeId,

            Names = goUser.Names.Select(ConvertFieldEntry).ToList(),
            Pronouns = goUser.Pronouns.Select(ConvertPronoun).ToList(),
            Avatar = goUser.Avatar,

            Role = goUser.IsAdmin ? UserRole.Admin : UserRole.User,
            MemberTitle = goUser.MemberTitle,
            ListHidden = goUser.ListPrivate,
            CustomPreferences = ConvertPreferences(),
            LastActive = goUser.LastActive.ToInstant(),
            Sid = goUser.Sid,
            LastSidReroll = goUser.LastSidReroll.ToInstant(),
            Timezone = goUser.Timezone,
            Fields = userFields
                .Where(f => f.UserId == goUser.Id)
                .Select(f => new Field
                {
                    Name = f.Name,
                    Entries = f.Entries.Select(ConvertFieldEntry).ToArray(),
                })
                .ToList(),
        };
        context.Users.Add(_user);

        // Create the user's auth methods
        if (goUser.Discord != null)
        {
            context.AuthMethods.Add(
                new AuthMethod
                {
                    Id = SnowflakeGenerator.Instance.GenerateSnowflake(_user.Id.Time),
                    UserId = _user.Id,
                    AuthType = AuthType.Discord,
                    RemoteId = goUser.Discord,
                    RemoteUsername = goUser.DiscordUsername ?? "(unknown)",
                }
            );
        }

        if (goUser.Google != null)
        {
            context.AuthMethods.Add(
                new AuthMethod
                {
                    Id = SnowflakeGenerator.Instance.GenerateSnowflake(_user.Id.Time),
                    UserId = _user.Id,
                    AuthType = AuthType.Google,
                    RemoteId = goUser.Google,
                    RemoteUsername = goUser.GoogleUsername ?? "(unknown)",
                }
            );
        }

        if (goUser.Tumblr != null)
        {
            context.AuthMethods.Add(
                new AuthMethod
                {
                    Id = SnowflakeGenerator.Instance.GenerateSnowflake(_user.Id.Time),
                    UserId = _user.Id,
                    AuthType = AuthType.Tumblr,
                    RemoteId = goUser.Tumblr,
                    RemoteUsername = goUser.TumblrUsername ?? "(unknown)",
                }
            );
        }

        if (goUser.Fediverse != null)
        {
            context.AuthMethods.Add(
                new AuthMethod
                {
                    Id = SnowflakeGenerator.Instance.GenerateSnowflake(_user.Id.Time),
                    UserId = _user.Id,
                    AuthType = AuthType.Fediverse,
                    RemoteId = goUser.Fediverse,
                    RemoteUsername = goUser.FediverseUsername ?? "(unknown)",
                    FediverseApplicationId = fediverseApplicationIds[goUser.FediverseAppId!.Value],
                }
            );
        }
    }

    private void MigrateFlags()
    {
        foreach (GoPrideFlag flag in prideFlags.Where(f => f.UserId == goUser.Id))
        {
            _flagIds[flag.Id] = flag.SnowflakeId;
            context.PrideFlags.Add(
                new PrideFlag
                {
                    Id = flag.SnowflakeId,
                    UserId = _user!.Id,
                    Hash = flag.Hash,
                    Name = flag.Name,
                    Description = flag.Description,
                }
            );
        }

        context.UserFlags.AddRange(
            userFlags
                .Where(f => f.UserId == goUser.Id)
                .Where(f => _flagIds.ContainsKey(f.FlagId))
                .Select(f => new UserFlag { UserId = _user!.Id, PrideFlagId = _flagIds[f.FlagId] })
        );
    }

    private async Task MigrateMembersAsync()
    {
        List<GoMember> members = (
            await conn.QueryAsync<GoMember>(
                "select * from members where user_id = @Id",
                new { goUser.Id }
            )
        ).ToList();

        foreach (GoMember member in members)
        {
            Log.Debug("Migrating member {Id}/{SnowflakeId}", member.Id, member.SnowflakeId);
            MigrateMember(member);
        }
    }

    private void MigrateMember(GoMember goMember)
    {
        var names = goMember.Names.Select(ConvertFieldEntry).ToList();
        var pronouns = goMember.Pronouns.Select(ConvertPronoun).ToList();
        var fields = memberFields
            .Where(f => f.MemberId == goMember.Id)
            .Select(f => new Field
            {
                Name = f.Name,
                Entries = f.Entries.Select(ConvertFieldEntry).ToArray(),
            })
            .ToList();

        var member = new Member
        {
            Id = goMember.SnowflakeId,
            UserId = _user!.Id,
            Name = goMember.Name,
            Sid = goMember.Sid,
            DisplayName = goMember.DisplayName,
            Bio = goMember.Bio,
            Avatar = goMember.Avatar,
            Links = goMember.Links ?? [],
            Unlisted = goMember.Unlisted,
            Names = names,
            Pronouns = pronouns,
            Fields = fields,
        };
        context.Members.Add(member);

        context.MemberFlags.AddRange(
            memberFlags
                .Where(f => f.MemberId == goMember.Id)
                .Select(f => new MemberFlag
                {
                    MemberId = member.Id,
                    PrideFlagId = _flagIds[f.FlagId],
                })
        );
    }

    private Dictionary<Snowflake, User.CustomPreference> ConvertPreferences()
    {
        var prefs = new Dictionary<Snowflake, User.CustomPreference>();

        foreach ((string id, GoCustomPreference goPref) in goUser.CustomPreferences)
        {
            Snowflake newId = SnowflakeGenerator.Instance.GenerateSnowflake(
                goUser.SnowflakeId.Time
            );
            _preferenceIds[id] = newId;
            prefs[newId] = new User.CustomPreference
            {
                Icon = goPref.Icon,
                Tooltip = goPref.Tooltip,
                Muted = goPref.Muted,
                Favourite = goPref.Favourite,
                Size = goPref.PreferenceSize switch
                {
                    "large" => PreferenceSize.Large,
                    "normal" => PreferenceSize.Normal,
                    "small" => PreferenceSize.Small,
                    _ => PreferenceSize.Normal,
                },
            };
        }

        return prefs;
    }

    private FieldEntry ConvertFieldEntry(GoFieldEntry entry) =>
        new() { Value = entry.Value, Status = ConvertPreferenceId(entry.Status) };

    private Pronoun ConvertPronoun(GoPronounEntry entry) =>
        new()
        {
            Value = entry.Pronouns,
            DisplayText = entry.DisplayText,
            Status = ConvertPreferenceId(entry.Status),
        };

    private string ConvertPreferenceId(string id)
    {
        if (_preferenceIds.TryGetValue(id, out Snowflake preferenceId))
            return preferenceId.ToString();
        return ValidationUtils.DefaultStatusOptions.Contains(id) ? id : "okay";
    }
}
