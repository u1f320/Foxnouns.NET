using Coravel.Queuing.Interfaces;
using EntityFramework.Exceptions.Common;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Jobs;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/users/{userRef}/members")]
public class MembersController(
    ILogger logger,
    DatabaseContext db,
    MemberRendererService memberRenderer,
    ISnowflakeGenerator snowflakeGenerator,
    ObjectStorageService objectStorageService,
    IQueue queue) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<MembersController>();

    [HttpGet]
    [ProducesResponseType<IEnumerable<MemberRendererService.PartialMember>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembersAsync(string userRef, CancellationToken ct = default)
    {
        var user = await db.ResolveUserAsync(userRef, CurrentToken, ct);
        return Ok(await memberRenderer.RenderUserMembersAsync(user, CurrentToken));
    }

    [HttpGet("{memberRef}")]
    [ProducesResponseType<MemberRendererService.MemberResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMemberAsync(string userRef, string memberRef, CancellationToken ct = default)
    {
        var member = await db.ResolveMemberAsync(userRef, memberRef, CurrentToken, ct);
        return Ok(memberRenderer.RenderMember(member, CurrentToken));
    }

    [HttpPost("/api/v2/users/@me/members")]
    [ProducesResponseType<MemberRendererService.MemberResponse>(StatusCodes.Status200OK)]
    [Authorize("member.create")]
    public async Task<IActionResult> CreateMemberAsync([FromBody] CreateMemberRequest req, CancellationToken ct = default)
    {
        ValidationUtils.Validate([
            ("name", ValidationUtils.ValidateMemberName(req.Name)),
            ("display_name", ValidationUtils.ValidateDisplayName(req.DisplayName)),
            ("bio", ValidationUtils.ValidateBio(req.Bio)),
            ("avatar", ValidationUtils.ValidateAvatar(req.Avatar)),
            ..ValidationUtils.ValidateFields(req.Fields, CurrentUser!.CustomPreferences),
            ..ValidationUtils.ValidateFieldEntries(req.Names?.ToArray(), CurrentUser!.CustomPreferences, "names"),
            ..ValidationUtils.ValidatePronouns(req.Pronouns?.ToArray(), CurrentUser!.CustomPreferences)
        ]);

        var member = new Member
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            User = CurrentUser!,
            Name = req.Name,
            DisplayName = req.DisplayName,
            Bio = req.Bio,
            Fields = req.Fields ?? [],
            Names = req.Names ?? [],
            Pronouns = req.Pronouns ?? [],
            Unlisted = req.Unlisted ?? false
        };
        db.Add(member);

        _logger.Debug("Creating member {MemberName} ({Id}) for {UserId}", member.Name, member.Id, CurrentUser!.Id);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintException)
        {
            _logger.Debug("Could not create member {Id} due to name conflict", member.Id);
            throw new ApiError.BadRequest("A member with that name already exists", "name", req.Name);
        }

        if (req.Avatar != null)
            queue.QueueInvocableWithPayload<MemberAvatarUpdateInvocable, AvatarUpdatePayload>(
                new AvatarUpdatePayload(member.Id, req.Avatar));

        return Ok(memberRenderer.RenderMember(member, CurrentToken));
    }

    [HttpDelete("/api/v2/users/@me/members/{memberRef}")]
    [Authorize("member.update")]
    public async Task<IActionResult> DeleteMemberAsync(string memberRef)
    {
        var member = await db.ResolveMemberAsync(CurrentUser!.Id, memberRef);
        var deleteCount = await db.Members.Where(m => m.UserId == CurrentUser!.Id && m.Id == member.Id)
            .ExecuteDeleteAsync();
        if (deleteCount == 0)
        {
            _logger.Warning("Successfully resolved member {Id} but could not delete them", member.Id);
            return NoContent();
        }

        if (member.Avatar != null) await objectStorageService.DeleteMemberAvatarAsync(member.Id, member.Avatar);
        return NoContent();
    }

    public record CreateMemberRequest(
        string Name,
        string? DisplayName,
        string? Bio,
        string? Avatar,
        bool? Unlisted,
        List<FieldEntry>? Names,
        List<Pronoun>? Pronouns,
        List<Field>? Fields);
}