using System.Text.RegularExpressions;

namespace Foxnouns.Backend.Utils;

/// <summary>
/// Static methods for validating user input (mostly making sure it's not too short or too long)
/// </summary>
public static class ValidationUtils
{
    private static readonly Regex UsernameRegex = new("^[\\w-.]{2,40}$", RegexOptions.IgnoreCase);

    private static readonly string[] InvalidUsernames =
    [
        "..",
        "admin",
        "administrator",
        "mod",
        "moderator",
        "api",
        "page",
        "pronouns",
        "settings",
        "pronouns.cc",
        "pronounscc"
    ];

    /// <summary>
    /// Validates whether a username is valid. If it is not valid, throws <see cref="Foxnouns.Backend.ApiError" />.
    /// This does not check if the username is already taken.
    /// </summary>
    public static void ValidateUsername(string username)
    {
        if (!UsernameRegex.IsMatch(username))
            throw username.Length switch
            {
                < 2 => new ApiError.BadRequest("Username is too short", "username"),
                > 40 => new ApiError.BadRequest("Username is too long", "username"),
                _ => new ApiError.BadRequest(
                    "Username is invalid, can only contain alphanumeric characters, dashes, underscores, and periods",
                    "username")
            };

        if (InvalidUsernames.Any(u => string.Equals(u, username, StringComparison.InvariantCultureIgnoreCase)))
            throw new ApiError.BadRequest("Username is not allowed", "username");
    }

    public static void ValidateDisplayName(string? displayName)
    {
        if (displayName == null) return;
        switch (displayName.Length)
        {
            case 0:
                throw new ApiError.BadRequest("Display name is too short", "display_name");
            case > 100:
                throw new ApiError.BadRequest("Display name is too long", "display_name");
        }
    }

    public static void ValidateBio(string? bio)
    {
        if (bio == null) return;
        switch (bio.Length)
        {
            case 0:
                throw new ApiError.BadRequest("Bio is too short", "bio");
            case > 1024:
                throw new ApiError.BadRequest("Bio is too long", "bio");
        }
    }

    public static void ValidateAvatar(string? avatar)
    {
        if (avatar == null) return;
        if (avatar.Length > 1_500_000) throw new ApiError.BadRequest("Avatar is too big", "avatar");
    }
}