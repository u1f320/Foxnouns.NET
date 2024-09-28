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
using NodaTime;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/users/{userRef}/members")]
public class MembersController(
    ILogger logger,
    DatabaseContext db,
    MemberRendererService memberRenderer,
    ISnowflakeGenerator snowflakeGenerator,
    ObjectStorageService objectStorageService,
    IQueue queue,
    IClock clock) : ApiControllerBase
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
    public async Task<IActionResult> CreateMemberAsync([FromBody] CreateMemberRequest req,
        CancellationToken ct = default)
    {
        ValidationUtils.Validate([
            ("name", ValidationUtils.ValidateMemberName(req.Name)),
            ("display_name", ValidationUtils.ValidateDisplayName(req.DisplayName)),
            ("bio", ValidationUtils.ValidateBio(req.Bio)),
            ("avatar", ValidationUtils.ValidateAvatar(req.Avatar)),
            .. ValidationUtils.ValidateFields(req.Fields, CurrentUser!.CustomPreferences),
            .. ValidationUtils.ValidateFieldEntries(req.Names?.ToArray(), CurrentUser!.CustomPreferences, "names"),
            .. ValidationUtils.ValidatePronouns(req.Pronouns?.ToArray(), CurrentUser!.CustomPreferences),
            .. ValidationUtils.ValidateLinks(req.Links)
        ]);

        var member = new Member
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            User = CurrentUser!,
            Name = req.Name,
            DisplayName = req.DisplayName,
            Bio = req.Bio,
            Links = req.Links ?? [],
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

    [HttpPatch("/api/v2/users/@me/members/{memberRef}")]
    [Authorize("member.update")]
    public async Task<IActionResult> UpdateMemberAsync(string memberRef, [FromBody] UpdateMemberRequest req)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var member = await db.ResolveMemberAsync(CurrentUser!.Id, memberRef);
        var errors = new List<(string, ValidationError?)>();

        if (req.Name != null)
        {
            errors.Add(("name", ValidationUtils.ValidateMemberName(req.Name)));
            member.Name = req.Name;
        }

        if (req.HasProperty(nameof(req.DisplayName)))
        {
            errors.Add(("display_name", ValidationUtils.ValidateDisplayName(req.DisplayName)));
            member.DisplayName = req.DisplayName;
        }

        if (req.HasProperty(nameof(req.Bio)))
        {
            errors.Add(("bio", ValidationUtils.ValidateBio(req.Bio)));
            member.Bio = req.Bio;
        }

        if (req.HasProperty(nameof(req.Links)))
        {
            errors.AddRange(ValidationUtils.ValidateLinks(req.Links));
            member.Links = req.Links ?? [];
        }

        if (req.Names != null)
        {
            errors.AddRange(ValidationUtils.ValidateFieldEntries(req.Names, CurrentUser!.CustomPreferences, "names"));
            member.Names = req.Names.ToList();
        }

        if (req.Pronouns != null)
        {
            errors.AddRange(ValidationUtils.ValidatePronouns(req.Pronouns, CurrentUser!.CustomPreferences));
            member.Pronouns = req.Pronouns.ToList();
        }

        if (req.Fields != null)
        {
            errors.AddRange(ValidationUtils.ValidateFields(req.Fields.ToList(), CurrentUser!.CustomPreferences));
            member.Fields = req.Fields.ToList();
        }

        if (req.Flags != null)
        {
            var flagError = await db.SetMemberFlagsAsync(CurrentUser!.Id, member.Id, req.Flags);
            if (flagError != null) errors.Add(("flags", flagError));
        }

        if (req.HasProperty(nameof(req.Avatar)))
            errors.Add(("avatar", ValidationUtils.ValidateAvatar(req.Avatar)));

        ValidationUtils.Validate(errors);
        // This is fired off regardless of whether the transaction is committed
        // (atomic operations are hard when combined with background jobs)
        // so it's in a separate block to the validation above.
        if (req.HasProperty(nameof(req.Avatar)))
            queue.QueueInvocableWithPayload<MemberAvatarUpdateInvocable, AvatarUpdatePayload>(
                new AvatarUpdatePayload(member.Id, req.Avatar));
        try
        {
            await db.SaveChangesAsync();
        }
        catch (UniqueConstraintException)
        {
            _logger.Debug("Could not update member {Id} due to name conflict ({CurrentName} / {NewName})", member.Id,
                member.Name, req.Name);
            throw new ApiError.BadRequest("A member with that name already exists", "name", req.Name!);
        }

        await tx.CommitAsync();
        return Ok(memberRenderer.RenderMember(member, CurrentToken));
    }

    public class UpdateMemberRequest : PatchRequest
    {
        public string? Name { get; init; }
        public string? DisplayName { get; init; }
        public string? Bio { get; init; }
        public string? Avatar { get; init; }
        public string[]? Links { get; init; }
        public FieldEntry[]? Names { get; init; }
        public Pronoun[]? Pronouns { get; init; }
        public Field[]? Fields { get; init; }
        public Snowflake[]? Flags { get; init; }
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
        string[]? Links,
        List<FieldEntry>? Names,
        List<Pronoun>? Pronouns,
        List<Field>? Fields);

    [HttpPost("/api/v2/users/@me/members/{memberRef}/reroll-sid")]
    [Authorize("member.update")]
    [ProducesResponseType<UserRendererService.UserResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> RerollSidAsync(string memberRef)
    {
        var member = await db.ResolveMemberAsync(CurrentUser!.Id, memberRef);

        var minTimeAgo = clock.GetCurrentInstant() - Duration.FromHours(1);
        if (CurrentUser!.LastSidReroll > minTimeAgo)
            throw new ApiError.BadRequest("Cannot reroll short ID yet");

        // Using ExecuteUpdateAsync here as the new short ID is generated by the database
        await db.Members.Where(m => m.Id == member.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Sid, _ => db.FindFreeMemberSid()));

        await db.Users.Where(u => u.Id == CurrentUser.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.LastSidReroll, clock.GetCurrentInstant())
                .SetProperty(u => u.LastActive, clock.GetCurrentInstant()));

        // Re-fetch member to fetch the new sid
        var updatedMember = await db.ResolveMemberAsync(CurrentUser!.Id, memberRef);
        return Ok(memberRenderer.RenderMember(updatedMember, CurrentToken));
    }
}