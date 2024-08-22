using System.Diagnostics.CodeAnalysis;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Jobs;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/users")]
public class UsersController(
    DatabaseContext db,
    UserRendererService userRendererService,
    ISnowflakeGenerator snowflakeGenerator) : ApiControllerBase
{
    [HttpGet("{userRef}")]
    [ProducesResponseType<UserRendererService.UserResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserAsync(string userRef)
    {
        var user = await db.ResolveUserAsync(userRef, CurrentToken);
        return await GetUserInnerAsync(user);
    }

    private async Task<IActionResult> GetUserInnerAsync(User user)
    {
        return Ok(await userRendererService.RenderUserAsync(
            user,
            selfUser: CurrentUser,
            token: CurrentToken,
            renderMembers: true,
            renderAuthMethods: true
        ));
    }

    [HttpPatch("@me")]
    [Authorize("user.update")]
    [ProducesResponseType<UserRendererService.UserResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserRequest req)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var user = await db.Users.FirstAsync(u => u.Id == CurrentUser!.Id);
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

        if (req.HasProperty(nameof(req.Avatar)))
            errors.Add(("avatar", ValidationUtils.ValidateAvatar(req.Avatar)));

        ValidationUtils.Validate(errors);
        // This is fired off regardless of whether the transaction is committed
        // (atomic operations are hard when combined with background jobs)
        // so it's in a separate block to the validation above.
        if (req.HasProperty(nameof(req.Avatar)))
            AvatarUpdateJob.QueueUpdateUserAvatar(CurrentUser!.Id, req.Avatar);

        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(await userRendererService.RenderUserAsync(user, CurrentUser, renderMembers: false,
            renderAuthMethods: false));
    }

    [HttpPatch("@me/custom-preferences")]
    [Authorize("user.update")]
    [ProducesResponseType<Dictionary<Snowflake, User.CustomPreference>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCustomPreferencesAsync([FromBody] List<CustomPreferencesUpdateRequest> req)
    {
        ValidationUtils.Validate(ValidateCustomPreferences(req));

        var user = await db.ResolveUserAsync(CurrentUser!.Id);
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
        await db.SaveChangesAsync();

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

    private static List<(string, ValidationError?)> ValidateCustomPreferences(
        List<CustomPreferencesUpdateRequest> preferences)
    {
        var errors = new List<(string, ValidationError?)>();

        if (preferences.Count > 25)
            errors.Add(("custom_preferences",
                ValidationError.LengthError("Too many custom preferences", 0, 25, preferences.Count)));
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
    }
}