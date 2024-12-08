// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable ClassNeverInstantiated.Global
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Newtonsoft.Json;
using NodaTime;

namespace Foxnouns.Backend.Dto;

public record CallbackResponse(
    bool HasAccount,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string? Ticket,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string? RemoteUsername,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] UserResponse? User,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string? Token,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] Instant? ExpiresAt
);

public record UrlsResponse(bool EmailEnabled, string? Discord, string? Google, string? Tumblr);

public record AuthResponse(UserResponse User, string Token, Instant ExpiresAt);

public record SingleUrlResponse(string Url);

public record AddOauthAccountResponse(
    Snowflake Id,
    [property: JsonConverter(typeof(ScreamingSnakeCaseEnumConverter))] AuthType Type,
    string RemoteId,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string? RemoteUsername
);

public record OauthRegisterRequest(string Ticket, string Username);

public record CallbackRequest(string Code, string State);

public record EmailLoginRequest(string Email, string Password);

public record EmailRegisterRequest(string Email);

public record EmailCompleteRegistrationRequest(string Ticket, string Username, string Password);

public record EmailCallbackRequest(string State);

public record EmailChangePasswordRequest(string Current, string New);

public record FediverseCallbackRequest(string Instance, string Code, string State);
