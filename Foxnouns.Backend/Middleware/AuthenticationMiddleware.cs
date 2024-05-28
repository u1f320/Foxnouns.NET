using System.Security.Cryptography;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Middleware;

public class AuthenticationMiddleware(DatabaseContext db, IClock clock) : IMiddleware
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

        var header = ctx.Request.Headers.Authorization.ToString();
        if (!OauthUtils.TryFromBase64String(header, out var rawToken))
        {
            await next(ctx);
            return;
        }

        var hash = SHA512.HashData(rawToken);
        var oauthToken = await db.Tokens
            .Include(t => t.Application)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Hash == hash && t.ExpiresAt > clock.GetCurrentInstant() && !t.ManuallyExpired);
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