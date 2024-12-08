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

public class RemoteAuthService(
    Config config,
    ILogger logger,
    DatabaseContext db,
    KeyCacheService keyCacheService
)
{
    private readonly ILogger _logger = logger.ForContext<RemoteAuthService>();
    private readonly HttpClient _httpClient = new();

    private readonly Uri _discordTokenUri = new("https://discord.com/api/oauth2/token");
    private readonly Uri _discordUserUri = new("https://discord.com/api/v10/users/@me");

    public async Task<RemoteUser> RequestDiscordTokenAsync(
        string code,
        CancellationToken ct = default
    )
    {
        var redirectUri = $"{config.BaseUrl}/auth/callback/discord";
        HttpResponseMessage resp = await _httpClient.PostAsync(
            _discordTokenUri,
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "client_id", config.DiscordAuth.ClientId! },
                    { "client_secret", config.DiscordAuth.ClientSecret! },
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", redirectUri },
                }
            ),
            ct
        );
        if (!resp.IsSuccessStatusCode)
        {
            string respBody = await resp.Content.ReadAsStringAsync(ct);
            _logger.Error(
                "Received error status {StatusCode} when exchanging OAuth token: {ErrorBody}",
                (int)resp.StatusCode,
                respBody
            );
            throw new FoxnounsError("Invalid Discord OAuth response");
        }

        resp.EnsureSuccessStatusCode();
        DiscordTokenResponse? token = await resp.Content.ReadFromJsonAsync<DiscordTokenResponse>(
            ct
        );
        if (token == null)
            throw new FoxnounsError("Discord token response was null");

        var req = new HttpRequestMessage(HttpMethod.Get, _discordUserUri);
        req.Headers.Add("Authorization", $"{token.token_type} {token.access_token}");

        HttpResponseMessage resp2 = await _httpClient.SendAsync(req, ct);
        resp2.EnsureSuccessStatusCode();
        DiscordUserResponse? user = await resp2.Content.ReadFromJsonAsync<DiscordUserResponse>(ct);
        if (user == null)
            throw new FoxnounsError("Discord user response was null");

        return new RemoteUser(user.id, user.username);
    }

    [SuppressMessage(
        "ReSharper",
        "InconsistentNaming",
        Justification = "Easier to use snake_case here, rather than passing in JSON converter options"
    )]
    [UsedImplicitly]
    private record DiscordTokenResponse(string access_token, string token_type);

    [SuppressMessage(
        "ReSharper",
        "InconsistentNaming",
        Justification = "Easier to use snake_case here, rather than passing in JSON converter options"
    )]
    [UsedImplicitly]
    private record DiscordUserResponse(string id, string username);

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
