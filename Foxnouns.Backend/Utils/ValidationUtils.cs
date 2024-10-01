using System.Text.RegularExpressions;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;

namespace Foxnouns.Backend.Utils;

/// <summary>
/// Static methods for validating user input (mostly making sure it's not too short or too long)
/// </summary>
public static partial class ValidationUtils
{
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
        if (!UsernameRegex().IsMatch(username))
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
        if (!MemberRegex().IsMatch(memberName))
            return memberName.Length switch
            {
                < 1 => ValidationError.LengthError("Name is too short", 1, 100, memberName.Length),
                > 100 => ValidationError.LengthError("Name is too long", 1, 100, memberName.Length),
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

    private const int MaxLinks = 25;
    private const int MaxLinkLength = 256;

    public static IEnumerable<(string, ValidationError?)> ValidateLinks(string[]? links)
    {
        if (links == null) return [];
        if (links.Length > MaxLinks)
            return [("links", ValidationError.LengthError("Too many links", 0, MaxLinks, links.Length))];

        var errors = new List<(string, ValidationError?)>();
        foreach (var (link, idx) in links.Select((l, i) => (l, i)))
        {
            switch (link.Length)
            {
                case 0:
                    errors.Add(($"links.{idx}",
                        ValidationError.LengthError("Link cannot be empty", 1, 256, 0)));
                    break;
                case > MaxLinkLength:
                    errors.Add(($"links.{idx}",
                        ValidationError.LengthError("Link is too long", 1, MaxLinkLength, link.Length)));
                    break;
            }
        }

        return errors;
    }

    public const int MaxBioLength = 1024;

    public static ValidationError? ValidateBio(string? bio)
    {
        return bio?.Length switch
        {
            0 => ValidationError.LengthError("Bio is too short", 1, MaxBioLength, bio.Length),
            > MaxBioLength => ValidationError.LengthError("Bio is too long", 1, MaxBioLength, bio.Length),
            _ => null
        };
    }

    public static ValidationError? ValidateAvatar(string? avatar)
    {
        return avatar?.Length switch
        {
            0 => ValidationError.GenericValidationError("Avatar cannot be empty", null),
            > 1_500_000 => ValidationError.GenericValidationError("Avatar is too large", null),
            _ => null
        };
    }


    private static readonly string[] DefaultStatusOptions =
    [
        "favourite",
        "okay",
        "jokingly",
        "friends_only",
        "avoid"
    ];

    public static IEnumerable<(string, ValidationError?)> ValidateFields(List<Field>? fields,
        IReadOnlyDictionary<Snowflake, User.CustomPreference> customPreferences)
    {
        if (fields == null) return [];

        var errors = new List<(string, ValidationError?)>();
        if (fields.Count > 25)
            errors.Add(("fields", ValidationError.LengthError("Too many fields", 0, Limits.FieldLimit, fields.Count)));
        // No overwhelming this function, thank you
        if (fields.Count > 100) return errors;

        foreach (var (field, index) in fields.Select((field, index) => (field, index)))
        {
            switch (field.Name.Length)
            {
                case > Limits.FieldNameLimit:
                    errors.Add(($"fields.{index}.name",
                        ValidationError.LengthError("Field name is too long", 1, Limits.FieldNameLimit,
                            field.Name.Length)));
                    break;
                case < 1:
                    errors.Add(($"fields.{index}.name",
                        ValidationError.LengthError("Field name is too short", 1, Limits.FieldNameLimit,
                            field.Name.Length)));
                    break;
            }

            errors = errors.Concat(ValidateFieldEntries(field.Entries, customPreferences, $"fields.{index}.entries"))
                .ToList();
        }

        return errors;
    }

    public static IEnumerable<(string, ValidationError?)> ValidateFieldEntries(FieldEntry[]? entries,
        IReadOnlyDictionary<Snowflake, User.CustomPreference> customPreferences, string errorPrefix = "fields")
    {
        if (entries == null || entries.Length == 0) return [];
        var errors = new List<(string, ValidationError?)>();

        if (entries.Length > Limits.FieldEntriesLimit)
            errors.Add((errorPrefix,
                ValidationError.LengthError("Field has too many entries", 0, Limits.FieldEntriesLimit,
                    entries.Length)));

        // Same as above, no overwhelming this function with a ridiculous amount of entries
        if (entries.Length > Limits.FieldEntriesLimit + 50) return errors;

        foreach (var (entry, entryIdx) in entries.Select((entry, entryIdx) => (entry, entryIdx)))
        {
            switch (entry.Value.Length)
            {
                case > Limits.FieldEntryTextLimit:
                    errors.Add(($"{errorPrefix}.{entryIdx}.value",
                        ValidationError.LengthError("Field value is too long", 1, Limits.FieldEntryTextLimit,
                            entry.Value.Length)));
                    break;
                case < 1:
                    errors.Add(($"{errorPrefix}.{entryIdx}.value",
                        ValidationError.LengthError("Field value is too short", 1, Limits.FieldEntryTextLimit,
                            entry.Value.Length)));
                    break;
            }

            var customPreferenceIds = customPreferences?.Keys.Select(id => id.ToString()) ?? [];

            if (!DefaultStatusOptions.Contains(entry.Status) && !customPreferenceIds.Contains(entry.Status))
                errors.Add(($"{errorPrefix}.{entryIdx}.status",
                    ValidationError.GenericValidationError("Invalid status", entry.Status)));
        }

        return errors;
    }

    public static IEnumerable<(string, ValidationError?)> ValidatePronouns(Pronoun[]? entries,
        IReadOnlyDictionary<Snowflake, User.CustomPreference> customPreferences, string errorPrefix = "pronouns")
    {
        if (entries == null || entries.Length == 0) return [];
        var errors = new List<(string, ValidationError?)>();

        if (entries.Length > Limits.FieldEntriesLimit)
            errors.Add((errorPrefix,
                ValidationError.LengthError("Too many pronouns", 0, Limits.FieldEntriesLimit,
                    entries.Length)));

        // Same as above, no overwhelming this function with a ridiculous amount of entries
        if (entries.Length > Limits.FieldEntriesLimit + 50) return errors;

        foreach (var (entry, entryIdx) in entries.Select((entry, entryIdx) => (entry, entryIdx)))
        {
            switch (entry.Value.Length)
            {
                case > Limits.FieldEntryTextLimit:
                    errors.Add(($"{errorPrefix}.{entryIdx}.value",
                        ValidationError.LengthError("Pronoun value is too long", 1, Limits.FieldEntryTextLimit,
                            entry.Value.Length)));
                    break;
                case < 1:
                    errors.Add(($"{errorPrefix}.{entryIdx}.value",
                        ValidationError.LengthError("Pronoun value is too short", 1, Limits.FieldEntryTextLimit,
                            entry.Value.Length)));
                    break;
            }

            if (entry.DisplayText != null)
            {
                switch (entry.DisplayText.Length)
                {
                    case > Limits.FieldEntryTextLimit:
                        errors.Add(($"{errorPrefix}.{entryIdx}.value",
                            ValidationError.LengthError("Pronoun display text is too long", 1,
                                Limits.FieldEntryTextLimit,
                                entry.Value.Length)));
                        break;
                    case < 1:
                        errors.Add(($"{errorPrefix}.{entryIdx}.value",
                            ValidationError.LengthError("Pronoun display text is too short", 1,
                                Limits.FieldEntryTextLimit,
                                entry.Value.Length)));
                        break;
                }
            }

            var customPreferenceIds = customPreferences?.Keys.Select(id => id.ToString()) ?? [];

            if (!DefaultStatusOptions.Contains(entry.Status) && !customPreferenceIds.Contains(entry.Status))
                errors.Add(($"{errorPrefix}.{entryIdx}.status",
                    ValidationError.GenericValidationError("Invalid status", entry.Status)));
        }

        return errors;
    }

    [GeneratedRegex(@"^[a-zA-Z_0-9\-\.]{2,40}$", RegexOptions.IgnoreCase, "en-NL")]
    private static partial Regex UsernameRegex();

    [GeneratedRegex("""^[^@'$%&()+<=>^|~`,*!#/\\\[\]""\{\}\?]{1,100}$""", RegexOptions.IgnoreCase, "en-NL")]
    private static partial Regex MemberRegex();
}