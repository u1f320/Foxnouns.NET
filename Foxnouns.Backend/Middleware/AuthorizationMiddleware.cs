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
            throw new ApiError.Unauthorized("This endpoint requires an authenticated user.");
        if (attribute.Scopes.Length > 0 && attribute.Scopes.Except(token.Scopes.ExpandScopes()).Any())
            throw new ApiError.Forbidden("This endpoint requires ungranted scopes.",
                attribute.Scopes.Except(token.Scopes.ExpandScopes()));

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