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
public class UsersController(DatabaseContext db, UserRendererService userRendererService) : ApiControllerBase
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

    public class UpdateUserRequest : PatchRequest
    {
        public string? Username { get; init; }
        public string? DisplayName { get; init; }
        public string? Bio { get; init; }
        public string? Avatar { get; init; }
    }
}