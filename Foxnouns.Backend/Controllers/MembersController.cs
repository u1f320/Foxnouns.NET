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
using Coravel.Queuing.Interfaces;
using EntityFramework.Exceptions.Common;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Jobs;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
    IClock clock
) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<MembersController>();

    [HttpGet]
    [ProducesResponseType<IEnumerable<PartialMember>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembersAsync(string userRef, CancellationToken ct = default)
    {
        User user = await db.ResolveUserAsync(userRef, CurrentToken, ct);
        return Ok(await memberRenderer.RenderUserMembersAsync(user, CurrentToken));
    }

    [HttpGet("{memberRef}")]
    [ProducesResponseType<MemberResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMemberAsync(
        string userRef,
        string memberRef,
        CancellationToken ct = default
    )
    {
        Member member = await db.ResolveMemberAsync(userRef, memberRef, CurrentToken, ct);
        return Ok(memberRenderer.RenderMember(member, CurrentToken));
    }

    public const int MaxMemberCount = 500;

    [HttpPost("/api/v2/users/@me/members")]
    [ProducesResponseType<MemberResponse>(StatusCodes.Status200OK)]
    [Authorize("member.create")]
    public async Task<IActionResult> CreateMemberAsync(
        [FromBody] CreateMemberRequest req,
        CancellationToken ct = default
    )
    {
        ValidationUtils.Validate(
            [
                ("name", ValidationUtils.ValidateMemberName(req.Name)),
                ("display_name", ValidationUtils.ValidateDisplayName(req.DisplayName)),
                ("bio", ValidationUtils.ValidateBio(req.Bio)),
                ("avatar", ValidationUtils.ValidateAvatar(req.Avatar)),
                .. ValidationUtils.ValidateFields(req.Fields, CurrentUser!.CustomPreferences),
                .. ValidationUtils.ValidateFieldEntries(
                    req.Names?.ToArray(),
                    CurrentUser!.CustomPreferences,
                    "names"
                ),
                .. ValidationUtils.ValidatePronouns(
                    req.Pronouns?.ToArray(),
                    CurrentUser!.CustomPreferences
                ),
                .. ValidationUtils.ValidateLinks(req.Links),
            ]
        );

        int memberCount = await db.Members.CountAsync(m => m.UserId == CurrentUser.Id, ct);
        if (memberCount >= MaxMemberCount)
            throw new ApiError.BadRequest("Maximum number of members reached");

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
            Unlisted = req.Unlisted ?? false,
            Sid = null!,
        };
        db.Add(member);

        _logger.Debug(
            "Creating member {MemberName} ({Id}) for {UserId}",
            member.Name,
            member.Id,
            CurrentUser!.Id
        );

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintException)
        {
            _logger.Debug("Could not create member {Id} due to name conflict", member.Id);
            throw new ApiError.BadRequest(
                "A member with that name already exists",
                "name",
                req.Name
            );
        }

        if (req.Avatar != null)
        {
            queue.QueueInvocableWithPayload<MemberAvatarUpdateInvocable, AvatarUpdatePayload>(
                new AvatarUpdatePayload(member.Id, req.Avatar)
            );
        }

        return Ok(memberRenderer.RenderMember(member, CurrentToken));
    }

    [HttpPatch("/api/v2/users/@me/members/{memberRef}")]
    [ProducesResponseType<MemberResponse>(statusCode: StatusCodes.Status200OK)]
    [Authorize("member.update")]
    public async Task<IActionResult> UpdateMemberAsync(
        string memberRef,
        [FromBody] UpdateMemberRequest req
    )
    {
        await using IDbContextTransaction tx = await db.Database.BeginTransactionAsync();
        Member member = await db.ResolveMemberAsync(CurrentUser!.Id, memberRef);
        var errors = new List<(string, ValidationError?)>();

        // We might add extra validations for names later down the line.
        // These should only take effect when a member's name is changed, not on other changes.
        if (req.Name != null && req.Name != member.Name)
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

        if (req.HasProperty(nameof(req.Unlisted)))
            member.Unlisted = req.Unlisted ?? false;

        if (req.Names != null)
        {
            errors.AddRange(
                ValidationUtils.ValidateFieldEntries(
                    req.Names,
                    CurrentUser!.CustomPreferences,
                    "names"
                )
            );
            member.Names = req.Names.ToList();
        }

        if (req.Pronouns != null)
        {
            errors.AddRange(
                ValidationUtils.ValidatePronouns(req.Pronouns, CurrentUser!.CustomPreferences)
            );
            member.Pronouns = req.Pronouns.ToList();
        }

        if (req.Fields != null)
        {
            errors.AddRange(
                ValidationUtils.ValidateFields(req.Fields.ToList(), CurrentUser!.CustomPreferences)
            );
            member.Fields = req.Fields.ToList();
        }

        if (req.Flags != null)
        {
            ValidationError? flagError = await db.SetMemberFlagsAsync(
                CurrentUser!.Id,
                member.Id,
                req.Flags
            );
            if (flagError != null)
                errors.Add(("flags", flagError));
        }

        if (req.HasProperty(nameof(req.Avatar)))
            errors.Add(("avatar", ValidationUtils.ValidateAvatar(req.Avatar)));

        ValidationUtils.Validate(errors);
        // This is fired off regardless of whether the transaction is committed
        // (atomic operations are hard when combined with background jobs)
        // so it's in a separate block to the validation above.
        if (req.HasProperty(nameof(req.Avatar)))
        {
            queue.QueueInvocableWithPayload<MemberAvatarUpdateInvocable, AvatarUpdatePayload>(
                new AvatarUpdatePayload(member.Id, req.Avatar)
            );
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (UniqueConstraintException)
        {
            _logger.Debug(
                "Could not update member {Id} due to name conflict ({CurrentName} / {NewName})",
                member.Id,
                member.Name,
                req.Name
            );
            throw new ApiError.BadRequest(
                "A member with that name already exists",
                "name",
                req.Name
            );
        }

        await tx.CommitAsync();
        return Ok(memberRenderer.RenderMember(member, CurrentToken));
    }

    [HttpDelete("/api/v2/users/@me/members/{memberRef}")]
    [Authorize("member.update")]
    public async Task<IActionResult> DeleteMemberAsync(string memberRef)
    {
        Member member = await db.ResolveMemberAsync(CurrentUser!.Id, memberRef);
        int deleteCount = await db
            .Members.Where(m => m.UserId == CurrentUser!.Id && m.Id == member.Id)
            .ExecuteDeleteAsync();
        if (deleteCount == 0)
        {
            _logger.Warning(
                "Successfully resolved member {Id} but could not delete them",
                member.Id
            );
            return NoContent();
        }

        if (member.Avatar != null)
            await objectStorageService.DeleteMemberAvatarAsync(member.Id, member.Avatar);
        return NoContent();
    }

    [HttpPost("/api/v2/users/@me/members/{memberRef}/reroll-sid")]
    [Authorize("member.update")]
    [ProducesResponseType<MemberResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> RerollSidAsync(string memberRef)
    {
        Member member = await db.ResolveMemberAsync(CurrentUser!.Id, memberRef);

        Instant minTimeAgo = clock.GetCurrentInstant() - Duration.FromHours(1);
        if (CurrentUser!.LastSidReroll > minTimeAgo)
            throw new ApiError.BadRequest("Cannot reroll short ID yet");

        // Using ExecuteUpdateAsync here as the new short ID is generated by the database
        await db
            .Members.Where(m => m.Id == member.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.Sid, _ => db.FindFreeMemberSid()));

        await db
            .Users.Where(u => u.Id == CurrentUser.Id)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(u => u.LastSidReroll, clock.GetCurrentInstant())
                    .SetProperty(u => u.LastActive, clock.GetCurrentInstant())
            );

        // Fetch the new sid then pass that to RenderMember
        string newSid = await db
            .Members.Where(m => m.Id == member.Id)
            .Select(m => m.Sid)
            .FirstAsync();
        return Ok(memberRenderer.RenderMember(member, CurrentToken, newSid));
    }
}
