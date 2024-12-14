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

public record EmailForgotPasswordRequest(string Email);

public record EmailResetPasswordRequest(string State, string Password);

public record FediverseCallbackRequest(string Instance, string Code, string? State = null);
