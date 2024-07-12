using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;

namespace Foxnouns.Backend.Services;

public class MemberRendererService(DatabaseContext db, Config config)
{
    public PartialMember RenderPartialMember(Member member) => new(member.Id, member.Name,
        member.DisplayName, member.Bio, AvatarUrlFor(member), member.Names, member.Pronouns);

    private string? AvatarUrlFor(Member member) =>
        member.Avatar != null ? $"{config.MediaBaseUrl}/members/{member.Id}/avatars/{member.Avatar}.webp" : null;

    public record PartialMember(
        Snowflake Id,
        string Name,
        string? DisplayName,
        string? Bio,
        string? AvatarUrl,
        IEnumerable<FieldEntry> Names,
        IEnumerable<Pronoun> Pronouns);
}