using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Extensions;
using Serilog;

namespace NetImporter;

public static class Users
{
    public static async Task ImportUsers(string filename)
    {
        await using var db = await NetImporter.GetContextAsync();
        await db.Database.ExecuteSqlRawAsync("SELECT 1");

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var users = NetImporter.ReadFromFile<ImportUser>(filename).Output.Select(ConvertUser).ToList();
        db.AddRange(users);
        await db.SaveChangesAsync();

        stopwatch.Stop();
        Log.Information("Imported {Count} users in {Duration}", users.Count, stopwatch.ElapsedDuration());
    }

    private static User ConvertUser(ImportUser oldUser)
    {
        var user = new User
        {
            Id = oldUser.Id,
            Username = oldUser.Username,
            DisplayName = oldUser.DisplayName,
            Bio = oldUser.Bio,
            MemberTitle = oldUser.MemberTitle,
            LastActive = oldUser.LastActive.ToInstant(),
            Avatar = oldUser.AvatarHash,
            Links = oldUser.Links ?? [],

            Role = oldUser.ParseRole(),
            Deleted = oldUser.Deleted,
            DeletedAt = oldUser.DeletedAt?.ToInstant(),
            DeletedBy = null
        };

        if (oldUser is { DiscordId: not null, DiscordUsername: not null })
        {
            user.AuthMethods.Add(new AuthMethod
            {
                Id = SnowflakeGenerator.Instance.GenerateSnowflake(),
                AuthType = AuthType.Discord,
                RemoteId = oldUser.DiscordId,
                RemoteUsername = oldUser.DiscordUsername
            });
        }

        if (oldUser is { TumblrId: not null, TumblrUsername: not null })
        {
            user.AuthMethods.Add(new AuthMethod
            {
                Id = SnowflakeGenerator.Instance.GenerateSnowflake(),
                AuthType = AuthType.Tumblr,
                RemoteId = oldUser.TumblrId,
                RemoteUsername = oldUser.TumblrUsername
            });
        }

        if (oldUser is { GoogleId: not null, GoogleUsername: not null })
        {
            user.AuthMethods.Add(new AuthMethod
            {
                Id = SnowflakeGenerator.Instance.GenerateSnowflake(),
                AuthType = AuthType.Google,
                RemoteId = oldUser.GoogleId,
                RemoteUsername = oldUser.GoogleUsername
            });
        }

        // Convert all custom preference UUIDs to snowflakes
        var prefMapping = new Dictionary<string, Snowflake>();
        foreach (var (key, value) in oldUser.CustomPreferences)
        {
            var newKey = SnowflakeGenerator.Instance.GenerateSnowflake();
            prefMapping[key] = newKey;
            user.CustomPreferences[newKey] = value;
        }

        foreach (var name in oldUser.Names ?? [])
        {
            user.Names.Add(new FieldEntry
            {
                Value = name.Value,
                Status = prefMapping.TryGetValue(name.Status, out var newStatus) ? newStatus.ToString() : name.Status,
            });
        }

        foreach (var pronoun in oldUser.Pronouns ?? [])
        {
            user.Pronouns.Add(new Pronoun
            {
                Value = pronoun.Value,
                DisplayText = pronoun.DisplayText,
                Status = prefMapping.TryGetValue(pronoun.Status, out var newStatus)
                    ? newStatus.ToString()
                    : pronoun.Status,
            });
        }

        foreach (var field in oldUser.Fields ?? [])
        {
            var entries = field.Entries.Select(entry => new FieldEntry
                {
                    Value = entry.Value,
                    Status = prefMapping.TryGetValue(entry.Status, out var newStatus)
                        ? newStatus.ToString()
                        : entry.Status,
                })
                .ToList();

            user.Fields.Add(new Field
            {
                Name = field.Name,
                Entries = entries.ToArray()
            });
        }

        Log.Debug("Converted user {UserId}", oldUser.Id);

        return user;
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private record ImportUser(
        Snowflake Id,
        string Sid,
        string Username,
        string? DisplayName,
        string? Bio,
        string? MemberTitle,
        OffsetDateTime LastActive,
        string? AvatarHash,
        string[]? Links,
        FieldEntry[]? Names,
        Pronoun[]? Pronouns,
        Field[]? Fields,
        string? DiscordId,
        string? DiscordUsername,
        string? FediverseId,
        string? FediverseUsername,
        long? FediverseAppId,
        string? TumblrId,
        string? TumblrUsername,
        string? GoogleId,
        string? GoogleUsername,
        bool MemberListHidden,
        string? Timezone,
        string Role,
        bool Deleted,
        OffsetDateTime? DeletedAt,
        string? DeleteReason,
        Dictionary<string, User.CustomPreference> CustomPreferences)
    {
        public UserRole ParseRole() => Role switch
        {
            "USER" => UserRole.User,
            "MODERATOR" => UserRole.Moderator,
            "ADMIN" => UserRole.Admin,
            _ => UserRole.User
        };
    }
}