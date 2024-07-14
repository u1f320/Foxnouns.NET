using EntityFramework.Exceptions.Common;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
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
    MemberRendererService memberRendererService,
    ISnowflakeGenerator snowflakeGenerator,
    AvatarUpdateJob avatarUpdate) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<MembersController>();

    [HttpGet]
    [ProducesResponseType<IEnumerable<MemberRendererService.PartialMember>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembersAsync(string userRef)
    {
        var user = await db.ResolveUserAsync(userRef, CurrentToken);
        return Ok(await memberRendererService.RenderUserMembersAsync(user, CurrentToken));
    }

    [HttpGet("{memberRef}")]
    [ProducesResponseType<MemberRendererService.MemberResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMemberAsync(string userRef, string memberRef)
    {
        var member = await db.ResolveMemberAsync(userRef, memberRef, CurrentToken);
        return Ok(memberRendererService.RenderMember(member, CurrentToken));
    }

    [HttpPost("/api/v2/users/@me/members")]
    [ProducesResponseType<MemberRendererService.MemberResponse>(StatusCodes.Status200OK)]
    [Authorize("member.create")]
    public async Task<IActionResult> CreateMemberAsync([FromBody] CreateMemberRequest req)
    {
        ValidationUtils.Validate([
            ("name", ValidationUtils.ValidateMemberName(req.Name)),
            ("display_name", ValidationUtils.ValidateDisplayName(req.DisplayName)),
            ("bio", ValidationUtils.ValidateBio(req.Bio)),
            ("avatar", ValidationUtils.ValidateAvatar(req.Avatar))
        ]);

        var member = new Member
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            User = CurrentUser!,
            Name = req.Name,
            DisplayName = req.DisplayName,
            Bio = req.Bio,
            Unlisted = req.Unlisted ?? false
        };
        db.Add(member);

        _logger.Debug("Creating member {MemberName} ({Id}) for {UserId}", member.Name, member.Id, CurrentUser!.Id);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (UniqueConstraintException)
        {
            _logger.Debug("Could not create member {Id} due to name conflict", member.Id);
            throw new ApiError.BadRequest("A member with that name already exists", "name", req.Name);
        }

        if (req.Avatar != null) AvatarUpdateJob.QueueUpdateMemberAvatar(member.Id, req.Avatar);

        return Ok(memberRendererService.RenderMember(member, CurrentToken));
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

        await db.SaveChangesAsync();

        if (member.Avatar != null) await avatarUpdate.DeleteMemberAvatar(member.Id, member.Avatar);
        return NoContent();
    }

    public record CreateMemberRequest(string Name, string? DisplayName, string? Bio, string? Avatar, bool? Unlisted);
}