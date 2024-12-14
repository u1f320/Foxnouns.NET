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
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;

namespace Foxnouns.Backend.Middleware;

public class AuthorizationMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        Endpoint? endpoint = ctx.GetEndpoint();
        AuthorizeAttribute? authorizeAttribute =
            endpoint?.Metadata.GetMetadata<AuthorizeAttribute>();
        LimitAttribute? limitAttribute = endpoint?.Metadata.GetMetadata<LimitAttribute>();

        if (authorizeAttribute == null || authorizeAttribute.Scopes.Length == 0)
        {
            await next(ctx);
            return;
        }

        Token? token = ctx.GetToken();
        if (token == null)
        {
            throw new ApiError.Unauthorized(
                "This endpoint requires an authenticated user.",
                ErrorCode.AuthenticationRequired
            );
        }

        // Users who got suspended by a moderator can still access *some* endpoints.
        if (
            token.User.Deleted
            && (limitAttribute?.UsableBySuspendedUsers != true || token.User.DeletedBy == null)
        )
        {
            throw new ApiError.Forbidden("Deleted users cannot access this endpoint.");
        }

        if (
            authorizeAttribute.Scopes.Length > 0
            && authorizeAttribute.Scopes.Except(token.Scopes.ExpandScopes()).Any()
        )
        {
            throw new ApiError.Forbidden(
                "This endpoint requires ungranted scopes.",
                authorizeAttribute.Scopes.Except(token.Scopes.ExpandScopes()),
                ErrorCode.MissingScopes
            );
        }

        if (limitAttribute?.RequireAdmin == true && token.User.Role != UserRole.Admin)
        {
            throw new ApiError.Forbidden("This endpoint can only be used by admins.");
        }

        if (
            limitAttribute?.RequireModerator == true
            && token.User.Role is not (UserRole.Admin or UserRole.Moderator)
        )
        {
            throw new ApiError.Forbidden("This endpoint can only be used by moderators.");
        }

        await next(ctx);
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute(params string[] scopes) : Attribute
{
    public readonly string[] Scopes = scopes.Except([":admin", ":moderator", ":deleted"]).ToArray();
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class LimitAttribute : Attribute
{
    public bool UsableBySuspendedUsers { get; init; }
    public bool RequireAdmin { get; init; }
    public bool RequireModerator { get; init; }
}
