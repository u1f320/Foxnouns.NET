using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;

namespace Foxnouns.Backend.Middleware;

public class AuthorizationMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        var endpoint = ctx.GetEndpoint();
        var attribute = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>();

        if (attribute == null)
        {
            await next(ctx);
            return;
        }

        var token = ctx.GetToken();
        if (token == null)
            throw new ApiError.Unauthorized("This endpoint requires an authenticated user.",
                ErrorCode.AuthenticationRequired);
        if (attribute.Scopes.Length > 0 && attribute.Scopes.Except(token.Scopes.ExpandScopes()).Any())
            throw new ApiError.Forbidden("This endpoint requires ungranted scopes.",
                attribute.Scopes.Except(token.Scopes.ExpandScopes()), ErrorCode.MissingScopes);
        if (attribute.RequireAdmin && token.User.Role != UserRole.Admin)
            throw new ApiError.Forbidden("This endpoint can only be used by admins.");
        if (attribute.RequireModerator && token.User.Role != UserRole.Admin && token.User.Role != UserRole.Moderator)
            throw new ApiError.Forbidden("This endpoint can only be used by moderators.");

        await next(ctx);
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute(params string[] scopes) : Attribute
{
    public readonly bool RequireAdmin = scopes.Contains(":admin");
    public readonly bool RequireModerator = scopes.Contains(":moderator");

    public readonly string[] Scopes = scopes.Except([":admin", ":moderator"]).ToArray();
}