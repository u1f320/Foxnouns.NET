using System.Security.Cryptography;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Database;

public static class DatabaseQueryExtensions
{
    public static async Task<User> ResolveUserAsync(
        this DatabaseContext context,
        string userRef,
        Token? token,
        CancellationToken ct = default
    )
    {
        if (userRef == "@me")
        {
            return token != null
                ? await context.Users.FirstAsync(u => u.Id == token.UserId, ct)
                : throw new ApiError.Unauthorized(
                    "This endpoint requires an authenticated user.",
                    ErrorCode.AuthenticationRequired
                );
        }

        User? user;
        if (Snowflake.TryParse(userRef, out Snowflake? snowflake))
        {
            user = await context
                .Users.Where(u => !u.Deleted)
                .FirstOrDefaultAsync(u => u.Id == snowflake, ct);
            if (user != null)
                return user;
        }

        user = await context
            .Users.Where(u => !u.Deleted)
            .FirstOrDefaultAsync(u => u.Username == userRef, ct);
        if (user != null)
            return user;
        throw new ApiError.NotFound(
            "No user with that ID or username found.",
            ErrorCode.UserNotFound
        );
    }

    public static async Task<User> ResolveUserAsync(
        this DatabaseContext context,
        Snowflake id,
        CancellationToken ct = default
    )
    {
        User? user = await context
            .Users.Where(u => !u.Deleted)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user != null)
            return user;
        throw new ApiError.NotFound("No user with that ID found.", ErrorCode.UserNotFound);
    }

    public static async Task<Member> ResolveMemberAsync(
        this DatabaseContext context,
        Snowflake id,
        CancellationToken ct = default
    )
    {
        Member? member = await context
            .Members.Include(m => m.User)
            .Where(m => !m.User.Deleted)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
        if (member != null)
            return member;
        throw new ApiError.NotFound("No member with that ID found.", ErrorCode.MemberNotFound);
    }

    public static async Task<Member> ResolveMemberAsync(
        this DatabaseContext context,
        string userRef,
        string memberRef,
        Token? token,
        CancellationToken ct = default
    )
    {
        User user = await context.ResolveUserAsync(userRef, token, ct);
        return await context.ResolveMemberAsync(user.Id, memberRef, ct);
    }

    public static async Task<Member> ResolveMemberAsync(
        this DatabaseContext context,
        Snowflake userId,
        string memberRef,
        CancellationToken ct = default
    )
    {
        Member? member;
        if (Snowflake.TryParse(memberRef, out Snowflake? snowflake))
        {
            member = await context
                .Members.Include(m => m.User)
                .Include(m => m.ProfileFlags)
                .Where(m => !m.User.Deleted)
                .FirstOrDefaultAsync(m => m.Id == snowflake && m.UserId == userId, ct);
            if (member != null)
                return member;
        }

        member = await context
            .Members.Include(m => m.User)
            .Include(m => m.ProfileFlags)
            .Where(m => !m.User.Deleted)
            .FirstOrDefaultAsync(m => m.Name == memberRef && m.UserId == userId, ct);
        if (member != null)
            return member;
        throw new ApiError.NotFound(
            "No member with that ID or name found.",
            ErrorCode.MemberNotFound
        );
    }

    public static async Task<Application> GetFrontendApplicationAsync(
        this DatabaseContext context,
        CancellationToken ct = default
    )
    {
        Application? app = await context.Applications.FirstOrDefaultAsync(
            a => a.Id == new Snowflake(0),
            ct
        );
        if (app != null)
            return app;

        app = new Application
        {
            Id = new Snowflake(0),
            ClientId = RandomNumberGenerator.GetHexString(32, true),
            ClientSecret = AuthUtils.RandomToken(),
            Name = "pronouns.cc",
            Scopes = ["*"],
            RedirectUris = [],
        };

        context.Add(app);
        await context.SaveChangesAsync(ct);
        return app;
    }

    public static async Task<Token?> GetToken(
        this DatabaseContext context,
        byte[] rawToken,
        CancellationToken ct = default
    )
    {
        byte[] hash = SHA512.HashData(rawToken);

        Token? oauthToken = await context
            .Tokens.Include(t => t.Application)
            .Include(t => t.User)
            .FirstOrDefaultAsync(
                t =>
                    t.Hash == hash
                    && t.ExpiresAt > SystemClock.Instance.GetCurrentInstant()
                    && !t.ManuallyExpired,
                ct
            );

        return oauthToken;
    }

    public static async Task<Snowflake?> GetTokenUserId(
        this DatabaseContext context,
        byte[] rawToken,
        CancellationToken ct = default
    )
    {
        byte[] hash = SHA512.HashData(rawToken);
        return await context
            .Tokens.Where(t =>
                t.Hash == hash
                && t.ExpiresAt > SystemClock.Instance.GetCurrentInstant()
                && !t.ManuallyExpired
            )
            .Select(t => t.UserId)
            .FirstOrDefaultAsync(ct);
    }
}
