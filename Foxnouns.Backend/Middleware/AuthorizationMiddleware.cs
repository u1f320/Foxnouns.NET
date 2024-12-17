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
        AuthorizeAttribute? attribute = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>();

        if (attribute == null || attribute.Scopes.Length == 0)
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

        if (attribute.Scopes.Except(token.Scopes.ExpandScopes()).Any())
        {
            throw new ApiError.Forbidden(
                "This endpoint requires ungranted scopes.",
                attribute.Scopes.Except(token.Scopes.ExpandScopes()),
                ErrorCode.MissingScopes
            );
        }

        await next(ctx);
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute(params string[] scopes) : Attribute
{
    public readonly string[] Scopes = scopes.Except([":admin", ":moderator", ":deleted"]).ToArray();
}
