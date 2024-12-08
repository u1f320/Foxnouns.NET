using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;

namespace Foxnouns.Backend.Services;

public class UserRendererService(
    DatabaseContext db,
    MemberRendererService memberRenderer,
    Config config
)
{
    public async Task<UserResponse> RenderUserAsync(
        User user,
        User? selfUser = null,
        Token? token = null,
        bool renderMembers = true,
        bool renderAuthMethods = false,
        string? overrideSid = null,
        CancellationToken ct = default
    ) =>
        await RenderUserInnerAsync(
            user,
            selfUser != null && selfUser.Id == user.Id,
            token?.Scopes ?? [],
            renderMembers,
            renderAuthMethods,
            overrideSid,
            ct
        );

    public async Task<UserResponse> RenderUserInnerAsync(
        User user,
        bool isSelfUser,
        string[] scopes,
        bool renderMembers = true,
        bool renderAuthMethods = false,
        string? overrideSid = null,
        CancellationToken ct = default
    )
    {
        scopes = scopes.ExpandScopes();
        bool tokenCanReadHiddenMembers = scopes.Contains("member.read") && isSelfUser;
        bool tokenHidden = scopes.Contains("user.read_hidden") && isSelfUser;
        bool tokenPrivileged = scopes.Contains("user.read_privileged") && isSelfUser;

        renderMembers = renderMembers && (!user.ListHidden || tokenCanReadHiddenMembers);
        renderAuthMethods = renderAuthMethods && tokenPrivileged;

        IEnumerable<Member> members = renderMembers
            ? await db.Members.Where(m => m.UserId == user.Id).OrderBy(m => m.Name).ToListAsync(ct)
            : [];
        // Unless the user is requesting their own members AND the token can read hidden members, we filter out unlisted members.
        if (!(isSelfUser && tokenCanReadHiddenMembers))
            members = members.Where(m => !m.Unlisted);

        List<UserFlag> flags = await db
            .UserFlags.Where(f => f.UserId == user.Id)
            .OrderBy(f => f.Id)
            .ToListAsync(ct);

        List<AuthMethod> authMethods = renderAuthMethods
            ? await db
                .AuthMethods.Where(a => a.UserId == user.Id)
                .Include(a => a.FediverseApplication)
                .ToListAsync(ct)
            : [];

        int? utcOffset = null;
        if (
            user.Timezone != null
            && TimeZoneInfo.TryFindSystemTimeZoneById(user.Timezone, out TimeZoneInfo? tz)
        )
        {
            utcOffset = (int)tz.GetUtcOffset(DateTimeOffset.UtcNow).TotalSeconds;
        }

        return new UserResponse(
            user.Id,
            overrideSid ?? user.Sid,
            user.Username,
            user.DisplayName,
            user.Bio,
            user.MemberTitle,
            AvatarUrlFor(user),
            user.Links,
            user.Names,
            user.Pronouns,
            user.Fields,
            user.CustomPreferences,
            flags.Select(f => RenderPrideFlag(f.PrideFlag)),
            utcOffset,
            user.Role,
            renderMembers
                ? members.Select(m => memberRenderer.RenderPartialMember(m, tokenHidden))
                : null,
            renderAuthMethods ? authMethods.Select(RenderAuthMethod) : null,
            tokenHidden ? user.ListHidden : null,
            tokenHidden ? user.LastActive : null,
            tokenHidden ? user.LastSidReroll : null,
            tokenHidden ? user.Timezone ?? "<none>" : null
        );
    }

    public static AuthMethodResponse RenderAuthMethod(AuthMethod a) =>
        new(
            a.Id,
            a.AuthType,
            a.RemoteId,
            a.FediverseApplication != null
                ? $"@{a.RemoteUsername}@{a.FediverseApplication.Domain}"
                : a.RemoteUsername
        );

    public PartialUser RenderPartialUser(User user) =>
        new(
            user.Id,
            user.Sid,
            user.Username,
            user.DisplayName,
            AvatarUrlFor(user),
            user.CustomPreferences
        );

    public string? AvatarUrlFor(User user) =>
        user.Avatar != null
            ? $"{config.MediaBaseUrl}/users/{user.Id}/avatars/{user.Avatar}.webp"
            : null;

    public string ImageUrlFor(PrideFlag flag) => $"{config.MediaBaseUrl}/flags/{flag.Hash}.webp";

    public PrideFlagResponse RenderPrideFlag(PrideFlag flag) =>
        new(flag.Id, ImageUrlFor(flag), flag.Name, flag.Description);
}
