using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto.V1;
using Microsoft.EntityFrameworkCore;
using FieldEntry = Foxnouns.Backend.Dto.V1.FieldEntry;
using PrideFlag = Foxnouns.Backend.Dto.V1.PrideFlag;

namespace Foxnouns.Backend.Services.V1;

public class MembersV1Service(DatabaseContext db)
{
    public async Task<Member> ResolveMemberAsync(string id, CancellationToken ct = default)
    {
        Member? member;
        if (Snowflake.TryParse(id, out Snowflake? sf))
        {
            member = await db
                .Members.Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == sf && !m.User.Deleted, ct);
            if (member != null)
                return member;
        }

        member = await db
            .Members.Include(m => m.User)
            .FirstOrDefaultAsync(m => m.LegacyId == id && !m.User.Deleted, ct);
        if (member != null)
            return member;

        throw new ApiError.NotFound("No member with that ID found.", ErrorCode.MemberNotFound);
    }

    public async Task<MemberResponse> RenderMemberAsync(
        Member m,
        Token? token = default,
        bool renderFlags = true,
        CancellationToken ct = default
    )
    {
        bool renderUnlisted = m.UserId == token?.UserId;

        List<MemberFlag> flags = renderFlags
            ? await db.MemberFlags.Where(f => f.MemberId == m.Id).OrderBy(f => f.Id).ToListAsync(ct)
            : [];

        return new MemberResponse(
            m.LegacyId,
            m.Id,
            m.Sid,
            m.Name,
            m.DisplayName,
            m.Bio,
            m.Avatar,
            m.Links,
            Names: FieldEntry.FromEntries(m.Names, m.User.CustomPreferences),
            Pronouns: PronounEntry.FromPronouns(m.Pronouns, m.User.CustomPreferences),
            Fields: ProfileField.FromFields(m.Fields, m.User.CustomPreferences),
            Flags: flags
                .Where(f => f.PrideFlag.Hash != null)
                .Select(f => new PrideFlag(
                    f.PrideFlag.LegacyId,
                    f.PrideFlag.Id,
                    f.PrideFlag.Hash!,
                    f.PrideFlag.Name,
                    f.PrideFlag.Description
                ))
                .ToArray(),
            User: UsersV1Service.RenderPartialUser(m.User),
            Unlisted: renderUnlisted ? m.Unlisted : null
        );
    }
}
