using System.Diagnostics.CodeAnalysis;
using Coravel.Queuing.Interfaces;
using EntityFramework.Exceptions.Common;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Jobs;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/users")]
public class UsersController(
    DatabaseContext db,
    ILogger logger,
    UserRendererService userRenderer,
    ISnowflakeGenerator snowflakeGenerator,
    IQueue queue,
    IClock clock) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<UsersController>();

    [HttpGet("{userRef}")]
    [ProducesResponseType<UserRendererService.UserResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserAsync(string userRef, CancellationToken ct = default)
    {
        var user = await db.ResolveUserAsync(userRef, CurrentToken, ct);
        return Ok(await userRenderer.RenderUserAsync(
            user,
            selfUser: CurrentUser,
            token: CurrentToken,
            renderMembers: true,
            renderAuthMethods: true,
            ct: ct
        ));
    }

    [HttpPatch("@me")]
    [Authorize("user.update")]
    [ProducesResponseType<UserRendererService.UserResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserRequest req, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var user = await db.Users.FirstAsync(u => u.Id == CurrentUser!.Id, ct);
        var errors = new List<(string, ValidationError?)>();

        if (req.Username != null && req.Username != user.Username)
        {
            errors.Add(("username", ValidationUtils.ValidateUsername(req.Username)));
            user.Username = req.Username;
        }

        if (req.HasProperty(nameof(req.DisplayName)))
        {
            errors.Add(("display_name", ValidationUtils.ValidateDisplayName(req.DisplayName)));
            user.DisplayName = req.DisplayName;
        }

        if (req.HasProperty(nameof(req.Bio)))
        {
            errors.Add(("bio", ValidationUtils.ValidateBio(req.Bio)));
            user.Bio = req.Bio;
        }

        if (req.HasProperty(nameof(req.Links)))
        {
            errors.AddRange(ValidationUtils.ValidateLinks(req.Links));
            user.Links = req.Links ?? [];
        }

        if (req.Names != null)
        {
            errors.AddRange(ValidationUtils.ValidateFieldEntries(req.Names, CurrentUser!.CustomPreferences, "names"));
            user.Names = req.Names.ToList();
        }

        if (req.Pronouns != null)
        {
            errors.AddRange(ValidationUtils.ValidatePronouns(req.Pronouns, CurrentUser!.CustomPreferences));
            user.Pronouns = req.Pronouns.ToList();
        }

        if (req.Fields != null)
        {
            errors.AddRange(ValidationUtils.ValidateFields(req.Fields.ToList(), CurrentUser!.CustomPreferences));
            user.Fields = req.Fields.ToList();
        }

        if (req.Flags != null)
        {
            var flagError = await db.SetUserFlagsAsync(CurrentUser!.Id, req.Flags);
            if (flagError != null) errors.Add(("flags", flagError));
        }

        if (req.HasProperty(nameof(req.Avatar)))
            errors.Add(("avatar", ValidationUtils.ValidateAvatar(req.Avatar)));

        ValidationUtils.Validate(errors);
        // This is fired off regardless of whether the transaction is committed
        // (atomic operations are hard when combined with background jobs)
        // so it's in a separate block to the validation above.
        if (req.HasProperty(nameof(req.Avatar)))
            queue.QueueInvocableWithPayload<UserAvatarUpdateInvocable, AvatarUpdatePayload>(
                new AvatarUpdatePayload(CurrentUser!.Id, req.Avatar));

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintException)
        {
            _logger.Debug("Could not update user {Id} due to name conflict ({CurrentName} / {NewName})", user.Id,
                user.Username, req.Username);
            throw new ApiError.BadRequest("That username is already taken.", "username", req.Username!);
        }

        await tx.CommitAsync(ct);
        return Ok(await userRenderer.RenderUserAsync(user, CurrentUser, renderMembers: false,
            renderAuthMethods: false, ct: ct));
    }

    [HttpPatch("@me/custom-preferences")]
    [Authorize("user.update")]
    [ProducesResponseType<Dictionary<Snowflake, User.CustomPreference>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCustomPreferencesAsync([FromBody] List<CustomPreferencesUpdateRequest> req,
        CancellationToken ct = default)
    {
        ValidationUtils.Validate(ValidateCustomPreferences(req));

        var user = await db.ResolveUserAsync(CurrentUser!.Id, ct);
        var preferences = user.CustomPreferences.Where(x => req.Any(r => r.Id == x.Key)).ToDictionary();

        foreach (var r in req)
        {
            if (r.Id != null && preferences.ContainsKey(r.Id.Value))
            {
                preferences[r.Id.Value] = new User.CustomPreference
                {
                    Favourite = r.Favourite,
                    Icon = r.Icon,
                    Muted = r.Muted,
                    Size = r.Size,
                    Tooltip = r.Tooltip
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
                    Tooltip = r.Tooltip
                };
            }
        }

        user.CustomPreferences = preferences;
        await db.SaveChangesAsync(ct);

        return Ok(user.CustomPreferences);
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class CustomPreferencesUpdateRequest
    {
        public Snowflake? Id { get; init; }
        public required string Icon { get; set; }
        public required string Tooltip { get; set; }
        public PreferenceSize Size { get; set; }
        public bool Muted { get; set; }
        public bool Favourite { get; set; }
    }

    public const int MaxCustomPreferences = 25;

    private static List<(string, ValidationError?)> ValidateCustomPreferences(
        List<CustomPreferencesUpdateRequest> preferences)
    {
        var errors = new List<(string, ValidationError?)>();

        if (preferences.Count > MaxCustomPreferences)
            errors.Add(("custom_preferences",
                ValidationError.LengthError("Too many custom preferences", 0, MaxCustomPreferences, preferences.Count)));
        if (preferences.Count > 50) return errors;

        // TODO: validate individual preferences

        return errors;
    }

    public class UpdateUserRequest : PatchRequest
    {
        public string? Username { get; init; }
        public string? DisplayName { get; init; }
        public string? Bio { get; init; }
        public string? Avatar { get; init; }
        public string[]? Links { get; init; }
        public FieldEntry[]? Names { get; init; }
        public Pronoun[]? Pronouns { get; init; }
        public Field[]? Fields { get; init; }
        public Snowflake[]? Flags { get; init; }
    }


    [HttpGet("@me/settings")]
    [Authorize("user.read_hidden")]
    [ProducesResponseType<UserSettings>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserSettingsAsync(CancellationToken ct = default)
    {
        var user = await db.Users.FirstAsync(u => u.Id == CurrentUser!.Id, ct);
        return Ok(user.Settings);
    }

    [HttpPatch("@me/settings")]
    [Authorize("user.read_hidden", "user.update")]
    [ProducesResponseType<UserSettings>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUserSettingsAsync([FromBody] UpdateUserSettingsRequest req,
        CancellationToken ct = default)
    {
        var user = await db.Users.FirstAsync(u => u.Id == CurrentUser!.Id, ct);

        if (req.HasProperty(nameof(req.DarkMode)))
            user.Settings.DarkMode = req.DarkMode;

        db.Update(user);
        await db.SaveChangesAsync(ct);

        return Ok(user.Settings);
    }

    public class UpdateUserSettingsRequest : PatchRequest
    {
        public bool? DarkMode { get; init; }
    }

    [HttpPost("@me/reroll-sid")]
    [Authorize("user.update")]
    [ProducesResponseType<UserRendererService.UserResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> RerollSidAsync()
    {
        var minTimeAgo = clock.GetCurrentInstant() - Duration.FromHours(1);
        if (CurrentUser!.LastSidReroll > minTimeAgo)
            throw new ApiError.BadRequest("Cannot reroll short ID yet");

        // Using ExecuteUpdateAsync here as the new short ID is generated by the database
        await db.Users.Where(u => u.Id == CurrentUser.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.Sid, _ => db.FindFreeUserSid())
                .SetProperty(u => u.LastSidReroll, clock.GetCurrentInstant())
                .SetProperty(u => u.LastActive, clock.GetCurrentInstant()));

        var user = await db.ResolveUserAsync(CurrentUser.Id);
        return Ok(await userRenderer.RenderUserAsync(user, CurrentUser, CurrentToken, renderMembers: false));
    }
}