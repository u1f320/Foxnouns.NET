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
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Utils;
using Microsoft.EntityFrameworkCore;

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
        bool renderSettings = false,
        CancellationToken ct = default
    ) =>
        await RenderUserInnerAsync(
            user,
            selfUser != null && selfUser.Id == user.Id,
            token?.Scopes ?? [],
            renderMembers,
            renderAuthMethods,
            overrideSid,
            renderSettings,
            ct
        );

    public async Task<UserResponse> RenderUserInnerAsync(
        User user,
        bool isSelfUser,
        string[] scopes,
        bool renderMembers = true,
        bool renderAuthMethods = false,
        string? overrideSid = null,
        bool renderSettings = false,
        CancellationToken ct = default
    )
    {
        scopes = scopes.ExpandScopes();
        bool tokenCanReadHiddenMembers = scopes.Contains("member.read") && isSelfUser;
        bool tokenHidden = scopes.Contains("user.read_hidden") && isSelfUser;
        bool tokenPrivileged = scopes.Contains("user.read_privileged") && isSelfUser;

        renderMembers = renderMembers && (!user.ListHidden || tokenCanReadHiddenMembers);
        renderAuthMethods = renderAuthMethods && tokenPrivileged;
        renderSettings = renderSettings && tokenHidden;

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
            user.CustomPreferences.Select(x => (x.Key, RenderCustomPreference(x.Value)))
                .ToDictionary(),
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
            tokenHidden ? user.Timezone ?? "<none>" : null,
            tokenHidden ? user is { Deleted: true, DeletedBy: not null } : null,
            tokenHidden ? user.Deleted : null,
            renderSettings ? user.Settings : null
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

    public static CustomPreferenceResponse RenderCustomPreference(User.CustomPreference pref) =>
        new(pref.Icon, pref.Tooltip, pref.Muted, pref.Favourite, pref.Size);

    public static Dictionary<Snowflake, CustomPreferenceResponse> RenderCustomPreferences(
        User user
    ) =>
        user.CustomPreferences.Select(x => (x.Key, RenderCustomPreference(x.Value))).ToDictionary();

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

    public string? ImageUrlFor(PrideFlag flag) =>
        flag.Hash != null ? $"{config.MediaBaseUrl}/flags/{flag.Hash}.webp" : null;

    public PrideFlagResponse RenderPrideFlag(PrideFlag flag) =>
        new(flag.Id, ImageUrlFor(flag), flag.Name, flag.Description);
}
