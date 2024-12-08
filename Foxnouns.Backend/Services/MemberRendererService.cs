using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Foxnouns.Backend.Services;

public class MemberRendererService(DatabaseContext db, Config config)
{
    public async Task<IEnumerable<PartialMember>> RenderUserMembersAsync(User user, Token? token)
    {
        bool canReadHiddenMembers =
            token != null && token.UserId == user.Id && token.HasScope("member.read");
        bool renderUnlisted =
            token != null && token.UserId == user.Id && token.HasScope("user.read_hidden");
        bool canReadMemberList = !user.ListHidden || canReadHiddenMembers;

        IEnumerable<Member> members = canReadMemberList
            ? await db.Members.Where(m => m.UserId == user.Id).OrderBy(m => m.Name).ToListAsync()
            : [];
        if (!canReadHiddenMembers)
            members = members.Where(m => !m.Unlisted);
        return members.Select(m => RenderPartialMember(m, renderUnlisted));
    }

    public MemberResponse RenderMember(
        Member member,
        Token? token = null,
        string? overrideSid = null
    )
    {
        bool renderUnlisted = token?.UserId == member.UserId && token.HasScope("user.read_hidden");

        return new MemberResponse(
            member.Id,
            overrideSid ?? member.Sid,
            member.Name,
            member.DisplayName ?? member.Name,
            member.Bio,
            AvatarUrlFor(member),
            member.Links,
            member.Names,
            member.Pronouns,
            member.Fields,
            member.ProfileFlags.Select(f => RenderPrideFlag(f.PrideFlag)),
            RenderPartialUser(member.User),
            renderUnlisted ? member.Unlisted : null
        );
    }

    private PartialUser RenderPartialUser(User user) =>
        new(
            user.Id,
            user.Sid,
            user.Username,
            user.DisplayName,
            AvatarUrlFor(user),
            user.CustomPreferences
        );

    public PartialMember RenderPartialMember(Member member, bool renderUnlisted = false) =>
        new(
            member.Id,
            member.Sid,
            member.Name,
            member.DisplayName ?? member.Name,
            member.Bio,
            AvatarUrlFor(member),
            member.Names,
            member.Pronouns,
            renderUnlisted ? member.Unlisted : null
        );

    public string? AvatarUrlFor(Member member) =>
        member.Avatar != null
            ? $"{config.MediaBaseUrl}/members/{member.Id}/avatars/{member.Avatar}.webp"
            : null;

    private string? AvatarUrlFor(User user) =>
        user.Avatar != null
            ? $"{config.MediaBaseUrl}/users/{user.Id}/avatars/{user.Avatar}.webp"
            : null;

    private string ImageUrlFor(PrideFlag flag) => $"{config.MediaBaseUrl}/flags/{flag.Hash}.webp";

    private PrideFlagResponse RenderPrideFlag(PrideFlag flag) =>
        new(flag.Id, ImageUrlFor(flag), flag.Name, flag.Description);
}
