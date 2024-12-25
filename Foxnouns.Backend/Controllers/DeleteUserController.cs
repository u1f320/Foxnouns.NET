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
