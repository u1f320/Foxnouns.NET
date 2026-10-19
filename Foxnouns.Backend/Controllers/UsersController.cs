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
using EntityFramework.Exceptions.Common;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Jobs;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NodaTime;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/users")]
public class UsersController(
    DatabaseContext db,
    ILogger logger,
    UserRendererService userRenderer,
    ISnowflakeGenerator snowflakeGenerator,
    IClock clock,
    ValidationService validationService
) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<UsersController>();

    [HttpGet("{userRef}")]
    [ProducesResponseType<UserResponse>(statusCode: StatusCodes.Status200OK)]
    [Limit(UsableByDeletedUsers = true)]
    public async Task<IActionResult> GetUserAsync(string userRef, CancellationToken ct = default)
    {
        User user = await db.ResolveUserAsync(userRef, CurrentToken, ct);
        return Ok(
            await userRenderer.RenderUserAsync(
                user,
                CurrentUser,
                CurrentToken,
                renderMembers: true,
                renderAuthMethods: true,
                renderSettings: true,
                ct: ct
            )
        );
    }

    [HttpPatch("@me")]
    [Authorize("user.update")]
    [ProducesResponseType<UserResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUserAsync(
        [FromBody] UpdateUserRequest req,
        CancellationToken ct = default
    )
    {
        await using IDbContextTransaction tx = await db.Database.BeginTransactionAsync(ct);
        User user = await db.Users.FirstAsync(u => u.Id == CurrentUser!.Id, ct);
        var errors = new List<(string, ValidationError?)>();

        if (req.Username != null && req.Username != user.Username)
        {
            errors.Add(("username", validationService.ValidateUsername(req.Username)));
            user.Username = req.Username;
        }

        if (req.HasProperty(nameof(req.DisplayName)))
        {
            errors.Add(("display_name", validationService.ValidateDisplayName(req.DisplayName)));
            user.DisplayName = req.DisplayName;
        }

        if (req.HasProperty(nameof(req.Bio)))
        {
            errors.Add(("bio", validationService.ValidateBio(req.Bio)));
            user.Bio = req.Bio;
        }

        if (req.HasProperty(nameof(req.Links)))
        {
            errors.AddRange(validationService.ValidateLinks(req.Links));
            user.Links = req.Links ?? [];
        }

        if (req.Names != null)
        {
            errors.AddRange(
                validationService.ValidateFieldEntries(
                    req.Names,
                    CurrentUser!.CustomPreferences,
                    "names"
                )
            );
            user.Names = req.Names.ToList();
        }

        if (req.Pronouns != null)
        {
            errors.AddRange(
                validationService.ValidatePronouns(req.Pronouns, CurrentUser!.CustomPreferences)
            );
            user.Pronouns = req.Pronouns.ToList();
        }

        if (req.Fields != null)
        {
            errors.AddRange(
                validationService.ValidateFields(
                    req.Fields.ToList(),
                    CurrentUser!.CustomPreferences
                )
            );
            user.Fields = req.Fields.ToList();
        }

        if (req.Flags != null)
        {
            ValidationError? flagError = await db.SetUserFlagsAsync(CurrentUser!.Id, req.Flags);
            if (flagError != null)
                errors.Add(("flags", flagError));
        }

        if (req.HasProperty(nameof(req.Avatar)))
            errors.Add(("avatar", validationService.ValidateAvatar(req.Avatar)));

        if (req.HasProperty(nameof(req.MemberTitle)))
        {
            if (string.IsNullOrEmpty(req.MemberTitle))
            {
                user.MemberTitle = null;
            }
            else
            {
                errors.Add(
                    ("member_title", validationService.ValidateDisplayName(req.MemberTitle))
                );
                user.MemberTitle = req.MemberTitle;
            }
        }

        if (req.HasProperty(nameof(req.MemberListHidden)))
            user.ListHidden = req.MemberListHidden == true;

        if (req.HasProperty(nameof(req.Timezone)))
        {
            if (string.IsNullOrEmpty(req.Timezone))
            {
                user.Timezone = null;
            }
            else
            {
                if (TimeZoneInfo.TryFindSystemTimeZoneById(req.Timezone, out _))
                {
                    user.Timezone = req.Timezone;
                }
                else
                {
                    errors.Add(
                        (
                            "timezone",
                            ValidationError.GenericValidationError("Invalid timezone", req.Timezone)
                        )
                    );
                }
            }
        }

        ValidationUtils.Validate(errors);
        // This is fired off regardless of whether the transaction is committed
        // (atomic operations are hard when combined with background jobs)
        // so it's in a separate block to the validation above.
        if (req.HasProperty(nameof(req.Avatar)))
        {
            UserAvatarUpdateJob.Enqueue(new AvatarUpdatePayload(CurrentUser!.Id, req.Avatar));
        }

        user.LastActive = clock.GetCurrentInstant();

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintException)
        {
            _logger.Debug(
                "Could not update user {Id} due to name conflict ({CurrentName} / {NewName})",
                user.Id,
                user.Username,
                req.Username
            );
            throw new ApiError.BadRequest(
                "That username is already taken.",
                "username",
                req.Username
            );
        }

        await tx.CommitAsync(ct);
        return Ok(
            await userRenderer.RenderUserAsync(
                user,
                CurrentUser,
                renderMembers: false,
                renderAuthMethods: false,
                ct: ct
            )
        );
    }

    [HttpPatch("@me/custom-preferences")]
    [Authorize("user.update")]
    [ProducesResponseType<Dictionary<Snowflake, User.CustomPreference>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCustomPreferencesAsync(
        [FromBody] List<CustomPreferenceUpdateRequest> req,
        CancellationToken ct = default
    )
    {
        ValidationUtils.Validate(ValidationUtils.ValidateCustomPreferences(req));

        User user = await db.ResolveUserAsync(CurrentUser!.Id, ct);
        var preferences = user
            .CustomPreferences.Where(x => req.Any(r => r.Id == x.Key))
            .ToDictionary();

        foreach (CustomPreferenceUpdateRequest r in req)
        {
            if (
                r.Id != null
                && preferences.TryGetValue(r.Id.Value, out User.CustomPreference? value)
            )
            {
                preferences[r.Id.Value] = new User.CustomPreference
                {
                    Favourite = r.Favourite,
                    Icon = r.Icon,
                    Muted = r.Muted,
                    Size = r.Size,
                    Tooltip = r.Tooltip,
                    LegacyId = value.LegacyId,
                };
            }
            else
            {
                preferences[snowflakeGenerator.GenerateSnowflake()] = new User.CustomPreference
                {
                    Favourite = r.Favourite,
                    Icon = r.Icon,
                    Muted = r.Muted,
                    Size = r.Size,
                    Tooltip = r.Tooltip,
                    LegacyId = Guid.NewGuid(),
                };
            }
        }

        user.CustomPreferences = preferences;
        user.LastActive = clock.GetCurrentInstant();
        await db.SaveChangesAsync(ct);

        return Ok(user.CustomPreferences);
    }

    [HttpPatch("@me/settings")]
    [Authorize("user.read_hidden", "user.update")]
    [ProducesResponseType<UserSettings>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUserSettingsAsync(
        [FromBody] UpdateUserSettingsRequest req,
        CancellationToken ct = default
    )
    {
        User user = await db.Users.FirstAsync(u => u.Id == CurrentUser!.Id, ct);

        if (req.HasProperty(nameof(req.DarkMode)))
            user.Settings.DarkMode = req.DarkMode;
        if (req.HasProperty(nameof(req.LastReadNotice)))
            user.Settings.LastReadNotice = req.LastReadNotice;

        user.LastActive = clock.GetCurrentInstant();
        db.Update(user);
        await db.SaveChangesAsync(ct);

        return Ok(user.Settings);
    }

    [HttpPost("@me/reroll-sid")]
    [Authorize("user.update")]
    [ProducesResponseType<UserResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> RerollSidAsync()
    {
        Instant minTimeAgo = clock.GetCurrentInstant() - Duration.FromHours(1);
        if (CurrentUser!.LastSidReroll > minTimeAgo)
            throw new ApiError.BadRequest("Cannot reroll short ID yet");

        // Using ExecuteUpdateAsync here as the new short ID is generated by the database
        await db
            .Users.Where(u => u.Id == CurrentUser.Id)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(u => u.Sid, _ => db.FindFreeUserSid())
                    .SetProperty(u => u.LastSidReroll, clock.GetCurrentInstant())
                    .SetProperty(u => u.LastActive, clock.GetCurrentInstant())
            );

        // Get the user's new sid
        string newSid = await db
            .Users.Where(u => u.Id == CurrentUser.Id)
            .Select(u => u.Sid)
            .FirstAsync();

        User user = await db.ResolveUserAsync(CurrentUser.Id);
        return Ok(
            await userRenderer.RenderUserAsync(
                user,
                CurrentUser,
                CurrentToken,
                false,
                overrideSid: newSid
            )
        );
    }
}
