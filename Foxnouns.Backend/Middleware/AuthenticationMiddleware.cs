using System.Security.Cryptography;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Hangfire.Dashboard;
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
        if (!AuthUtils.TryFromBase64String(header, out var rawToken))
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

/// <summary>
/// Authentication filter for the Hangfire dashboard. Uses the cookie created by the frontend
/// (and otherwise only read <i>by</i> the frontend) to only allow admins to use it.
/// </summary>
public class HangfireDashboardAuthorizationFilter(IServiceProvider services) : IDashboardAsyncAuthorizationFilter
{
    public async Task<bool> AuthorizeAsync(DashboardContext context)
    {
        await using var scope = services.CreateAsyncScope();

        await using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        var httpContext = context.GetHttpContext();

        if (!httpContext.Request.Cookies.TryGetValue("pronounscc-token", out var cookie)) return false;

        if (!AuthUtils.TryFromBase64String(cookie!, out var rawToken)) return false;

        var hash = SHA512.HashData(rawToken);
        var oauthToken = await db.Tokens
            .Include(t => t.Application)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Hash == hash && t.ExpiresAt > clock.GetCurrentInstant() && !t.ManuallyExpired);

        return oauthToken?.User.Role == UserRole.Admin;
    }
}