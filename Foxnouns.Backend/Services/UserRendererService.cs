using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Foxnouns.Backend.Services;

public class UserRendererService(DatabaseContext db, MemberRendererService memberRendererService)
{
    public async Task<object> RenderUserAsync(User user, User? selfUser = null, bool renderMembers = true)
    {
        renderMembers = renderMembers && (!user.ListHidden || selfUser?.Id == user.Id);

        var members = renderMembers ? await db.Members.Where(m => m.UserId == user.Id).ToListAsync() : [];

        return new UserResponse(
            user.Id, user.Username, user.DisplayName, user.Bio, user.MemberTitle, user.Avatar, user.Links, user.Names,
            user.Pronouns, user.Fields,
            renderMembers ? members.Select(memberRendererService.RenderPartialMember) : null);
    }

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
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        IEnumerable<MemberRendererService.PartialMember>? Members
    );
}