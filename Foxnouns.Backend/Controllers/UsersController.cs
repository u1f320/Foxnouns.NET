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
        var user = await db.ResolveUserAsync(userRef);
        return await GetUserInnerAsync(user);
    }

    [HttpGet("@me")]
    [Authorize("identify")]
    [ProducesResponseType<UserRendererService.UserResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMeAsync()
    {
        var user = await db.ResolveUserAsync(CurrentUser!.Id);
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

        if (req.Username != null && req.Username != user.Username)
        {
            ValidationUtils.ValidateUsername(req.Username);
            user.Username = req.Username;
        }

        if (req.HasProperty(nameof(req.DisplayName)))
        {
            ValidationUtils.ValidateDisplayName(req.DisplayName);
            user.DisplayName = req.DisplayName;
        }

        if (req.HasProperty(nameof(req.Bio)))
        {
            ValidationUtils.ValidateBio(req.Bio);
            user.Bio = req.Bio;
        }

        if (req.HasProperty(nameof(req.Avatar)))
        {
            ValidationUtils.ValidateAvatar(req.Avatar);
            AvatarUpdateJob.QueueUpdateUserAvatar(CurrentUser!.Id, req.Avatar);
        }

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