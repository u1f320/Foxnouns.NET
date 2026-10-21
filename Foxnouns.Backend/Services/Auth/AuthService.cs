// Copyright (C) 2023-present sam/u1f320 (vulpine.solutions)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System.Security.Cryptography;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using XidNet;

namespace Foxnouns.Backend.Services.Auth;

public class AuthService(
    IClock clock,
    ILogger logger,
    DatabaseContext db,
    ISnowflakeGenerator snowflakeGenerator,
    UserRendererService userRenderer,
    ValidationService validationService
)
{
    private readonly ILogger _logger = logger.ForContext<AuthService>();
    private readonly PasswordHasher<User> _passwordHasher = new();

    /// <summary>
    /// Creates a new user with the given email address and password.
    /// This method does <i>not</i> save the resulting user, the caller must still call <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" />.
    /// </summary>
    public async Task<User> CreateUserWithPasswordAsync(
        string username,
        string email,
        string password,
        CancellationToken ct = default
    )
    {
        // Validate username and whether it's not taken
        ValidationUtils.Validate([
            ("username", validationService.ValidateUsername(username)),
            ("password", ValidationUtils.ValidatePassword(password)),
        ]);
        if (await db.Users.AnyAsync(u => u.Username == username, ct))
            throw new ApiError.BadRequest("Username is already taken", "username", username);

        var user = new User
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            Username = username,
            AuthMethods =
            {
                new AuthMethod
                {
                    Id = snowflakeGenerator.GenerateSnowflake(),
                    AuthType = AuthType.Email,
                    RemoteId = email,
                },
            },
            LastActive = clock.GetCurrentInstant(),
            Sid = null!,
            LegacyId = Xid.NewXid().ToString(),
        };

        db.Add(user);
        user.Password = await HashPasswordAsync(user, password, ct);

        return user;
    }

    /// <summary>
    /// Creates a new user with the given username and remote authentication method.
    /// To create a user with email authentication, use <see cref="CreateUserWithPasswordAsync" />
    /// This method does <i>not</i> save the resulting user, the caller must still call <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" />.
    /// </summary>
    public async Task<User> CreateUserWithRemoteAuthAsync(
        string username,
        AuthType authType,
        string remoteId,
        string remoteUsername,
        FediverseApplication? instance = null,
        CancellationToken ct = default
    )
    {
        AssertValidAuthType(authType, instance);

        // Validate username and whether it's not taken
        ValidationUtils.Validate([("username", validationService.ValidateUsername(username))]);
        if (await db.Users.AnyAsync(u => u.Username == username, ct))
            throw new ApiError.BadRequest("Username is already taken", "username", username);

        var user = new User
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            Username = username,
            AuthMethods =
            {
                new AuthMethod
                {
                    Id = snowflakeGenerator.GenerateSnowflake(),
                    AuthType = authType,
                    RemoteId = remoteId,
                    RemoteUsername = remoteUsername,
                    FediverseApplication = instance,
                },
            },
            LastActive = clock.GetCurrentInstant(),
            Sid = null!,
            LegacyId = Xid.NewXid().ToString(),
        };

        db.Add(user);
        return user;
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="password">The user's password, in plain text</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A tuple of the authenticated user and whether multi-factor authentication is required</returns>
    /// <exception cref="ApiError.NotFound">Thrown if the email address is not associated with any user
    /// or if the password is incorrect</exception>
    public async Task<(User, EmailAuthenticationResult)> AuthenticateUserAsync(
        string email,
        string password,
        CancellationToken ct = default
    )
    {
        User? user = await db.Users.FirstOrDefaultAsync(
            u => u.AuthMethods.Any(a => a.AuthType == AuthType.Email && a.RemoteId == email),
            ct
        );
        if (user == null)
        {
            throw new ApiError.NotFound(
                "No user with that email address found, or password is incorrect",
                ErrorCode.UserNotFound
            );
        }

        PasswordVerificationResult pwResult = await VerifyHashedPasswordAsync(user, password, ct);
        if (pwResult == PasswordVerificationResult.Failed) // TODO: this seems to fail on some valid passwords?
        {
            throw new ApiError.NotFound(
                "No user with that email address found, or password is incorrect",
                ErrorCode.UserNotFound
            );
        }

        if (pwResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.Password = await HashPasswordAsync(user, password, ct);
            await db.SaveChangesAsync(ct);
        }

        return (user, EmailAuthenticationResult.AuthSuccessful);
    }

    public enum EmailAuthenticationResult
    {
        AuthSuccessful,
        MfaRequired,
    }

    /// <summary>
    /// Validates a user's password outside an authentication context, for when a password is required for changing
    /// a setting, such as adding a new email address or changing passwords.
    /// </summary>
    public async Task<bool> ValidatePasswordAsync(
        User user,
        string password,
        CancellationToken ct = default
    )
    {
        if (user.Password == null)
        {
            throw new FoxnounsError("Password for user supplied to ValidatePasswordAsync was null");
        }

        PasswordVerificationResult pwResult = await VerifyHashedPasswordAsync(user, password, ct);
        return pwResult
            is PasswordVerificationResult.SuccessRehashNeeded
                or PasswordVerificationResult.Success;
    }

    /// <summary>
    /// Sets or updates a password for the given user. This method does <i>not</i> save the updated password automatically.
    /// </summary>
    public async Task SetUserPasswordAsync(
        User user,
        string password,
        CancellationToken ct = default
    )
    {
        user.Password = await HashPasswordAsync(user, password, ct);
        db.Update(user);
    }

    /// <summary>
    /// Authenticates a user with a remote authentication provider.
    /// </summary>
    /// <param name="authType">The remote authentication provider type</param>
    /// <param name="remoteId">The remote user ID</param>
    /// <param name="instance">The Fediverse instance, if authType is Fediverse.
    /// Will throw an exception if passed with another authType.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A user object, or null if the remote account isn't linked to any user.</returns>
    /// <exception cref="FoxnounsError">Thrown if <c>instance</c> is passed when not required,
    /// or not passed when required</exception>
    public async Task<User?> AuthenticateUserAsync(
        AuthType authType,
        string remoteId,
        FediverseApplication? instance = null,
        CancellationToken ct = default
    )
    {
        AssertValidAuthType(authType, instance);

        return await db.Users.FirstOrDefaultAsync(
            u =>
                u.AuthMethods.Any(a =>
                    a.AuthType == authType
                    && a.RemoteId == remoteId
                    && a.FediverseApplication == instance
                ),
            ct
        );
    }

    public async Task<AuthMethod> AddAuthMethodAsync(
        Snowflake userId,
        AuthType authType,
        string remoteId,
        string? remoteUsername = null,
        FediverseApplication? app = null,
        CancellationToken ct = default
    )
    {
        AssertValidAuthType(authType, app);

        // This is already checked when generating an add account state, but we check it here too just in case.
        int currentCount = await db
            .AuthMethods.Where(m => m.UserId == userId && m.AuthType == authType)
            .CountAsync(ct);
        if (currentCount >= AuthUtils.MaxAuthMethodsPerType)
        {
            throw new ApiError.BadRequest(
                $"Too many linked accounts of this type, maximum of {AuthUtils.MaxAuthMethodsPerType} per account."
            );
        }

        var authMethod = new AuthMethod
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            AuthType = authType,
            RemoteId = remoteId,
            FediverseApplicationId = app?.Id,
            RemoteUsername = remoteUsername,
            UserId = userId,
        };

        db.Add(authMethod);
        await db.SaveChangesAsync(ct);
        return authMethod;
    }

    public (string, Token) GenerateToken(
        User user,
        Application application,
        string[] scopes,
        Instant expires
    )
    {
        if (!AuthUtils.ValidateScopes(application, scopes))
        {
            throw new ApiError.BadRequest(
                "Invalid scopes requested for this token",
                "scopes",
                scopes
            );
        }

        (string? token, byte[]? hash) = GenerateToken();
        return (
            token,
            new Token
            {
                Id = snowflakeGenerator.GenerateSnowflake(),
                Hash = hash,
                Application = application,
                User = user,
                ExpiresAt = expires,
                Scopes = scopes,
            }
        );
    }

    /// <summary>
    /// Generates a token for the given user and adds it to the database, returning a fully formed auth response for the user.
    /// This method is always called at the end of an endpoint method, so the resulting token
    /// (and user, if this is a registration request) is also saved to the database.
    /// </summary>
    public async Task<CallbackResponse> GenerateUserTokenAsync(
        User user,
        CancellationToken ct = default
    )
    {
        Application frontendApp = await db.GetFrontendApplicationAsync(ct);

        (string? tokenStr, Token? token) = GenerateToken(
            user,
            frontendApp,
            ["*"],
            clock.GetCurrentInstant() + Duration.FromDays(365)
        );
        db.Add(token);

        _logger.Debug("Generated token {TokenId} for {UserId}", user.Id, token.Id);

        await db.SaveChangesAsync(ct);

        return new CallbackResponse(
            true,
            null,
            null,
            await userRenderer.RenderUserAsync(user, user, renderMembers: false, ct: ct),
            tokenStr,
            token.ExpiresAt
        );
    }

    private Task<string> HashPasswordAsync(
        User user,
        string password,
        CancellationToken ct = default
    ) => Task.Run(() => _passwordHasher.HashPassword(user, password), ct);

    private Task<PasswordVerificationResult> VerifyHashedPasswordAsync(
        User user,
        string providedPassword,
        CancellationToken ct = default
    ) =>
        Task.Run(
            () => _passwordHasher.VerifyHashedPassword(user, user.Password!, providedPassword),
            ct
        );

    private static (string, byte[]) GenerateToken()
    {
        string token = AuthUtils.RandomUrlUnsafeToken();
        byte[] hash = SHA512.HashData(Convert.FromBase64String(token));

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
