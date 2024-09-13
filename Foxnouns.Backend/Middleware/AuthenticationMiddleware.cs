using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;

namespace Foxnouns.Backend.Middleware;

public class AuthenticationMiddleware(DatabaseContext db) : IMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        var endpoint = ctx.GetEndpoint();
        var metadata = endpoint?.Metadata.GetMetadata<AuthenticateAttribute>();

        if (metadata == null)
        {
            await next(ctx);
            return;
        }

        if (!AuthUtils.TryParseToken(ctx.Request.Headers.Authorization.ToString(), out var rawToken))
        {
            await next(ctx);
            return;
        }

        var oauthToken = await db.GetToken(rawToken);
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
        if (ctx.Items.TryGetValue(Key, out var token))
            return token as Token;
        return null;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthenticateAttribute : Attribute;