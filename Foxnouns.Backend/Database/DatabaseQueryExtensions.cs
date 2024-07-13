using System.Security.Cryptography;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Database;

public static class DatabaseQueryExtensions
{
    public static async Task<User> ResolveUserAsync(this DatabaseContext context, string userRef)
    {
        User? user;
        if (Snowflake.TryParse(userRef, out var snowflake))
        {
            user = await context.Users
                .Where(u => !u.Deleted)
                .FirstOrDefaultAsync(u => u.Id == snowflake);
            if (user != null) return user;
        }

        user = await context.Users
            .Where(u => !u.Deleted)
            .FirstOrDefaultAsync(u => u.Username == userRef);
        if (user != null) return user;
        throw new ApiError.NotFound("No user with that ID or username found.", code: ErrorCode.UserNotFound);
    }

    public static async Task<User> ResolveUserAsync(this DatabaseContext context, Snowflake id)
    {
        var user = await context.Users
            .Where(u => !u.Deleted)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user != null) return user;
        throw new ApiError.NotFound("No user with that ID found.", code: ErrorCode.UserNotFound);
    }

    public static async Task<Member> ResolveMemberAsync(this DatabaseContext context, Snowflake id)
    {
        var member = await context.Members
            .Include(m => m.User)
            .Where(m => !m.User.Deleted)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (member != null) return member;
        throw new ApiError.NotFound("No member with that ID found.", code: ErrorCode.MemberNotFound);
    }

    public static async Task<Member> ResolveMemberAsync(this DatabaseContext context, string userRef, string memberRef)
    {
        var user = await context.ResolveUserAsync(userRef);
        return await context.ResolveMemberAsync(user.Id, memberRef);
    }

    public static async Task<Member> ResolveMemberAsync(this DatabaseContext context, Snowflake userId,
        string memberRef)
    {
        Member? member;
        if (Snowflake.TryParse(memberRef, out var snowflake))
        {
            member = await context.Members
                .Include(m => m.User)
                .Where(m => !m.User.Deleted)
                .FirstOrDefaultAsync(m => m.Id == snowflake && m.UserId == userId);
            if (member != null) return member;
        }

        member = await context.Members
            .Include(m => m.User)
            .Where(m => !m.User.Deleted)
            .FirstOrDefaultAsync(m => m.Name == memberRef && m.UserId == userId);
        if (member != null) return member;
        throw new ApiError.NotFound("No member with that ID or name found.", code: ErrorCode.MemberNotFound);
    }

    public static async Task<Application> GetFrontendApplicationAsync(this DatabaseContext context)
    {
        var app = await context.Applications.FirstOrDefaultAsync(a => a.Id == new Snowflake(0));
        if (app != null) return app;

        app = new Application
        {
            Id = new Snowflake(0),
            ClientId = RandomNumberGenerator.GetHexString(32, true),
            ClientSecret = AuthUtils.RandomToken(48),
            Name = "pronouns.cc",
            Scopes = ["*"],
            RedirectUris = [],
        };

        context.Add(app);
        await context.SaveChangesAsync();
        return app;
    }

    public static Task SetKeyAsync(this DatabaseContext context, string key, string value, Duration expireAfter) =>
        context.SetKeyAsync(key, value, SystemClock.Instance.GetCurrentInstant() + expireAfter);

    public static async Task SetKeyAsync(this DatabaseContext context, string key, string value, Instant expires)
    {
        context.TemporaryKeys.Add(new TemporaryKey
        {
            Expires = expires,
            Key = key,
            Value = value,
        });
        await context.SaveChangesAsync();
    }

    public static async Task<string?> GetKeyAsync(this DatabaseContext context, string key,
        bool delete = false)
    {
        var value = await context.TemporaryKeys.FirstOrDefaultAsync(k => k.Key == key);
        if (value == null) return null;

        if (delete)
        {
            await context.TemporaryKeys.Where(k => k.Key == key).ExecuteDeleteAsync();
            await context.SaveChangesAsync();
        }

        return value.Value;
    }
}