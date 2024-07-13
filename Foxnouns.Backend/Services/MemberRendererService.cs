using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Foxnouns.Backend.Services;

public class MemberRendererService(DatabaseContext db, Config config)
{
    public async Task<IEnumerable<PartialMember>> RenderUserMembersAsync(User user, Token? token)
    {
        var canReadHiddenMembers = token != null && token.UserId == user.Id && token.HasScope("member.read");
        var canReadMemberList = !user.ListHidden || canReadHiddenMembers;

        IEnumerable<Member> members = canReadMemberList
            ? await db.Members
                .Where(m => m.UserId == user.Id)
                .OrderBy(m => m.Name)
                .ToListAsync()
            : [];
        if (!canReadHiddenMembers) members = members.Where(m => !m.Unlisted);
        return members.Select(RenderPartialMember);
    }

    public MemberResponse RenderMember(Member member, Token? token)
    {
        var renderUnlisted = token?.UserId == member.UserId && token.HasScope("user.read_hidden");

        return new MemberResponse(
            member.Id, member.Name, member.DisplayName, member.Bio,
            AvatarUrlFor(member), member.Links, member.Names, member.Pronouns, member.Fields,
            RenderPartialUser(member.User), renderUnlisted ? member.Unlisted : null);
    }

    private UserRendererService.PartialUser RenderPartialUser(User user) =>
        new(user.Id, user.Username, user.DisplayName, AvatarUrlFor(user));

    public PartialMember RenderPartialMember(Member member) => new(member.Id, member.Name,
        member.DisplayName, member.Bio, AvatarUrlFor(member), member.Names, member.Pronouns);

    private string? AvatarUrlFor(Member member) =>
        member.Avatar != null ? $"{config.MediaBaseUrl}/members/{member.Id}/avatars/{member.Avatar}.webp" : null;

    private string? AvatarUrlFor(User user) =>
        user.Avatar != null ? $"{config.MediaBaseUrl}/users/{user.Id}/avatars/{user.Avatar}.webp" : null;

    public record PartialMember(
        Snowflake Id,
        string Name,
        string? DisplayName,
        string? Bio,
        string? AvatarUrl,
        IEnumerable<FieldEntry> Names,
        IEnumerable<Pronoun> Pronouns);

    public record MemberResponse(
        Snowflake Id,
        string Name,
        string? DisplayName,
        string? Bio,
        string? AvatarUrl,
        string[] Links,
        IEnumerable<FieldEntry> Names,
        IEnumerable<Pronoun> Pronouns,
        IEnumerable<Field> Fields,
        UserRendererService.PartialUser User,
        [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        bool? Unlisted);
}