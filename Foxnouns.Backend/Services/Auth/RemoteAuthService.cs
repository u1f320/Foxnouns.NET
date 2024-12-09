using System.Diagnostics.CodeAnalysis;
using System.Web;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Utils;
using Humanizer;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Services.Auth;

public partial class RemoteAuthService(
    Config config,
    ILogger logger,
    DatabaseContext db,
    KeyCacheService keyCacheService
)
{
    private readonly ILogger _logger = logger.ForContext<RemoteAuthService>();
    private readonly HttpClient _httpClient = new();

    public record RemoteUser(string Id, string Username);

    /// <summary>
    /// Validates whether a user can still add a new account of the given AuthType, and throws an error if they can't.
    /// </summary>
    /// <param name="userId">The user to check.</param>
    /// <param name="authType">The auth type to check.</param>
    /// <param name="instance">The optional fediverse instance to generate a state for.</param>
    /// <returns>A state for the given auth type and user ID.</returns>
    /// <exception cref="ApiError.BadRequest">The given user can't add another account of this type.
    /// This exception should not be caught by controller code.</exception>
    public async Task<string> ValidateAddAccountRequestAsync(
        Snowflake userId,
        AuthType authType,
        string? instance = null
    )
    {
        int existingAccounts = await db
            .AuthMethods.Where(m => m.UserId == userId && m.AuthType == authType)
            .CountAsync();
        if (existingAccounts > AuthUtils.MaxAuthMethodsPerType)
        {
            throw new ApiError.BadRequest(
                $"Too many linked {authType.Humanize()} accounts, maximum of {AuthUtils.MaxAuthMethodsPerType} per account."
            );
        }

        return HttpUtility.UrlEncode(
            await keyCacheService.GenerateAddExtraAccountStateAsync(authType, userId, instance)
        );
    }

    /// <summary>
    /// Checks whether the given state is correct for the given user/auth type combination.
    /// </summary>
    /// <exception cref="ApiError.BadRequest">The state doesn't match.
    /// This exception should not be caught by controller code.</exception>
    public async Task ValidateAddAccountStateAsync(
        string state,
        Snowflake userId,
        AuthType authType,
        string? instance = null
    )
    {
        AddExtraAccountState? accountState = await keyCacheService.GetAddExtraAccountStateAsync(
            state
        );
        if (
            accountState == null
            || accountState.AuthType != authType
            || accountState.UserId != userId
            || (instance != null && accountState.Instance != instance)
        )
        {
            throw new ApiError.BadRequest("Invalid state", "state", state);
        }
    }
}
