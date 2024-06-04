using System.Security.Cryptography;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace Foxnouns.Backend.Services;

public class AuthService(ILogger logger, DatabaseContext db, ISnowflakeGenerator snowflakeGenerator)
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    /// <summary>
    /// Creates a new user with the given email address and password.
    /// This method does <i>not</i> save the resulting user, the caller must still call <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" />.
    /// </summary>
    public async Task<User> CreateUserWithPasswordAsync(string username, string email, string password)
    {
        var user = new User
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            Username = username,
            AuthMethods = { new AuthMethod { Id = snowflakeGenerator.GenerateSnowflake(), AuthType = AuthType.Email, RemoteId = email } }
        };

        db.Add(user);
        user.Password = await Task.Run(() => _passwordHasher.HashPassword(user, password));

        return user;
    }

    public (string, Token) GenerateToken(User user, Application application, string[] scopes, Instant expires)
    {
        if (!OauthUtils.ValidateScopes(application, scopes))
            throw new ApiError.BadRequest("Invalid scopes requested for this token");

        var (token, hash) = GenerateToken();
        return (token, new Token
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            Hash = hash,
            Application = application,
            User = user,
            ExpiresAt = expires,
            Scopes = scopes
        });
    }

    private static (string, byte[]) GenerateToken()
    {
        var token = OauthUtils.RandomToken(48);
        var hash = SHA512.HashData(Convert.FromBase64String(token));

        return (token, hash);
    }
}