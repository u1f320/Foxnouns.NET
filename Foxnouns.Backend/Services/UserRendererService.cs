using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;

namespace Foxnouns.Backend.Services;

public class UserRendererService(DatabaseContext db, MemberRendererService memberRendererService, Config config)
{
    public async Task<UserResponse> RenderUserAsync(User user, User? selfUser = null,
        Token? token = null,
        bool renderMembers = true,
        bool renderAuthMethods = false)
    {
        var isSelfUser = selfUser?.Id == user.Id;
        var tokenCanReadHiddenMembers = token.HasScope("member.read") && isSelfUser;
        var tokenHidden = token.HasScope("user.read_hidden") && isSelfUser;
        var tokenPrivileged = token.HasScope("user.read_privileged") && isSelfUser;

        renderMembers = renderMembers &&
                        (!user.ListHidden || tokenCanReadHiddenMembers);
        renderAuthMethods = renderAuthMethods && tokenPrivileged;

        IEnumerable<Member> members =
            renderMembers ? await db.Members.Where(m => m.UserId == user.Id).ToListAsync() : [];
        // Unless the user is requesting their own members AND the token can read hidden members, we filter out unlisted members.
        if (!(isSelfUser && tokenCanReadHiddenMembers)) members = members.Where(m => m.Unlisted);

        var authMethods = renderAuthMethods
            ? await db.AuthMethods
                .Where(a => a.UserId == user.Id)
                .Include(a => a.FediverseApplication)
                .ToListAsync()
            : [];

        return new UserResponse(
            user.Id, user.Username, user.DisplayName, user.Bio, user.MemberTitle, AvatarUrlFor(user), user.Links,
            user.Names, user.Pronouns, user.Fields, user.CustomPreferences,
            renderMembers ? members.Select(memberRendererService.RenderPartialMember) : null,
            renderAuthMethods
                ? authMethods.Select(a => new AuthenticationMethodResponse(
                    a.Id, a.AuthType, a.RemoteId,
                    a.RemoteUsername, a.FediverseApplication?.Domain
                ))
                : null,
            tokenHidden ? user.ListHidden : null,
            tokenHidden ? user.LastActive : null
        );
    }

    public PartialUser RenderPartialUser(User user) =>
        new(user.Id, user.Username, user.DisplayName, AvatarUrlFor(user));

    private string? AvatarUrlFor(User user) =>
        user.Avatar != null ? $"{config.MediaBaseUrl}/users/{user.Id}/avatars/{user.Avatar}.webp" : null;

    public record UserResponse(
        Snowflake Id,
        string Username,
        string? DisplayName,
        string? Bio,
        string? MemberTitle,
        string? AvatarUrl,
        string[] Links,
        IEnumerable<FieldEntry> Names,
        IEnumerable<Pronoun> Pronouns,
        IEnumerable<Field> Fields,
        Dictionary<Snowflake, User.CustomPreference> CustomPreferences,
        [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        IEnumerable<MemberRendererService.PartialMember>? Members,
        [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        IEnumerable<AuthenticationMethodResponse>? AuthMethods,
        [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        bool? MemberListHidden,
        [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        Instant? LastActive
    );

    public record AuthenticationMethodResponse(
        Snowflake Id,
        [property: JsonConverter(typeof(ScreamingSnakeCaseEnumConverter))]
        AuthType Type,
        string RemoteId,
        [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        string? RemoteUsername,
        [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        string? FediverseInstance
    );

    public record PartialUser(
        Snowflake Id,
        string Username,
        string? DisplayName,
        string? AvatarUrl
    );
}