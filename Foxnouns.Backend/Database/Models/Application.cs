using System.Security.Cryptography;
using Foxnouns.Backend.Utils;

namespace Foxnouns.Backend.Database.Models;

public class Application : BaseModel
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string Name { get; init; }
    public required string[] Scopes { get; init; }
    public required string[] RedirectUris { get; init; }

    public static Application Create(
        ISnowflakeGenerator snowflakeGenerator,
        string name,
        string[] scopes,
        string[] redirectUrls
    )
    {
        string clientId = RandomNumberGenerator.GetHexString(32, true);
        string clientSecret = AuthUtils.RandomToken();

        if (scopes.Except(AuthUtils.ApplicationScopes).Any())
        {
            throw new ArgumentException(
                "Invalid scopes passed to Application.Create",
                nameof(scopes)
            );
        }

        if (redirectUrls.Any(s => !AuthUtils.ValidateRedirectUri(s)))
        {
            throw new ArgumentException(
                "Invalid redirect URLs passed to Application.Create",
                nameof(redirectUrls)
            );
        }

        return new Application
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            ClientId = clientId,
            ClientSecret = clientSecret,
            Name = name,
            Scopes = scopes,
            RedirectUris = redirectUrls,
        };
    }
}
