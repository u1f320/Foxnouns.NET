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

namespace Foxnouns.Backend.Services;

public partial class ValidationService
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
        "null",
    ];

    private static readonly string[] InvalidMemberNames =
    [
        // these break routing outright
        ".",
        "..",
        // TODO: remove this? i'm not sure if /@[username]/edit will redirect to settings
        "edit",
        // this breaks the frontend, somehow
        "null",
    ];

    public ValidationError? ValidateUsername(string username)
    {
        if (!UsernameRegex().IsMatch(username))
        {
            if (username.Length < 2)
            {
                return ValidationError.LengthError(
                    "Username is too short",
                    2,
                    _limits.MaxUsernameLength,
                    username.Length
                );
            }

            if (username.Length > _limits.MaxUsernameLength)
            {
                return ValidationError.LengthError(
                    "Username is too long",
                    2,
                    _limits.MaxUsernameLength,
                    username.Length
                );
            }

            return ValidationError.GenericValidationError(
                "Username is invalid, can only contain alphanumeric characters, dashes, underscores, and periods",
                username
            );
        }

        if (
            InvalidUsernames.Any(u =>
                string.Equals(u, username, StringComparison.InvariantCultureIgnoreCase)
            )
        )
        {
            return ValidationError.GenericValidationError("Username is not allowed", username);
        }

        return null;
    }

    public ValidationError? ValidateMemberName(string memberName)
    {
        if (!MemberRegex().IsMatch(memberName))
        {
            if (memberName.Length < 1)
            {
                return ValidationError.LengthError(
                    "Name is too short",
                    1,
                    _limits.MaxMemberNameLength,
                    memberName.Length
                );
            }

            if (memberName.Length > _limits.MaxMemberNameLength)
            {
                return ValidationError.LengthError(
                    "Name is too long",
                    1,
                    _limits.MaxMemberNameLength,
                    memberName.Length
                );
            }

            return ValidationError.GenericValidationError(
                "Member name cannot contain any of the following: "
                    + " @, ?, !, #, /, \\, [, ], \", ', $, %, &, (, ), {, }, +, <, =, >, ^, |, ~, `, , "
                    + "and cannot be one or two periods",
                memberName
            );
        }

        if (
            InvalidMemberNames.Any(u =>
                string.Equals(u, memberName, StringComparison.InvariantCultureIgnoreCase)
            )
        )
        {
            return ValidationError.GenericValidationError("Name is not allowed", memberName);
        }

        return null;
    }

    public ValidationError? ValidateDisplayName(string? displayName)
    {
        if (displayName?.Length == 0)
        {
            return ValidationError.LengthError(
                "Display name is too short",
                1,
                _limits.MaxDisplayNameLength,
                displayName.Length
            );
        }

        if (displayName?.Length > _limits.MaxDisplayNameLength)
        {
            return ValidationError.LengthError(
                "Display name is too long",
                1,
                _limits.MaxDisplayNameLength,
                displayName.Length
            );
        }

        return null;
    }

    public IEnumerable<(string, ValidationError?)> ValidateLinks(string[]? links)
    {
        if (links == null)
            return [];
        if (links.Length > _limits.MaxLinks)
        {
            return
            [
                (
                    "links",
                    ValidationError.LengthError("Too many links", 0, _limits.MaxLinks, links.Length)
                ),
            ];
        }

        var errors = new List<(string, ValidationError?)>();
        foreach ((string link, int idx) in links.Select((l, i) => (l, i)))
        {
            if (link.Length == 0)
            {
                errors.Add(
                    (
                        $"links.{idx}",
                        ValidationError.LengthError(
                            "Link cannot be empty",
                            1,
                            _limits.MaxLinkLength,
                            0
                        )
                    )
                );
            }
            else if (link.Length > _limits.MaxLinkLength)
            {
                errors.Add(
                    (
                        $"links.{idx}",
                        ValidationError.LengthError(
                            "Link is too long",
                            1,
                            _limits.MaxLinkLength,
                            link.Length
                        )
                    )
                );
            }
        }

        return errors;
    }

    public ValidationError? ValidateBio(string? bio)
    {
        if (bio?.Length == 0)
        {
            return ValidationError.LengthError(
                "Bio is too short",
                1,
                _limits.MaxBioLength,
                bio.Length
            );
        }

        if (bio?.Length > _limits.MaxBioLength)
        {
            return ValidationError.LengthError(
                "Bio is too long",
                1,
                _limits.MaxBioLength,
                bio.Length
            );
        }

        return null;
    }

    public ValidationError? ValidateAvatar(string? avatar)
    {
        if (avatar?.Length == 0)
        {
            return ValidationError.GenericValidationError("Avatar cannot be empty", null);
        }

        if (avatar?.Length > _limits.MaxAvatarLength)
        {
            return ValidationError.GenericValidationError("Avatar is too large", null);
        }

        return null;
    }

    [GeneratedRegex(@"^[a-zA-Z_0-9\-\.]{2,40}$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex UsernameRegex();

    [GeneratedRegex(
        """^[^@'$%&()+<=>^|~`,*!#/\\\[\]""\{\}\?]{1,100}$""",
        RegexOptions.IgnoreCase,
        "en-US"
    )]
    private static partial Regex MemberRegex();
}
