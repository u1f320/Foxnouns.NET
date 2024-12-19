using Foxnouns.Backend.Database;
using Foxnouns.Backend.Middleware;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Foxnouns.Backend.Controllers;

[Route("/api/internal/self-delete")]
[Authorize("*")]
[ApiExplorerSettings(IgnoreApi = true)]
public class DeleteUserController(DatabaseContext db, IClock clock, ILogger logger)
    : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<DeleteUserController>();

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteSelfAsync()
    {
        _logger.Information(
            "User {UserId} has requested their account to be deleted",
            CurrentUser!.Id
        );

        CurrentUser.Deleted = true;
        CurrentUser.DeletedAt = clock.GetCurrentInstant();

        db.Update(CurrentUser);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("force")]
    [Limit(UsableByDeletedUsers = true)]
    public async Task<IActionResult> ForceDeleteAsync()
    {
        if (!CurrentUser!.Deleted)
            throw new ApiError.BadRequest("Your account isn't deleted.");

        _logger.Information(
            "User {UserId} has requested an early full delete of their account",
            CurrentUser.Id
        );

        // This is the easiest way to force delete a user, don't judge me
        CurrentUser.DeletedAt = clock.GetCurrentInstant() - Duration.FromDays(365);
        db.Update(CurrentUser);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("undelete")]
    [Limit(UsableByDeletedUsers = true)]
    public async Task<IActionResult> UndeleteSelfAsync()
    {
        if (!CurrentUser!.Deleted)
            throw new ApiError.BadRequest("Your account isn't deleted.");
        if (CurrentUser!.DeletedBy != null)
        {
            throw new ApiError.BadRequest(
                "Your account has been suspended and can't be reactivated by yourself."
            );
        }

        _logger.Information(
            "User {UserId} has requested to undelete their account",
            CurrentUser.Id
        );

        CurrentUser.Deleted = false;
        CurrentUser.DeletedAt = null;
        db.Update(CurrentUser);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
