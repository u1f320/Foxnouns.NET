using System.Security.Cryptography;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            AuthMethods =
            {
                new AuthMethod
                    { Id = snowflakeGenerator.GenerateSnowflake(), AuthType = AuthType.Email, RemoteId = email }
            }
        };

        db.Add(user);
        user.Password = await Task.Run(() => _passwordHasher.HashPassword(user, password));

        return user;
    }

    /// <summary>
    /// Creates a new user with the given username and remote authentication method.
    /// To create a user with email authentication, use <see cref="CreateUserWithPasswordAsync" />
    /// This method does <i>not</i> save the resulting user, the caller must still call <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" />.
    /// </summary>
    public async Task<User> CreateUserWithRemoteAuthAsync(string username, AuthType authType, string remoteId,
        string remoteUsername, FediverseApplication? instance = null)
    {
        AssertValidAuthType(authType, instance);

        if (await db.Users.AnyAsync(u => u.Username == username))
            throw new ApiError.BadRequest("Username is already taken", "username");

        var user = new User
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            Username = username,
            AuthMethods =
            {
                new AuthMethod
                {
                    Id = snowflakeGenerator.GenerateSnowflake(), AuthType = authType, RemoteId = remoteId,
                    RemoteUsername = remoteUsername, FediverseApplication = instance
                }
            }
        };

        db.Add(user);
        return user;
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="password">The user's password, in plain text</param>
    /// <returns>A tuple of the authenticated user and whether multi-factor authentication is required</returns>
    /// <exception cref="ApiError.NotFound">Thrown if the email address is not associated with any user
    /// or if the password is incorrect</exception>
    public async Task<(User, EmailAuthenticationResult)> AuthenticateUserAsync(string email, string password)
    {
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.AuthMethods.Any(a => a.AuthType == AuthType.Email && a.RemoteId == email));
        if (user == null)
            throw new ApiError.NotFound("No user with that email address found, or password is incorrect");

        var pwResult = await Task.Run(() => _passwordHasher.VerifyHashedPassword(user, user.Password!, password));
        if (pwResult == PasswordVerificationResult.Failed)
            throw new ApiError.NotFound("No user with that email address found, or password is incorrect");
        if (pwResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.Password = await Task.Run(() => _passwordHasher.HashPassword(user, password));
            await db.SaveChangesAsync();
        }

        return (user, EmailAuthenticationResult.AuthSuccessful);
    }

    public enum EmailAuthenticationResult
    {
        AuthSuccessful,
        MfaRequired,
    }

    /// <summary>
    /// Authenticates a user with a remote authentication provider.
    /// </summary>
    /// <param name="authType">The remote authentication provider type</param>
    /// <param name="remoteId">The remote user ID</param>
    /// <param name="instance">The Fediverse instance, if authType is Fediverse.
    /// Will throw an exception if passed with another authType.</param>
    /// <returns>A user object, or null if the remote account isn't linked to any user.</returns>
    /// <exception cref="FoxnounsError">Thrown if <c>instance</c> is passed when not required,
    /// or not passed when required</exception>
    public async Task<User?> AuthenticateUserAsync(AuthType authType, string remoteId,
        FediverseApplication? instance = null)
    {
        AssertValidAuthType(authType, instance);

        return await db.Users.FirstOrDefaultAsync(u =>
            u.AuthMethods.Any(a =>
                a.AuthType == authType && a.RemoteId == remoteId && a.FediverseApplication == instance));
    }

    public (string, Token) GenerateToken(User user, Application application, string[] scopes, Instant expires)
    {
        if (!AuthUtils.ValidateScopes(application, scopes))
            throw new ApiError.BadRequest("Invalid scopes requested for this token", "scopes");

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
        var token = AuthUtils.RandomToken(48);
        var hash = SHA512.HashData(Convert.FromBase64String(token));

        return (token, hash);
    }

    private static void AssertValidAuthType(AuthType authType, FediverseApplication? instance)
    {
        if (authType == AuthType.Fediverse && instance == null)
            throw new FoxnounsError("Fediverse authentication requires an instance.");
        if (authType != AuthType.Fediverse && instance != null)
            throw new FoxnounsError("Non-Fediverse authentication does not require an instance.");
    }
}