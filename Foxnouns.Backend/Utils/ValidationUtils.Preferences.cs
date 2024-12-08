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

using Foxnouns.Backend.Controllers;

namespace Foxnouns.Backend.Utils;

public static partial class ValidationUtils
{
    public const int MaxCustomPreferences = 25;
    public const int MaxPreferenceTooltipLength = 128;

    public static List<(string, ValidationError?)> ValidateCustomPreferences(
        List<UsersController.CustomPreferenceUpdate> preferences
    )
    {
        var errors = new List<(string, ValidationError?)>();

        if (preferences.Count > MaxCustomPreferences)
        {
            errors.Add(
                (
                    "custom_preferences",
                    ValidationError.LengthError(
                        "Too many custom preferences",
                        0,
                        MaxCustomPreferences,
                        preferences.Count
                    )
                )
            );
        }

        if (preferences.Count > 50)
            return errors;

        foreach (
            (UsersController.CustomPreferenceUpdate? p, int i) in preferences.Select(
                (p, i) => (p, i)
            )
        )
        {
            if (!BootstrapIcons.IsValid(p.Icon))
            {
                errors.Add(
                    (
                        $"custom_preferences.{i}.icon",
                        ValidationError.DisallowedValueError("Invalid icon name", [], p.Icon)
                    )
                );
            }

            if (p.Tooltip.Length is 1 or > MaxPreferenceTooltipLength)
            {
                errors.Add(
                    (
                        $"custom_preferences.{i}.tooltip",
                        ValidationError.LengthError(
                            "Tooltip is too short or too long",
                            1,
                            MaxPreferenceTooltipLength,
                            p.Tooltip.Length
                        )
                    )
                );
            }
        }

        return errors;
    }
}
