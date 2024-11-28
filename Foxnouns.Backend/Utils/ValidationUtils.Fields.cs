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

using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;

namespace Foxnouns.Backend.Utils;

public static partial class ValidationUtils
{
    private static readonly string[] DefaultStatusOptions =
    [
        "favourite",
        "okay",
        "jokingly",
        "friends_only",
        "avoid",
    ];

    public static IEnumerable<(string, ValidationError?)> ValidateFields(
        List<Field>? fields,
        IReadOnlyDictionary<Snowflake, User.CustomPreference> customPreferences
    )
    {
        if (fields == null)
            return [];

        var errors = new List<(string, ValidationError?)>();
        if (fields.Count > 25)
            errors.Add(
                (
                    "fields",
                    ValidationError.LengthError(
                        "Too many fields",
                        0,
                        Limits.FieldLimit,
                        fields.Count
                    )
                )
            );
        // No overwhelming this function, thank you
        if (fields.Count > 100)
            return errors;

        foreach (var (field, index) in fields.Select((field, index) => (field, index)))
        {
            switch (field.Name.Length)
            {
                case > Limits.FieldNameLimit:
                    errors.Add(
                        (
                            $"fields.{index}.name",
                            ValidationError.LengthError(
                                "Field name is too long",
                                1,
                                Limits.FieldNameLimit,
                                field.Name.Length
                            )
                        )
                    );
                    break;
                case < 1:
                    errors.Add(
                        (
                            $"fields.{index}.name",
                            ValidationError.LengthError(
                                "Field name is too short",
                                1,
                                Limits.FieldNameLimit,
                                field.Name.Length
                            )
                        )
                    );
                    break;
            }

            errors = errors
                .Concat(
                    ValidateFieldEntries(
                        field.Entries,
                        customPreferences,
                        $"fields.{index}.entries"
                    )
                )
                .ToList();
        }

        return errors;
    }

    public static IEnumerable<(string, ValidationError?)> ValidateFieldEntries(
        FieldEntry[]? entries,
        IReadOnlyDictionary<Snowflake, User.CustomPreference> customPreferences,
        string errorPrefix = "fields"
    )
    {
        if (entries == null || entries.Length == 0)
            return [];
        var errors = new List<(string, ValidationError?)>();

        if (entries.Length > Limits.FieldEntriesLimit)
            errors.Add(
                (
                    errorPrefix,
                    ValidationError.LengthError(
                        "Field has too many entries",
                        0,
                        Limits.FieldEntriesLimit,
                        entries.Length
                    )
                )
            );

        // Same as above, no overwhelming this function with a ridiculous amount of entries
        if (entries.Length > Limits.FieldEntriesLimit + 50)
            return errors;

        foreach (var (entry, entryIdx) in entries.Select((entry, entryIdx) => (entry, entryIdx)))
        {
            switch (entry.Value.Length)
            {
                case > Limits.FieldEntryTextLimit:
                    errors.Add(
                        (
                            $"{errorPrefix}.{entryIdx}.value",
                            ValidationError.LengthError(
                                "Field value is too long",
                                1,
                                Limits.FieldEntryTextLimit,
                                entry.Value.Length
                            )
                        )
                    );
                    break;
                case < 1:
                    errors.Add(
                        (
                            $"{errorPrefix}.{entryIdx}.value",
                            ValidationError.LengthError(
                                "Field value is too short",
                                1,
                                Limits.FieldEntryTextLimit,
                                entry.Value.Length
                            )
                        )
                    );
                    break;
            }

            var customPreferenceIds = customPreferences?.Keys.Select(id => id.ToString()) ?? [];

            if (
                !DefaultStatusOptions.Contains(entry.Status)
                && !customPreferenceIds.Contains(entry.Status)
            )
                errors.Add(
                    (
                        $"{errorPrefix}.{entryIdx}.status",
                        ValidationError.GenericValidationError("Invalid status", entry.Status)
                    )
                );
        }

        return errors;
    }

    public static IEnumerable<(string, ValidationError?)> ValidatePronouns(
        Pronoun[]? entries,
        IReadOnlyDictionary<Snowflake, User.CustomPreference> customPreferences,
        string errorPrefix = "pronouns"
    )
    {
        if (entries == null || entries.Length == 0)
            return [];
        var errors = new List<(string, ValidationError?)>();

        if (entries.Length > Limits.FieldEntriesLimit)
            errors.Add(
                (
                    errorPrefix,
                    ValidationError.LengthError(
                        "Too many pronouns",
                        0,
                        Limits.FieldEntriesLimit,
                        entries.Length
                    )
                )
            );

        // Same as above, no overwhelming this function with a ridiculous amount of entries
        if (entries.Length > Limits.FieldEntriesLimit + 50)
            return errors;

        foreach (var (entry, entryIdx) in entries.Select((entry, entryIdx) => (entry, entryIdx)))
        {
            switch (entry.Value.Length)
            {
                case > Limits.FieldEntryTextLimit:
                    errors.Add(
                        (
                            $"{errorPrefix}.{entryIdx}.value",
                            ValidationError.LengthError(
                                "Pronoun value is too long",
                                1,
                                Limits.FieldEntryTextLimit,
                                entry.Value.Length
                            )
                        )
                    );
                    break;
                case < 1:
                    errors.Add(
                        (
                            $"{errorPrefix}.{entryIdx}.value",
                            ValidationError.LengthError(
                                "Pronoun value is too short",
                                1,
                                Limits.FieldEntryTextLimit,
                                entry.Value.Length
                            )
                        )
                    );
                    break;
            }

            if (entry.DisplayText != null)
            {
                switch (entry.DisplayText.Length)
                {
                    case > Limits.FieldEntryTextLimit:
                        errors.Add(
                            (
                                $"{errorPrefix}.{entryIdx}.display_text",
                                ValidationError.LengthError(
                                    "Pronoun display text is too long",
                                    1,
                                    Limits.FieldEntryTextLimit,
                                    entry.Value.Length
                                )
                            )
                        );
                        break;
                    case < 1:
                        errors.Add(
                            (
                                $"{errorPrefix}.{entryIdx}.display_text",
                                ValidationError.LengthError(
                                    "Pronoun display text is too short",
                                    1,
                                    Limits.FieldEntryTextLimit,
                                    entry.Value.Length
                                )
                            )
                        );
                        break;
                }
            }

            var customPreferenceIds = customPreferences?.Keys.Select(id => id.ToString()) ?? [];

            if (
                !DefaultStatusOptions.Contains(entry.Status)
                && !customPreferenceIds.Contains(entry.Status)
            )
                errors.Add(
                    (
                        $"{errorPrefix}.{entryIdx}.status",
                        ValidationError.GenericValidationError("Invalid status", entry.Status)
                    )
                );
        }

        return errors;
    }
}
