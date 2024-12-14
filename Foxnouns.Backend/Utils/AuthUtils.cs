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
using Foxnouns.Backend.Database.Models;

namespace Foxnouns.Backend.Utils;

public static class AuthUtils
{
    public const string ClientCredentials = "client_credentials";
    public const string AuthorizationCode = "authorization_code";
    private static readonly string[] ForbiddenSchemes =
    [
        "javascript",
        "file",
        "data",
        "mailto",
        "tel",
    ];

    public static readonly string[] UserScopes =
    [
        "user.read_hidden",
        "user.read_privileged",
        "user.update",
        "user.read_flags",
        "user.create_flags",
        "user.update_flags",
    ];

    public static readonly string[] MemberScopes =
    [
        "member.read",
        "member.update",
        "member.create",
    ];

    /// <summary>
    /// All scopes endpoints can be secured by. This does *not* include the catch-all token scopes.
    /// </summary>
    public static readonly string[] Scopes = ["identify", .. UserScopes, .. MemberScopes];

    /// <summary>
    /// All scopes that can be granted to applications and tokens. Includes the catch-all token scopes,
    /// except for "*" which is only granted to the frontend.
    /// </summary>
    public static readonly string[] ApplicationScopes = [.. Scopes, "user", "member"];

    public static string[] ExpandScopes(this string[] scopes)
    {
        if (scopes.Contains("*"))
            return ["*", .. Scopes];
        List<string> expandedScopes = ["identify"];
        if (scopes.Contains("user"))
            expandedScopes.AddRange(UserScopes);
        if (scopes.Contains("member"))
            expandedScopes.AddRange(MemberScopes);

        return expandedScopes.ToArray();
    }

    public static bool HasScope(this Token? token, string scope) =>
        token?.Scopes.ExpandScopes().Contains(scope) == true;

    private static string[] ExpandAppScopes(this string[] scopes)
    {
        var expandedScopes = scopes.ExpandScopes().ToList();
        if (scopes.Contains("user"))
            expandedScopes.Add("user");
        if (scopes.Contains("member"))
            expandedScopes.Add("member");
        return expandedScopes.ToArray();
    }

    public static bool ValidateScopes(Application application, string[] scopes)
    {
        string[] expandedScopes = scopes.ExpandScopes();
        string[] appScopes = application.Scopes.ExpandAppScopes();
        return !expandedScopes.Except(appScopes).Any();
    }

    public static bool ValidateRedirectUri(string uri)
    {
        try
        {
            string scheme = new Uri(uri).Scheme;
            return !ForbiddenSchemes.Contains(scheme);
        }
        catch
        {
            return false;
        }
    }

    public static bool TryFromBase64String(string b64, out byte[] bytes)
    {
        try
        {
            bytes = Convert.FromBase64String(b64);
            return true;
        }
        catch
        {
            bytes = [];
            return false;
        }
    }

    public static bool TryParseToken(string? input, out byte[] rawToken)
    {
        rawToken = [];

        if (string.IsNullOrWhiteSpace(input))
            return false;
        if (input.StartsWith("bearer ", StringComparison.InvariantCultureIgnoreCase))
            input = input["bearer ".Length..];

        return TryFromBase64String(input, out rawToken);
    }

    public static string RandomToken(int bytes = 48) =>
        Convert
            .ToBase64String(RandomNumberGenerator.GetBytes(bytes))
            .Trim('=')
            // Make the token URL-safe
            .Replace('+', '-')
            .Replace('/', '_');

    public const int MaxAuthMethodsPerType = 3; // Maximum of 3 Discord accounts, 3 emails, etc
}
