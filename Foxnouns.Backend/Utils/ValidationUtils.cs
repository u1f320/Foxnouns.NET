using System.Text.RegularExpressions;

namespace Foxnouns.Backend.Utils;

/// <summary>
/// Static methods for validating user input (mostly making sure it's not too short or too long)
/// </summary>
public static class ValidationUtils
{
    private static readonly Regex UsernameRegex = new("^[\\w-.]{2,40}$", RegexOptions.IgnoreCase);

    private static readonly Regex MemberRegex =
        new("^[^@\\?!#/\\\\[\\]\"\\{\\}'$%&()+<=>^|~`,\\*]{1,100}$", RegexOptions.IgnoreCase);

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

    private static readonly string[] InvalidMemberNames =
    [
        // these break routing outright
        ".",
        "..",
        // the user edit page lives at `/@{username}/edit`, so a member named "edit" would be inaccessible
        "edit"
    ];

    public static ValidationError? ValidateUsername(string username)
    {
        if (!UsernameRegex.IsMatch(username))
            return username.Length switch
            {
                < 2 => ValidationError.LengthError("Username is too short", 2, 40, username.Length),
                > 40 => ValidationError.LengthError("Username is too long", 2, 40, username.Length),
                _ => ValidationError.GenericValidationError(
                    "Username is invalid, can only contain alphanumeric characters, dashes, underscores, and periods",
                    username)
            };

        if (InvalidUsernames.Any(u => string.Equals(u, username, StringComparison.InvariantCultureIgnoreCase)))
            return ValidationError.GenericValidationError("Username is not allowed", username);
        return null;
    }

    public static ValidationError? ValidateMemberName(string memberName)
    {
        if (!UsernameRegex.IsMatch(memberName))
            return memberName.Length switch
            {
                < 2 => ValidationError.LengthError("Name is too short", 1, 100, memberName.Length),
                > 40 => ValidationError.LengthError("Name is too long", 1, 100, memberName.Length),
                _ => ValidationError.GenericValidationError(
                    "Member name cannot contain any of the following: " +
                    " @, ?, !, #, /, \\, [, ], \", ', $, %, &, (, ), {, }, +, <, =, >, ^, |, ~, `, , " +
                    "and cannot be one or two periods",
                    memberName)
            };

        if (InvalidMemberNames.Any(u => string.Equals(u, memberName, StringComparison.InvariantCultureIgnoreCase)))
            return ValidationError.GenericValidationError("Name is not allowed", memberName);
        return null;
    }

    public static void Validate(IEnumerable<(string, ValidationError?)> errors)
    {
        errors = errors.Where(e => e.Item2 != null).ToList();
        if (!errors.Any()) return;

        var errorDict = new Dictionary<string, IEnumerable<ValidationError>>();
        foreach (var error in errors)
        {
            if (errorDict.TryGetValue(error.Item1, out var value)) errorDict[error.Item1] = value.Append(error.Item2!);
            errorDict.Add(error.Item1, [error.Item2!]);
        }

        throw new ApiError.BadRequest("Error validating input", errorDict);
    }

    public static ValidationError? ValidateDisplayName(string? displayName)
    {
        return displayName?.Length switch
        {
            0 => ValidationError.LengthError("Display name is too short", 1, 100, displayName.Length),
            > 100 => ValidationError.LengthError("Display name is too long", 1, 100, displayName.Length),
            _ => null
        };
    }

    public static ValidationError? ValidateBio(string? bio)
    {
        return bio?.Length switch
        {
            0 => ValidationError.LengthError("Bio is too short", 1, 1024, bio.Length),
            > 1024 => ValidationError.LengthError("Bio is too long", 1, 1024, bio.Length),
            _ => null
        };
    }

    public static ValidationError? ValidateAvatar(string? avatar)
    {
        return avatar?.Length switch
        {
            0 => ValidationError.GenericValidationError("Avatar cannot be empty", null),
            > 1_500_00 => ValidationError.GenericValidationError("Avatar is too large", null),
            _ => null
        };
    }
}