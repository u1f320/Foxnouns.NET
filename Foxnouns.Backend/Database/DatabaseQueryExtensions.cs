using System.Security.Cryptography;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Database;

public static class DatabaseQueryExtensions
{
    public static async Task<User> ResolveUserAsync(this DatabaseContext context, string userRef)
    {
        User? user;
        if (Snowflake.TryParse(userRef, out var snowflake))
        {
            user = await context.Users
                .Include(u => u.Members)
                .FirstOrDefaultAsync(u => u.Id == snowflake);
            if (user != null) return user;
        }

        user = await context.Users
            .Include(u => u.Members)
            .FirstOrDefaultAsync(u => u.Username == userRef);
        if (user != null) return user;
        throw new FoxnounsError.UnknownEntityError(typeof(User));
    }

    public static async Task<Application> GetFrontendApplicationAsync(this DatabaseContext context)
    {
        var app = await context.Applications.FirstOrDefaultAsync(a => a.Id == new Snowflake(0));
        if (app != null) return app;

        app = new Application
        {
            Id = new Snowflake(0),
            ClientId = RandomNumberGenerator.GetHexString(32, true),
            ClientSecret = OauthUtils.RandomToken(48),
            Name = "pronouns.cc",
            Scopes = ["*"],
            RedirectUris = [],
        };

        context.Add(app);
        await context.SaveChangesAsync();
        return app;
    }
}