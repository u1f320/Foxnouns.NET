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

using System.Text.RegularExpressions;

namespace Foxnouns.Backend.Utils;

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
        "pronounscc",
    ];

    private static readonly string[] InvalidMemberNames =
    [
        // these break routing outright
        ".",
        "..",
        // the user edit page lives at `/@{username}/edit`, so a member named "edit" would be inaccessible
        "edit",
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
                    username
                ),
            };

        if (
            InvalidUsernames.Any(u =>
                string.Equals(u, username, StringComparison.InvariantCultureIgnoreCase)
            )
        )
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
                    "Member name cannot contain any of the following: "
                        + " @, ?, !, #, /, \\, [, ], \", ', $, %, &, (, ), {, }, +, <, =, >, ^, |, ~, `, , "
                        + "and cannot be one or two periods",
                    memberName
                ),
            };

        if (
            InvalidMemberNames.Any(u =>
                string.Equals(u, memberName, StringComparison.InvariantCultureIgnoreCase)
            )
        )
            return ValidationError.GenericValidationError("Name is not allowed", memberName);
        return null;
    }

    public static ValidationError? ValidateDisplayName(string? displayName)
    {
        return displayName?.Length switch
        {
            0 => ValidationError.LengthError(
                "Display name is too short",
                1,
                100,
                displayName.Length
            ),
            > 100 => ValidationError.LengthError(
                "Display name is too long",
                1,
                100,
                displayName.Length
            ),
            _ => null,
        };
    }

    private const int MaxLinks = 25;
    private const int MaxLinkLength = 256;

    public static IEnumerable<(string, ValidationError?)> ValidateLinks(string[]? links)
    {
        if (links == null)
            return [];
        if (links.Length > MaxLinks)
            return
            [
                ("links", ValidationError.LengthError("Too many links", 0, MaxLinks, links.Length)),
            ];

        var errors = new List<(string, ValidationError?)>();
        foreach (var (link, idx) in links.Select((l, i) => (l, i)))
        {
            switch (link.Length)
            {
                case 0:
                    errors.Add(
                        (
                            $"links.{idx}",
                            ValidationError.LengthError("Link cannot be empty", 1, 256, 0)
                        )
                    );
                    break;
                case > MaxLinkLength:
                    errors.Add(
                        (
                            $"links.{idx}",
                            ValidationError.LengthError(
                                "Link is too long",
                                1,
                                MaxLinkLength,
                                link.Length
                            )
                        )
                    );
                    break;
            }
        }

        return errors;
    }

    public const int MaxBioLength = 1024;
    public const int MaxAvatarLength = 1_500_000;

    public static ValidationError? ValidateBio(string? bio)
    {
        return bio?.Length switch
        {
            0 => ValidationError.LengthError("Bio is too short", 1, MaxBioLength, bio.Length),
            > MaxBioLength => ValidationError.LengthError(
                "Bio is too long",
                1,
                MaxBioLength,
                bio.Length
            ),
            _ => null,
        };
    }

    public static ValidationError? ValidateAvatar(string? avatar)
    {
        return avatar?.Length switch
        {
            0 => ValidationError.GenericValidationError("Avatar cannot be empty", null),
            > MaxAvatarLength => ValidationError.GenericValidationError(
                "Avatar is too large",
                null
            ),
            _ => null,
        };
    }

    [GeneratedRegex(@"^[a-zA-Z_0-9\-\.]{2,40}$", RegexOptions.IgnoreCase, "en-NL")]
    private static partial Regex UsernameRegex();

    [GeneratedRegex(
        """^[^@'$%&()+<=>^|~`,*!#/\\\[\]""\{\}\?]{1,100}$""",
        RegexOptions.IgnoreCase,
        "en-NL"
    )]
    private static partial Regex MemberRegex();
}
