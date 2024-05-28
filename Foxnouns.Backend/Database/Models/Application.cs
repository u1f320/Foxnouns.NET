using System.Security.Cryptography;
using Foxnouns.Backend.Utils;

namespace Foxnouns.Backend.Database.Models;

public class Application : BaseModel
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string Name { get; init; }
    public required string[] Scopes { get; init; }
    public required string[] RedirectUris { get; set; }

    public static Application Create(ISnowflakeGenerator snowflakeGenerator, string name, string[] scopes,
        string[] redirectUrls)
    {
        var clientId = RandomNumberGenerator.GetHexString(32, true);
        var clientSecret = OauthUtils.RandomToken(48);

        if (scopes.Except(OauthUtils.ApplicationScopes).Any())
        {
            throw new ArgumentException("Invalid scopes passed to Application.Create", nameof(scopes));
        }

        if (redirectUrls.Any(s => !OauthUtils.ValidateRedirectUri(s)))
        {
            throw new ArgumentException("Invalid redirect URLs passed to Application.Create", nameof(redirectUrls));
        }

        return new Application
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            ClientId = clientId,
            ClientSecret = clientSecret,
            Name = name,
            Scopes = scopes,
            RedirectUris = redirectUrls
        };
    }
}