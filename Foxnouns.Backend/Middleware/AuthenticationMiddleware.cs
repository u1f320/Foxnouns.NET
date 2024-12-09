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
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;

namespace Foxnouns.Backend.Middleware;

public class AuthenticationMiddleware(DatabaseContext db) : IMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        Endpoint? endpoint = ctx.GetEndpoint();
        AuthenticateAttribute? metadata = endpoint?.Metadata.GetMetadata<AuthenticateAttribute>();

        if (metadata == null)
        {
            await next(ctx);
            return;
        }

        if (
            !AuthUtils.TryParseToken(
                ctx.Request.Headers.Authorization.ToString(),
                out byte[]? rawToken
            )
        )
        {
            await next(ctx);
            return;
        }

        Token? oauthToken = await db.GetToken(rawToken);
        if (oauthToken == null)
        {
            await next(ctx);
            return;
        }

        ctx.SetToken(oauthToken);
        await next(ctx);
    }
}

public static class HttpContextExtensions
{
    private const string Key = "token";

    public static void SetToken(this HttpContext ctx, Token token) => ctx.Items.Add(Key, token);

    public static User? GetUser(this HttpContext ctx) => ctx.GetToken()?.User;

    public static User GetUserOrThrow(this HttpContext ctx) =>
        ctx.GetUser() ?? throw new ApiError.AuthenticationError("No user in HttpContext");

    public static Token? GetToken(this HttpContext ctx)
    {
        if (ctx.Items.TryGetValue(Key, out object? token))
            return token as Token;
        return null;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthenticateAttribute : Attribute;
