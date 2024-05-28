using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;

namespace Foxnouns.Backend.Services;

public class MemberRendererService(DatabaseContext db)
{
    public PartialMember RenderPartialMember(Member member) => new(member.Id, member.Name,
        member.DisplayName, member.Bio, member.Names, member.Pronouns);

    public record PartialMember(
        Snowflake Id,
        string Name,
        string? DisplayName,
        string? Bio,
        IEnumerable<FieldEntry> Names,
        IEnumerable<Pronoun> Pronouns);
}